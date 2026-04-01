using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace TravelWeb_API.Models.Itinerary.Service
{
    public class AIService : IAIService

    {
        private readonly AzureOpenAIClient _client;
        private readonly ChatClient _chatClient;

        public AIService(IConfiguration config)
        {
            // 從 appsettings.json 讀取金鑰與 Endpoint
            string endpoint = config["AzureOpenAI:Endpoint"];
            string key = config["AzureOpenAI:Key"];
            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            _chatClient = _client.GetChatClient(config["AzureOpenAI:DeploymentName"]);

        }
        public async Task<string> CallAiAsync(string systemPrompt, object userJsonInput)
        {
            // 3. 設定對話選項
            ChatCompletionOptions options = new()
            {
                // 強制輸出 JSON
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = 0.7f,

            };
            var userContent = userJsonInput is string s
        ? s
        : JsonSerializer.Serialize(userJsonInput);


            // 4. 發送對話 (建立通道並傳送訊息)
            var completion = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userContent)
            ], options);

            // 4. 安全地獲取回傳文字
            string resultText = completion.Value.Content[0].Text;
            return resultText;
        }


    }

}
