using System.Text;
using System.Text.Json;
using TravelWeb_API.DTO.ActivityDTO;

namespace TravelWeb_API.Services
{
    public class GoogleRouteForActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public GoogleRouteForActivityService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<RouteResponseDTO> ComputeRouteAsync(RouteRequestDTO req) {
            var apiKey = _configuration["GoogleMapAPI:RoutesApiKey"];
            var url = "https://routes.googleapis.com/directions/v2:computeRoutes";

            var requestBody = new
            {
                origin = new {
                    location = new {
                        latLng = new {
                            latitude = req.OriginLat,
                            longitude = req.OriginLng
                        }
                    }
                },
                destination = new {
                    location = new {
                        latLng = new {
                            latitude = req.DestinationLat,
                            longitude = req.DestinationLng
                        }
                    }
                },
                travelMode = req.TravelMode,
                computeAlternativeRoutes = true,
                languageCode = "zh-TW",
                units = "METRIC"
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Add("X-Goog-Api-Key",apiKey);
            httpRequest.Headers.Add(
            "X-Goog-FieldMask",
            ("routes.description," +
            "routes.distanceMeters," +
            "routes.duration," +
            "routes.polyline.encodedPolyline," +
            "routes.routeLabels," +
            "routes.legs.steps.distanceMeters," +
            "routes.legs.steps.staticDuration," +
            "routes.legs.steps.navigationInstruction.instructions," +
            "routes.legs.steps.localizedValues")
            );

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync( httpRequest );
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Routes API error:{content}");
            }

            using var doc = JsonDocument.Parse(content);

            if (!doc.RootElement.TryGetProperty("routes", out var routesEl) || routesEl.GetArrayLength() == 0)
            {
                return null;
            }

            var result = new RouteResponseDTO();

            foreach (var routeEl in routesEl.EnumerateArray())
            {
                var distanceMeters = routeEl.GetProperty("distanceMeters").GetInt32();
                var durationRaw = FormatGoogleDuration(routeEl.GetProperty("duration").GetString()) ?? "";
                var encodedPolyline = routeEl.GetProperty("polyline").GetProperty("encodedPolyline").GetString() ?? "";
                var summary = routeEl.TryGetProperty("description", out var descEl) ? descEl.GetString() ?? "" : "";

                var labels = new List<string>();
                if (routeEl.TryGetProperty("routeLabels", out var labelsEl))
                {
                    foreach (var label in labelsEl.EnumerateArray())
                    {
                        labels.Add(label.GetString() ?? "");
                    }
                }

                var steps = new List<RouteStepDTO>();
                if (routeEl.TryGetProperty("legs", out var legsEl) && legsEl.GetArrayLength() > 0)
                {
                    var firstLeg = legsEl[0];

                    if (firstLeg.TryGetProperty("steps", out var stepsEl))
                    {
                        foreach (var stepEl in stepsEl.EnumerateArray())
                        {
                            var instruction = "";
                            if (stepEl.TryGetProperty("navigationInstruction", out var navEl) &&
                                navEl.TryGetProperty("instructions", out var instEl))
                            {
                                instruction = instEl.GetString() ?? "";
                            }

                            var stepDistance = stepEl.TryGetProperty("distanceMeters", out var distEl)
                                ? distEl.GetInt32()
                                : 0;

                            var stepDuration = stepEl.TryGetProperty("staticDuration", out var durEl)
                                ? durEl.GetString() ?? ""
                                : "";

                            steps.Add(new RouteStepDTO
                            {
                                Instruction = instruction,
                                DistanceMeters = stepDistance,
                                DurationText = stepDuration
                            });
                        }
                    }
                }

                result.Routes.Add(new RouteOptionDTO
                {
                    Summary = summary,
                    DistanceMeters = distanceMeters,
                    DistanceText = $"{distanceMeters / 1000.0:F1} 公里",
                    DurationText = durationRaw,
                    EncodedPolyline = encodedPolyline,
                    RouteLabels = labels,
                    Steps = steps
                });
            }

            return result;
        }

        //將 GOOGLE MAP API 回傳的時間格式改成 H / M ，取代原先只有 S 的格式
        private string FormatGoogleDuration(string durationRaw = "")
        {
            if (string.IsNullOrWhiteSpace(durationRaw) || !durationRaw.EndsWith("s"))
                return durationRaw;

            var secondsText = durationRaw.Replace("s", "");

            if (!double.TryParse(secondsText, out double totalSeconds))
                return durationRaw;

            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours} 小時 {timeSpan.Minutes} 分";
            }

            return $"{timeSpan.Minutes} 分";
        }



    }
}
