namespace TravelWeb_API.Models.Itinerary.Service
{
    public class GooglePlaceService : IGooglePlaceService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GooglePlaceService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["GooglePlaces:ApiKey"]; // 從 appsettings.json 讀
        }

        public async Task<string?> GetPlaceIdAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var url = $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json" +
                      $"?input={Uri.EscapeDataString(address)}" +
                      $"&inputtype=textquery&fields=place_id&key={_apiKey}";
            var raw = await _http.GetStringAsync(url);
            Console.WriteLine($"Places API 原始回傳: {raw}");
            var response = await _http.GetFromJsonAsync<PlacesApiResponse>(url);
            Console.WriteLine($"反序列化結果: candidates數量={response?.candidates?.Count}, status={response?.status}");
            return response?.candidates?.FirstOrDefault()?.place_id;
        }

        // 對應 Google API 回傳的 JSON 結構
        private record PlacesApiResponse(List<Candidate> candidates, string status);
        private record Candidate(string place_id);
    }
}

