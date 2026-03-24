using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using TravelWeb_API.Models.Itinerary.DBContext;
using TravelWeb_API.Models.Itinerary.DBModel;
using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class AIItineraryService : IAIItineraryService
    {
        private readonly IAIService _aiService;
        private readonly TravelContext _context;
        public AIItineraryService(IAIService aIService, TravelContext travelContext)
        {
            _aiService = aIService;
            _context = travelContext;
        }
        private DateTime CombineDateAndTime(DateTime startBase, int dayNumber, string timeStr)
        {
            // 1. 計算該行程是哪一天 (第一天就是加 0 天)
            var targetDate = startBase.Date.AddDays(dayNumber - 1);

            // 2. 解析 AI 回傳的 "HH:mm" 字串
            if (TimeSpan.TryParse(timeStr, out var time))
            {
                return targetDate.Add(time);
            }

            // 3. 防呆：若 AI 格式錯誤，回傳當天零點
            return targetDate;
        }
        public async Task<int> GenerateNewItineraryAsync(ItineraryCreateDto dto, List<int> selectedPoiIds, int days, string memberid)
        {
            // 【前因 1】：AI 需要知道景點的座標與營業時間才能排動線，所以要先從 DB 撈詳細資料
            var pois = await _context.Attractions
                .Where(a => selectedPoiIds.Contains(a.AttractionId))
                .Select(a => new PoiInfoForAi
                {
                    AttractionId = a.AttractionId,
                    Name = a.Name,
                    Latitude = a.Latitude ?? 0,
                    Longitude = a.Longitude ?? 0,
                    BusinessHours = a.BusinessHours,
                    MustVisit = true // 告訴 AI 這些都是必去
                }).ToListAsync();

            var inputContext = new AiInputContext
            {
                TotalDays = days,
                Destination = dto.ItineraryName,
                AttractionPool = pois
            };

            // 【前因 2】：呼叫 AI 取得規劃好的 JSON
            var jsonResult = await _aiService.CallAiAsync(TravelPrompts.CreateItinerarySystemPrompt, inputContext);
            // 設定：忽略大小寫差異
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            if (string.IsNullOrEmpty(jsonResult)) throw new Exception("AI 未回傳任何內容");
            // 使用設定進行轉型
            var result = JsonSerializer.Deserialize<AiItineraryResult>(jsonResult, options);
            if (result?.Itinerary == null || !result.Itinerary.Any())
            {
                // 如果行程是空的，把原因寫進版本備註，方便你從 DB 看到底發生什麼事
                var reason = result?.UnplacedPois?.FirstOrDefault()?.Reason ?? "AI 判定無法生成可行行程";
                throw new Exception($"AI 規劃失敗：{reason}");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. 建立行程主表
                var newItinerary = new DBModel.Itinerary
                {
                    MemberId = memberid,
                    ItineraryName = dto.ItineraryName,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    CreateTime = DateTime.Now,
                    CurrentStatus = "Active"
                };
                _context.Itineraries.Add(newItinerary);
                await _context.SaveChangesAsync();

                // 2. 建立版本
                var aiVersion = new ItineraryVersion
                {
                    ItineraryId = newItinerary.ItineraryId,
                    VersionNumber = 1,
                    Creator = 1, // 這裡建議從 MemberId 轉換
                    CreateTime = DateTime.Now,
                    // 【後果 1】：將 AI 的「規劃策略」與「遺漏景點」序列化存入備註，方便以後對比
                    VersionRemark = $"[AI策略] {result.Summary.VersionStrategy} | 未排入: {JsonSerializer.Serialize(result.UnplacedPois)}",
                    Source = "AI",
                    CurrentUsageStatus = "Y"
                };
                _context.ItineraryVersions.Add(aiVersion);
                await _context.SaveChangesAsync();

                // 3. 處理行程項目 (巢狀迴圈：天 -> 活動)
                foreach (var day in result.Itinerary)
                {
                    int currentOrder = 100;
                    foreach (var item in day.Schedule)
                    {
                        _context.ItineraryItems.Add(new ItineraryItem
                        {
                            VersionId = aiVersion.VersionId,
                            // 【後果 2】：如果是 AI 生成的通用項 (如午餐)，PoiId 為 null 是正常的
                            AttractionId = item.PoiId,
                            DayNumber = day.Day,
                            SortOrder = currentOrder,
                            ContentDescription = item.Details,
                            // 【後果 3】：轉換為絕對時間
                            StartTime = CombineDateAndTime((DateTime)dto.StartTime, day.Day, item.Start),
                            EndTime = CombineDateAndTime((DateTime)dto.StartTime, day.Day, item.End),
                            ActivityId = item.Type
                        });
                        currentOrder += 100;
                    }
                }

                // 4. 存入 AI 分析報告
                _context.Aianalyses.Add(new Aianalysis
                {
                    VersionId = aiVersion.VersionId,
                    FeasibilityScore = result.Summary.AnalysisMetrics.OverallFeasibility,
                    FatigueIndex = result.Summary.AnalysisMetrics.OverallFatigue,
                    AnalysisTime = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return newItinerary.ItineraryId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // 建議在此 log 錯誤原因
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
        public async Task<Metrics> AnalyzeItineraryAsync(List<ItineraryItem> currentItems)
        {
            // 僅呼叫「分析提示詞」，不變動順序
            var jsonResult = await _aiService.CallAiAsync(TravelPrompts.AnalysisPrompt, currentItems);
            var result = JsonSerializer.Deserialize<AiItineraryResult>(jsonResult);
            return result.Summary.AnalysisMetrics;
        }
    }
}
