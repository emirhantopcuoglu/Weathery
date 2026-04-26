using System.Text.Json.Serialization;

namespace Weathery.Services.Models;

internal sealed class CurrentWeatherResponse
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("visibility")] public int Visibility { get; set; }
    [JsonPropertyName("weather")] public List<WeatherEntry>? Weather { get; set; }
    [JsonPropertyName("main")] public MainBlock? Main { get; set; }
    [JsonPropertyName("wind")] public WindBlock? Wind { get; set; }
    [JsonPropertyName("rain")] public RainBlock? Rain { get; set; }
    [JsonPropertyName("sys")] public SysBlock? Sys { get; set; }
    [JsonPropertyName("coord")] public CoordBlock? Coord { get; set; }
}

internal sealed class WeatherEntry
{
    [JsonPropertyName("main")] public string? Main { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("icon")] public string? Icon { get; set; }
}

internal sealed class MainBlock
{
    [JsonPropertyName("temp")] public double Temp { get; set; }
    [JsonPropertyName("feels_like")] public double FeelsLike { get; set; }
    [JsonPropertyName("temp_min")] public double TempMin { get; set; }
    [JsonPropertyName("temp_max")] public double TempMax { get; set; }
    [JsonPropertyName("pressure")] public double Pressure { get; set; }
    [JsonPropertyName("humidity")] public double Humidity { get; set; }
}

internal sealed class WindBlock
{
    [JsonPropertyName("speed")] public double Speed { get; set; }
    [JsonPropertyName("deg")] public int Deg { get; set; }
}

internal sealed class RainBlock
{
    [JsonPropertyName("1h")] public double? OneHour { get; set; }
    [JsonPropertyName("3h")] public double? ThreeHour { get; set; }
}

internal sealed class SysBlock
{
    [JsonPropertyName("sunrise")] public long Sunrise { get; set; }
    [JsonPropertyName("sunset")] public long Sunset { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
}

internal sealed class CoordBlock
{
    [JsonPropertyName("lat")] public double Lat { get; set; }
    [JsonPropertyName("lon")] public double Lon { get; set; }
}

internal sealed class ForecastResponse
{
    [JsonPropertyName("city")] public ForecastCity? City { get; set; }
    [JsonPropertyName("list")] public List<ForecastItem>? List { get; set; }
}

internal sealed class ForecastCity
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
}

internal sealed class ForecastItem
{
    [JsonPropertyName("dt")] public long Dt { get; set; }
    [JsonPropertyName("main")] public MainBlock? Main { get; set; }
    [JsonPropertyName("weather")] public List<WeatherEntry>? Weather { get; set; }
}
