using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Weathery.Helpers;
using Weathery.Models;
using Weathery.Services.Models;

namespace Weathery.Services;

public sealed class OpenWeatherService : IWeatherService, IForecastService
{
    private static readonly TimeSpan CurrentWeatherTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ForecastTtl = TimeSpan.FromMinutes(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        IMemoryCache cache,
        ILogger<OpenWeatherService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public Task<WeatherInfo?> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"weather:current:{city.ToLowerInvariant()}";
        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CurrentWeatherTtl;
            var url = BuildUrl("weather", city);
            var response = await SendAsync<CurrentWeatherResponse>(url, cancellationToken);
            if (response is null)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return null;
            }
            return MapCurrent(response);
        });
    }

    public Task<ForecastData?> Get5DayAsync(string city, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"weather:forecast:{city.ToLowerInvariant()}";
        return _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = ForecastTtl;
            var url = BuildUrl("forecast", city);
            var response = await SendAsync<ForecastResponse>(url, cancellationToken);
            if (response?.List is null || response.City is null)
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return null;
            }
            return MapForecast(response, city);
        });
    }

    private static WeatherInfo MapCurrent(CurrentWeatherResponse response)
    {
        var weather = response.Weather?.FirstOrDefault();
        return new WeatherInfo
        {
            City = response.Name,
            Country = response.Sys?.Country,
            Latitude = response.Coord?.Lat ?? 0,
            Longitude = response.Coord?.Lon ?? 0,
            Icon = weather?.Icon,
            Condition = weather?.Main,
            Description = weather?.Description,
            Temperature = response.Main?.Temp ?? 0,
            FeelsLike = response.Main?.FeelsLike ?? 0,
            TempMin = response.Main?.TempMin ?? 0,
            TempMax = response.Main?.TempMax ?? 0,
            Humidity = response.Main?.Humidity ?? 0,
            WindSpeed = response.Wind?.Speed ?? 0,
            WindDeg = response.Wind?.Deg ?? 0,
            Pressure = response.Main?.Pressure ?? 0,
            VisibilityKm = Math.Round(response.Visibility / 1000.0, 1),
            Rain = response.Rain?.OneHour ?? response.Rain?.ThreeHour ?? 0,
            SunRise = UnixTime.ToLocalDateTime(response.Sys?.Sunrise ?? 0),
            SunSet = UnixTime.ToLocalDateTime(response.Sys?.Sunset ?? 0),
            Date = DateTime.Now
        };
    }

    private static ForecastData MapForecast(ForecastResponse response, string fallbackCity)
    {
        var forecasts = response.List!.Select(item =>
        {
            var w = item.Weather?.FirstOrDefault();
            return new HourlyForecast
            {
                DateTime = UnixTime.ToLocalDateTime(item.Dt),
                Temperature = item.Main?.Temp ?? 0,
                FeelsLike = item.Main?.FeelsLike ?? 0,
                Description = w?.Description ?? string.Empty,
                Icon = w?.Icon ?? string.Empty,
                Condition = w?.Main ?? string.Empty
            };
        }).ToList();

        var daily = forecasts
            .GroupBy(f => f.DateTime.Date)
            .Select(g =>
            {
                var midday = g.OrderBy(s => Math.Abs((s.DateTime.Hour - 12))).First();
                return new DailySummary
                {
                    Date = g.Key,
                    TempMin = g.Min(s => s.Temperature),
                    TempMax = g.Max(s => s.Temperature),
                    Icon = midday.Icon,
                    Description = midday.Description,
                    Condition = midday.Condition,
                    Slots = g.OrderBy(s => s.DateTime).ToList()
                };
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new ForecastData
        {
            City = response.City!.Name ?? fallbackCity,
            Country = response.City.Country ?? string.Empty,
            Forecasts = forecasts,
            Daily = daily
        };
    }

    private string BuildUrl(string endpoint, string city)
    {
        var encodedCity = Uri.EscapeDataString(city);
        return $"{_options.BaseUrl.TrimEnd('/')}/{endpoint}" +
               $"?q={encodedCity}" +
               $"&appid={_options.ApiKey}" +
               $"&units={_options.Units}" +
               $"&lang={_options.Language}";
    }

    private async Task<T?> SendAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Hava durumu API isteği başarısız oldu. URL: {Url}", url);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Hava durumu API yanıtı çözümlenemedi. URL: {Url}", url);
            return null;
        }
    }
}
