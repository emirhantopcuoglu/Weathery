using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Weathery.Helpers;
using Weathery.Models;
using Weathery.Services.Models;

namespace Weathery.Services;

public sealed class OpenMeteoService : IWeatherService, IForecastService
{
    private static readonly TimeSpan CurrentTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ForecastTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan GeocodingTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan MissTtl = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string CurrentParams =
        "temperature_2m,apparent_temperature,relative_humidity_2m,is_day," +
        "precipitation,rain,weather_code,wind_speed_10m,wind_direction_10m,pressure_msl";

    private const string HourlyParams =
        "temperature_2m,apparent_temperature,weather_code,is_day,visibility";

    private const string DailyParams =
        "weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset";

    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OpenMeteoService> _logger;

    public OpenMeteoService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        IMemoryCache cache,
        ILogger<OpenMeteoService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public Task<WeatherInfo?> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        var key = $"meteo:current:{city.ToLowerInvariant()}";
        return _cache.GetOrCreateAsync(key, async entry =>
        {
            var place = await ResolveAsync(city, cancellationToken);
            if (place is null)
            {
                entry.AbsoluteExpirationRelativeToNow = MissTtl;
                return null;
            }
            var response = await FetchAsync(place, includeHourly: false, includeDaily: true, cancellationToken);
            if (response?.Current is null)
            {
                entry.AbsoluteExpirationRelativeToNow = MissTtl;
                return null;
            }
            entry.AbsoluteExpirationRelativeToNow = CurrentTtl;
            return MapCurrent(place, response);
        });
    }

    public Task<ForecastData?> Get5DayAsync(string city, CancellationToken cancellationToken = default)
    {
        var key = $"meteo:forecast:{city.ToLowerInvariant()}";
        return _cache.GetOrCreateAsync(key, async entry =>
        {
            var place = await ResolveAsync(city, cancellationToken);
            if (place is null)
            {
                entry.AbsoluteExpirationRelativeToNow = MissTtl;
                return null;
            }
            var response = await FetchAsync(place, includeHourly: true, includeDaily: true, cancellationToken);
            if (response?.Hourly?.Time is null || response.Daily?.Time is null)
            {
                entry.AbsoluteExpirationRelativeToNow = MissTtl;
                return null;
            }
            entry.AbsoluteExpirationRelativeToNow = ForecastTtl;
            return MapForecast(place, response);
        });
    }

    private Task<GeocodingResult?> ResolveAsync(string city, CancellationToken cancellationToken)
    {
        var key = $"meteo:geo:{city.ToLowerInvariant()}";
        return _cache.GetOrCreateAsync(key, async entry =>
        {
            var url = $"{_options.GeocodingUrl}" +
                      $"?name={Uri.EscapeDataString(city)}" +
                      $"&count=1" +
                      $"&language={_options.Language}" +
                      $"&format=json";
            var response = await SendAsync<GeocodingResponse>(url, cancellationToken);
            var first = response?.Results?.FirstOrDefault();
            entry.AbsoluteExpirationRelativeToNow = first is null ? MissTtl : GeocodingTtl;
            return first;
        });
    }

    private Task<MeteoForecastResponse?> FetchAsync(
        GeocodingResult place,
        bool includeHourly,
        bool includeDaily,
        CancellationToken cancellationToken)
    {
        var lat = place.Latitude.ToString("0.######", CultureInfo.InvariantCulture);
        var lon = place.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
        var url = $"{_options.ForecastUrl}" +
                  $"?latitude={lat}&longitude={lon}" +
                  $"&current={CurrentParams}" +
                  (includeHourly ? $"&hourly={HourlyParams}" : string.Empty) +
                  (includeDaily ? $"&daily={DailyParams}" : string.Empty) +
                  $"&wind_speed_unit=ms" +
                  $"&timezone=auto" +
                  $"&forecast_days=5";

        return SendAsync<MeteoForecastResponse>(url, cancellationToken);
    }

    private static WeatherInfo MapCurrent(GeocodingResult place, MeteoForecastResponse response)
    {
        var c = response.Current!;
        var isDay = c.IsDay == 1;
        var descriptor = WmoWeatherCode.Describe(c.WeatherCode, isDay);

        var info = new WeatherInfo
        {
            City = place.Name,
            Country = place.Country,
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            Icon = descriptor.Icon,
            Condition = descriptor.Condition,
            Description = descriptor.Description,
            Temperature = c.Temperature,
            FeelsLike = c.FeelsLike,
            Humidity = c.Humidity,
            WindSpeed = c.WindSpeed,
            WindDeg = c.WindDirection,
            Pressure = c.Pressure,
            Rain = c.Rain,
            Date = c.Time
        };

        if (response.Daily?.Time is { Count: > 0 })
        {
            info.TempMin = response.Daily.TempMin?.FirstOrDefault() ?? 0;
            info.TempMax = response.Daily.TempMax?.FirstOrDefault() ?? 0;
            info.SunRise = response.Daily.Sunrise?.FirstOrDefault() ?? default;
            info.SunSet = response.Daily.Sunset?.FirstOrDefault() ?? default;
        }

        return info;
    }

    private static ForecastData MapForecast(GeocodingResult place, MeteoForecastResponse response)
    {
        var hourly = response.Hourly!;
        var daily = response.Daily!;

        var slots = new List<HourlyForecast>(hourly.Time!.Count);
        for (int i = 0; i < hourly.Time.Count; i++)
        {
            var time = hourly.Time[i];
            if (time.Hour % 3 != 0) continue;

            var code = hourly.WeatherCode?.ElementAtOrDefault(i) ?? 0;
            var isDay = (hourly.IsDay?.ElementAtOrDefault(i) ?? 1) == 1;
            var descriptor = WmoWeatherCode.Describe(code, isDay);

            slots.Add(new HourlyForecast
            {
                DateTime = time,
                Temperature = hourly.Temperature?.ElementAtOrDefault(i) ?? 0,
                FeelsLike = hourly.FeelsLike?.ElementAtOrDefault(i) ?? 0,
                Description = descriptor.Description,
                Icon = descriptor.Icon,
                Condition = descriptor.Condition
            });
        }

        var days = new List<DailySummary>(daily.Time!.Count);
        for (int i = 0; i < daily.Time.Count; i++)
        {
            var date = daily.Time[i].Date;
            var code = daily.WeatherCode?.ElementAtOrDefault(i) ?? 0;
            var descriptor = WmoWeatherCode.Describe(code, isDay: true);

            days.Add(new DailySummary
            {
                Date = date,
                TempMin = daily.TempMin?.ElementAtOrDefault(i) ?? 0,
                TempMax = daily.TempMax?.ElementAtOrDefault(i) ?? 0,
                Icon = descriptor.Icon,
                Description = descriptor.Description,
                Condition = descriptor.Condition,
                Slots = slots.Where(s => s.DateTime.Date == date).OrderBy(s => s.DateTime).ToList()
            });
        }

        return new ForecastData
        {
            City = place.Name ?? string.Empty,
            Country = place.Country ?? string.Empty,
            Forecasts = slots,
            Daily = days
        };
    }

    private async Task<T?> SendAsync<T>(string url, CancellationToken cancellationToken) where T : class
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Open-Meteo isteği başarısız oldu. URL: {Url}", url);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Open-Meteo yanıtı çözümlenemedi. URL: {Url}", url);
            return null;
        }
    }
}
