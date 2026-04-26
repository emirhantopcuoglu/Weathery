namespace Weathery.Services;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string ForecastUrl { get; set; } = "https://api.open-meteo.com/v1/forecast";
    public string GeocodingUrl { get; set; } = "https://geocoding-api.open-meteo.com/v1/search";
    public string DefaultCity { get; set; } = "Istanbul";
    public string Language { get; set; } = "tr";
}
