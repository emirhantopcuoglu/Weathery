namespace Weathery.Services;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5/";
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultCity { get; set; } = "Istanbul";
    public string Language { get; set; } = "tr";
    public string Units { get; set; } = "metric";
}
