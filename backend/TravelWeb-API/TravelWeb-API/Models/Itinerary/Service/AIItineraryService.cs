using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using TravelWeb_API.Models.Itinerary.DTO;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class AIItineraryService : IAIItineraryService
    {
        private readonly AzureOpenAIClient _client;
        private readonly ChatClient _chatClient;
        private readonly IItineraryservice _itineraryService;
        public AIItineraryService(IConfiguration config, IItineraryservice itineraryservice)
        {
            // 從 appsettings.json 讀取金鑰與 Endpoint
            string endpoint = config["AzureOpenAI:Endpoint"];
            string key = config["AzureOpenAI:Key"];
            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            _chatClient = _client.GetChatClient(config["AzureOpenAI:DeploymentName"]);
            _itineraryService = itineraryservice;
        }
        public async Task<AIAnalysisResultDto> AnalyzeItineraryAsync(int itineraryId)
        {
            // 3. 設定對話選項
            ChatCompletionOptions options = new()
            {
                // 強制輸出 JSON
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                MaxOutputTokenCount = 2000,
                Temperature = 0.7f
            };
            var result = await _itineraryService.GetItineraryDetailAsync(itineraryId);
            // 4. 發送對話 (建立通道並傳送訊息)
            var completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("# Role\r\n你是一位旅遊行程評核員。\r\n\r\n# Task\r\n僅針對使用者提供的「固定順序行程」進行量化指標分析。\r\n\r\n# Rules\r\n1. 嚴禁修改順序：絕對不要更動景點的先後順序或刪除任何景點。\r\n2. 僅計算指標：針對當前排序，計算每日與整體的「可行度(Feasibility)」與「疲勞度(Fatigue)」。\r\n3. 輸出格式：僅回傳 JSON。\r\n\r\n# Output JSON Structure\r\n{\r\n  \"overall_analysis\": { \"feasibility\": 0.0, \"fatigue\": 0.0, \"reason\": \"string\" },\r\n  \"daily_analysis\": [\r\n    { \"day\": 1, \"feasibility\": 0.0, \"fatigue\": 0.0, \"route_comment\": \"string\" }\r\n  ]\r\n}"),
                new UserChatMessage($"請分析：{result}")
            ], options);

            // 4. 安全地獲取回傳文字
            string resultText = completion.Value.Content[0].Text;
            var analysisDto = JsonSerializer.Deserialize<AIAnalysisResultDto>(resultText);
            ;
        }
    }

}
