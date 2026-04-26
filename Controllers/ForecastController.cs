using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Weathery.Models;

namespace Weathery.Controllers
{
    public class ForecastController : Controller
    {
        private readonly IConfiguration _configuration;
        public ForecastController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string city = "Istanbul")
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                city = "Istanbul"; // Varsayılan şehir
            }

            var forecastData = await GetForecastDataAsync(city);

            if (forecastData == null)
            {
                TempData["Error"] = "Veri alınamadı. Lütfen tekrar deneyiniz.";
                return RedirectToAction("Index");
            }

            return View(forecastData);
        }

        private async Task<ForecastData> GetForecastDataAsync(string city)
        {
            var apiKey = _configuration["WeatherApiKey"];

            using (var client = new HttpClient())
            {
                string url = $"http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric&lang=tr";

                var response = await client.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<JsonElement>(response);

                if (!response.Contains("list"))
                {
                    return null; // Veri bulunamazsa null döndür
                }

                return ParseForecastData(data);
            }
        }

        private ForecastData ParseForecastData(JsonElement data)
        {
            var forecastData = new ForecastData
            {
                City = data.GetProperty("city").GetProperty("name").GetString(),
                Country = data.GetProperty("city").GetProperty("country").GetString(),
                Forecasts = new List<HourlyForecast>()
            };

            foreach (var item in data.GetProperty("list").EnumerateArray())
            {
                var forecast = new HourlyForecast
                {
                    DateTime = UnixTimeToDateTime(item.GetProperty("dt").GetDouble()),
                    Temperature = item.GetProperty("main").GetProperty("temp").GetDouble(),
                    Description = item.GetProperty("weather")[0].GetProperty("description").GetString()
                };
                forecastData.Forecasts.Add(forecast);
            }

            return forecastData;
        }

        public static DateTime UnixTimeToDateTime(double unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTime);
            return dateTimeOffset.DateTime.ToLocalTime(); // Yerel saat dilimine göre dönüştürme
        }
    }
}
