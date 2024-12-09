using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
            var weatherInfo = await GetWeatherInfo(city);
            return View(weatherInfo);
        }

        private async Task<WeatherInfo> GetWeatherInfo(string city)
        {
            var apiKey = _configuration["WeatherApiKey"];

            using (var client = new HttpClient())
            {
                string url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

                var response = await client.GetStringAsync(url);

                var data = JsonSerializer.Deserialize<JsonElement>(response);

                return new WeatherInfo
                {
                    City = data.GetProperty("name").GetString(),
                    Temperature = data.GetProperty("main").GetProperty("temp").GetDouble(),
                    Description = data.GetProperty("weather")[0].GetProperty("description").GetString(),
                    Humidity = data.GetProperty("main").GetProperty("humidity").GetDouble()
                };
            }
        }

    }
}