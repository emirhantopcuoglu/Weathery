using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Weathery.Models;
namespace Weathery.Controllers
{
    public class WeatherInfoController : Controller
    {
        private readonly IConfiguration _configuration;
        public WeatherInfoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string city = "Istanbul")
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return RedirectToAction("Index", "WeatherInfo");
            }

            var weatherInfo = await GetWeatherInfo(city);

            if (weatherInfo == null)
            {
                TempData["Error"] = "Şehir ismi hatalı. Lütfen tekrar deneyiniz.";
                return RedirectToAction("Index", new { city = "Istanbul" });
            }

            return View(weatherInfo);
        }

        private async Task<WeatherInfo> GetWeatherInfo(string city)
        {
            var apiKey = _configuration["WeatherApiKey"];

            using (var client = new HttpClient())
            {
                string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&lang=tr&units=metric";
                try
                {
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    var responseData = await response.Content.ReadAsStringAsync();

                    var data = JsonSerializer.Deserialize<JsonElement>(responseData);

                    var sunriseUnix = data.GetProperty("sys").GetProperty("sunrise").GetDouble();

                    var sunsetUnix = data.GetProperty("sys").GetProperty("sunset").GetDouble();

                    return new WeatherInfo
                    {
                        City = data.GetProperty("name").GetString(),

                        Icon = data.GetProperty("weather")[0].GetProperty("icon").GetString(),

                        Temperature = data.GetProperty("main").GetProperty("temp").GetDouble(),

                        Description = data.GetProperty("weather")[0].GetProperty("description").GetString(),

                        Humidity = data.GetProperty("main").GetProperty("humidity").GetDouble(),

                        WindSpeed = data.GetProperty("wind").GetProperty("speed").GetDouble(),

                        Rain = data.TryGetProperty("rain", out var rainData) ? rainData.GetProperty("1h").GetDouble() : 0.0,

                        SunRise = UnixTimeToDateTime(sunriseUnix),

                        SunSet = UnixTimeToDateTime(sunsetUnix),
                        
                        Date = DateTime.Now
                    };
                }
                catch (Exception ex)
                {

                    return null;
                }
            }
        }
        public static DateTime UnixTimeToDateTime(double unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTime);
            return dateTimeOffset.DateTime.ToLocalTime(); // Yerel saat dilimine göre dönüştürme
        }

    }
}