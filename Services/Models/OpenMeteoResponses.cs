using System.Text.Json.Serialization;

namespace Weathery.Services.Models;

internal sealed class GeocodingResponse
{
    [JsonPropertyName("results")] public List<GeocodingResult>? Results { get; set; }
}

internal sealed class GeocodingResult
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
    [JsonPropertyName("country_code")] public string? CountryCode { get; set; }
    [JsonPropertyName("admin1")] public string? Admin1 { get; set; }
    [JsonPropertyName("timezone")] public string? Timezone { get; set; }
}

internal sealed class MeteoForecastResponse
{
    [JsonPropertyName("latitude")] public double Latitude { get; set; }
    [JsonPropertyName("longitude")] public double Longitude { get; set; }
    [JsonPropertyName("timezone")] public string? Timezone { get; set; }
    [JsonPropertyName("current")] public MeteoCurrent? Current { get; set; }
    [JsonPropertyName("hourly")] public MeteoHourly? Hourly { get; set; }
    [JsonPropertyName("daily")] public MeteoDaily? Daily { get; set; }
}

internal sealed class MeteoCurrent
{
    [JsonPropertyName("time")] public DateTime Time { get; set; }
    [JsonPropertyName("temperature_2m")] public double Temperature { get; set; }
    [JsonPropertyName("apparent_temperature")] public double FeelsLike { get; set; }
    [JsonPropertyName("relative_humidity_2m")] public double Humidity { get; set; }
    [JsonPropertyName("is_day")] public int IsDay { get; set; }
    [JsonPropertyName("precipitation")] public double Precipitation { get; set; }
    [JsonPropertyName("rain")] public double Rain { get; set; }
    [JsonPropertyName("weather_code")] public int WeatherCode { get; set; }
    [JsonPropertyName("wind_speed_10m")] public double WindSpeed { get; set; }
    [JsonPropertyName("wind_direction_10m")] public int WindDirection { get; set; }
    [JsonPropertyName("pressure_msl")] public double Pressure { get; set; }
}

internal sealed class MeteoHourly
{
    [JsonPropertyName("time")] public List<DateTime>? Time { get; set; }
    [JsonPropertyName("temperature_2m")] public List<double>? Temperature { get; set; }
    [JsonPropertyName("apparent_temperature")] public List<double>? FeelsLike { get; set; }
    [JsonPropertyName("weather_code")] public List<int>? WeatherCode { get; set; }
    [JsonPropertyName("is_day")] public List<int>? IsDay { get; set; }
    [JsonPropertyName("visibility")] public List<double>? Visibility { get; set; }
}

internal sealed class MeteoDaily
{
    [JsonPropertyName("time")] public List<DateTime>? Time { get; set; }
    [JsonPropertyName("weather_code")] public List<int>? WeatherCode { get; set; }
    [JsonPropertyName("temperature_2m_max")] public List<double>? TempMax { get; set; }
    [JsonPropertyName("temperature_2m_min")] public List<double>? TempMin { get; set; }
    [JsonPropertyName("sunrise")] public List<DateTime>? Sunrise { get; set; }
    [JsonPropertyName("sunset")] public List<DateTime>? Sunset { get; set; }
}
