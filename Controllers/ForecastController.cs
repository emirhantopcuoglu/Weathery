using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Weathery.Services;

namespace Weathery.Controllers;

public class ForecastController : Controller
{
    private readonly IForecastService _forecastService;
    private readonly WeatherApiOptions _options;

    public ForecastController(IForecastService forecastService, IOptions<WeatherApiOptions> options)
    {
        _forecastService = forecastService;
        _options = options.Value;
    }

    public async Task<IActionResult> Index(string? city, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(city) ? _options.DefaultCity : city.Trim();

        var forecastData = await _forecastService.Get5DayAsync(query, cancellationToken);

        if (forecastData is null)
        {
            TempData["Error"] = "Şehir bulunamadı veya tahmin verisi alınamadı. Lütfen tekrar deneyiniz.";
            if (!string.Equals(query, _options.DefaultCity, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(Index), new { city = _options.DefaultCity });
            }
            return View();
        }

        return View(forecastData);
    }
}
