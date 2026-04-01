using System.Text.Json;
using TravelWeb_API.Models.Itinerary.DBModel;
using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class ItineraryAnalysisService : IItineraryAnalysisService
    {

        private readonly IAnalysisRepository _Repo;

        private readonly IAIService _ai;

        public ItineraryAnalysisService(
            IAnalysisRepository analysisRepo,
            IAIService ai)
        {
            _Repo = analysisRepo;
            _ai = ai;
        }
        // ① 取得 or 建立 AI 分析
        public async Task<AnalysisMetricsDto> GetOrCreateAnalysisAsync(
            int itineraryId, int versionId)
        {
            // 先查有無快取（AIAnalyses table）
            var cached = await _Repo.GetAnalysisByVersionIdAsync(versionId);
            if (cached != null) return MapToDto(cached, reason: null);

            // 無快取 → 撈版本與行程項目
            var version = await _Repo.GetVersionWithItemsAndAnalysisAsync(itineraryId, versionId);
            var itemsForAi = version.ItineraryItems
    .OrderBy(i => i.DayNumber).ThenBy(i => i.SortOrder)
    .Select(i => new
    {
        dayNumber = i.DayNumber,
        sortOrder = i.SortOrder,
        attractionName = i.Attraction.Name ?? i.ContentDescription
                      ?? string.Empty,
        startTime = i.StartTime.HasValue ? i.StartTime.Value.ToString("HH:mm") : string.Empty,
        endTime = i.EndTime.HasValue ? i.EndTime.Value.ToString("HH:mm") : string.Empty,
    })
    .ToList();
            var rawJson = await _ai.CallAiAsync(TravelPrompts.AnalysisPrompt, itemsForAi);
            var aiOutput = ParseAiOutput(rawJson);

            // 調和平均：Feasibility 高好、Fatigue 低好 → 反轉後取平均
            var pace = Math.Round(
                (aiOutput.OverallAnalysis.Feasibility + (1 - aiOutput.OverallAnalysis.Fatigue)) / 2.0, 2);

            // 存入 AIAnalyses
            var entity = await _Repo.CreateAnalysisAsync(new Aianalysis
            {
                VersionId = versionId,
                FeasibilityScore = (decimal)aiOutput.OverallAnalysis.Feasibility,
                PaceBalanceScore = (decimal)pace,
                FatigueIndex = (decimal)aiOutput.OverallAnalysis.Fatigue,
                AnalysisTime = DateTime.UtcNow,
                Reason = aiOutput.OverallAnalysis.Reason
            });

            return MapToDto(entity, reason: aiOutput.OverallAnalysis.Reason);
        }

        // ② 版本清單
        public async Task<List<VersionSummaryDto>> GetVersionListAsync(int itineraryId)
        {
            var list = await _Repo.GetVersionSummariesAsync(itineraryId);

            return list.Select(v => new VersionSummaryDto
            {
                VersionId = v.VersionId,
                VersionNumber = (int)v.VersionNumber,
                Source = v.Source,
                VersionRemark = v.VersionRemark,
                CreateTime = (DateTime)v.CreateTime,
                CurrentUsageStatus = v.CurrentUsageStatus,
                // ICollection → 有任何一筆分析就算 HasAnalysis
                HasAnalysis = v.Aianalyses.Any()
            }).ToList();
        }

        // ③ 版本比較
        public async Task<VersionCompareResponseDto> CompareVersionsAsync(
            int itineraryId, int versionAId, int versionBId)
        {
            var vA = await _Repo.GetVersionWithItemsAndAnalysisAsync(itineraryId, versionAId);
            var vB = await _Repo.GetVersionWithItemsAndAnalysisAsync(itineraryId, versionBId);

            // 寫入比較紀錄（審計軌跡）
            var record = await _Repo.CreateComparisonAsync(versionAId, versionBId);

            return new VersionCompareResponseDto
            {
                ComparisonId = record.ComparisonId,
                VersionA = MapVersionDetail(vA),
                VersionB = MapVersionDetail(vB),
                Diffs = ComputeDiffs(vA.ItineraryItems, vB.ItineraryItems)
            };
        }

        // ── 私有輔助 ──

        private static AnalysisMetricsDto MapToDto(Aianalysis a, string? reason) => new()
        {
            AnalysisId = a.AnalysisId,
            VersionId = a.VersionId,
            FeasibilityScore = (double)a.FeasibilityScore,
            PaceBalanceScore = (double)a.PaceBalanceScore,
            FatigueIndex = (double)a.FatigueIndex,
            Reason = a.Reason ?? reason ?? string.Empty,
            AnalysisTime = (DateTime)a.AnalysisTime
        };

        private static VersionDetailDto MapVersionDetail(ItineraryVersion v) => new()
        {
            VersionId = v.VersionId,
            VersionNumber = (int)v.VersionNumber,
            CreateTime = (DateTime)v.CreateTime,
            VersionRemark = v.VersionRemark,
            Items = v.ItineraryItems
                             .OrderBy(i => i.DayNumber).ThenBy(i => i.SortOrder)
                             .Select(i => new ItemSnapDto
                             {
                                 DayNumber = (int)i.DayNumber,
                                 SortOrder = (int)i.SortOrder,
                                 AttractionName = i.AttractionName ?? string.Empty,
                                 StartTime = i.StartTime.HasValue ? i.StartTime.Value.ToString("HH:mm") : string.Empty,
                                 EndTime = i.EndTime.HasValue ? i.EndTime.Value.ToString("HH:mm") : string.Empty,
                                 GooglePlaceId = i.GooglePlaceId
                             }).ToList(),
            // ICollection → 取第一筆分析，無則 null
            Analysis = v.Aianalyses.FirstOrDefault() is { } analysis
                       ? MapToDto(analysis, reason: null)
                       : null
        };

        private static List<DiffItemDto> ComputeDiffs(
            ICollection<ItineraryItem> a, ICollection<ItineraryItem> b)
        {
            // key = "DayNumber-SortOrder"，代表行程中的同一個位置
            var aMap = a.GroupBy(x => $"{x.DayNumber}-{x.SortOrder}")
             .ToDictionary(g => g.Key, g => g.First().AttractionName ?? "");
            var bMap = b.GroupBy(x => $"{x.DayNumber}-{x.SortOrder}")
                        .ToDictionary(g => g.Key, g => g.First().AttractionName ?? "");

            return aMap.Keys.Union(bMap.Keys)
                       .OrderBy(k => k)
                       .Select(key =>
                       {
                           var parts = key.Split('-');
                           var label = $"Day {parts[0]} 第 {parts[1]} 站";
                           var inA = aMap.TryGetValue(key, out var valA);
                           var inB = bMap.TryGetValue(key, out var valB);

                           return (inA, inB) switch
                           {
                               (true, false) => new DiffItemDto { Label = label, OldValue = valA, DiffType = "removed" },
                               (false, true) => new DiffItemDto { Label = label, NewValue = valB, DiffType = "added" },
                               _ when valA != valB => new DiffItemDto { Label = label, OldValue = valA, NewValue = valB, DiffType = "changed" },
                               _ => null
                           };
                       })
                       .Where(d => d != null)
                       .Cast<DiffItemDto>()
                       .ToList();
        }

        private static AiPromptOutput ParseAiOutput(string raw)
        {
            var s = raw.Trim();
            if (s.StartsWith("```")) s = s[(s.IndexOf('\n') + 1)..];
            if (s.EndsWith("```")) s = s[..s.LastIndexOf("```")];
            Console.WriteLine($"[AI Raw] {s}");  // 加這行
            return JsonSerializer.Deserialize<AiPromptOutput>(s.Trim())
                   ?? throw new InvalidOperationException("AI 回傳格式錯誤");
        }
    }
}
