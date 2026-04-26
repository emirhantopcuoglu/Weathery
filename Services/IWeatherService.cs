using Weathery.Models;

namespace Weathery.Services;

public interface IWeatherService
{
    Task<WeatherInfo?> GetCurrentAsync(string city, CancellationToken cancellationToken = default);
}

public interface IForecastService
{
    Task<ForecastData?> Get5DayAsync(string city, CancellationToken cancellationToken = default);
}
