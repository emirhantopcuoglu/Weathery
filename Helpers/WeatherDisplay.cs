namespace Weathery.Helpers;

public static class WeatherDisplay
{
    public static string IconUrl(string? icon)
        => string.IsNullOrEmpty(icon) ? string.Empty : $"https://openweathermap.org/img/wn/{icon}@2x.png";

    public static string ConditionClass(string? condition, string? icon = null)
    {
        var isNight = !string.IsNullOrEmpty(icon) && icon.EndsWith("n", StringComparison.OrdinalIgnoreCase);
        var key = (condition ?? string.Empty).ToLowerInvariant() switch
        {
            "clear" => isNight ? "clear-night" : "clear-day",
            "clouds" => "clouds",
            "rain" or "drizzle" => "rain",
            "thunderstorm" => "thunder",
            "snow" => "snow",
            "mist" or "fog" or "haze" or "smoke" or "dust" or "sand" or "ash" or "squall" or "tornado" => "mist",
            _ => "default"
        };
        return $"weather-{key}";
    }

    public static string WindDirection(int deg) => deg switch
    {
        >= 337 or < 23 => "K",
        < 68 => "KD",
        < 113 => "D",
        < 158 => "GD",
        < 203 => "G",
        < 248 => "GB",
        < 293 => "B",
        _ => "KB"
    };
}
