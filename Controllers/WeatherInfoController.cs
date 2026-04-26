using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Weathery.Services;

namespace Weathery.Controllers;

public class WeatherInfoController : Controller
{
    private readonly IWeatherService _weatherService;
    private readonly WeatherApiOptions _options;

    public WeatherInfoController(IWeatherService weatherService, IOptions<WeatherApiOptions> options)
    {
        _weatherService = weatherService;
        _options = options.Value;
    }

    public async Task<IActionResult> Index(string? city, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(city) ? _options.DefaultCity : city.Trim();

        var weatherInfo = await _weatherService.GetCurrentAsync(query, cancellationToken);

        if (weatherInfo is null)
        {
            TempData["Error"] = "Şehir bulunamadı veya hava durumu verisi alınamadı. Lütfen tekrar deneyiniz.";
            if (!string.Equals(query, _options.DefaultCity, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index), new { city = _options.DefaultCity });
            }
            return View();
        }

        return View(weatherInfo);
    }
}
