namespace Weathery.Helpers;

public static class WeatherDisplay
{
    public static string ConditionClass(string? condition)
        => $"weather-{condition ?? "default"}";

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
