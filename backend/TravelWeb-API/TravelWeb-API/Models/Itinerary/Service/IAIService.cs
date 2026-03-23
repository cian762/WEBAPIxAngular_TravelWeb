namespace TravelWeb_API.Models.Itinerary.Service
{
    public interface IAIService
    {
        public Task<string> CallAiAsync(string systemPrompt, object userJsonInput);
    }
}
