using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Weathery.Helpers;
using Weathery.Models;
using Weathery.Services.Models;

namespace Weathery.Services;

public sealed class OpenWeatherService : IWeatherService, IForecastService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<OpenWeatherService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherInfo?> GetCurrentAsync(string city, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("weather", city);
        var response = await SendAsync<CurrentWeatherResponse>(url, cancellationToken);
        if (response is null)
        {
            return null;
        }

        var weather = response.Weather?.FirstOrDefault();
        return new WeatherInfo
        {
            City = response.Name,
            Latitude = response.Coord?.Lat ?? 0,
            Longitude = response.Coord?.Lon ?? 0,
            Icon = weather?.Icon,
            Description = weather?.Description,
            Temperature = response.Main?.Temp ?? 0,
            Humidity = response.Main?.Humidity ?? 0,
            WindSpeed = response.Wind?.Speed ?? 0,
            Rain = response.Rain?.OneHour ?? 0,
            SunRise = UnixTime.ToLocalDateTime(response.Sys?.Sunrise ?? 0),
            SunSet = UnixTime.ToLocalDateTime(response.Sys?.Sunset ?? 0),
            Date = DateTime.Now
        };
    }

    public async Task<ForecastData?> Get5DayAsync(string city, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("forecast", city);
        var response = await SendAsync<ForecastResponse>(url, cancellationToken);
        if (response?.List is null || response.City is null)
        {
            return null;
        }

        return new ForecastData
        {
            City = response.City.Name ?? city,
            Country = response.City.Country ?? string.Empty,
            Forecasts = response.List.Select(item => new HourlyForecast
            {
                DateTime = UnixTime.ToLocalDateTime(item.Dt),
                Temperature = item.Main?.Temp ?? 0,
                Description = item.Weather?.FirstOrDefault()?.Description ?? string.Empty
            }).ToList()
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
