namespace Weathery.Helpers;

public static class WeatherDisplay
{
    private const double HotTemperatureThreshold = 30;
    private const double WarmTemperatureThreshold = 18;
    private const double CoolTemperatureThreshold = 5;
    private const double HighWindThreshold = 10;

    public static string BackgroundColor(double temperature) => temperature switch
    {
        > HotTemperatureThreshold => "#ff4c4c",
        > WarmTemperatureThreshold => "#ffcc00",
        > CoolTemperatureThreshold => "#1e90ff",
        _ => "#6a5acd"
    };

    public static string TemperatureIconColor(double temperature) => temperature switch
    {
        > HotTemperatureThreshold => "red",
        > WarmTemperatureThreshold => "orange",
        > CoolTemperatureThreshold => "blue",
        _ => "purple"
    };

    public static string WindIconColor(double windSpeed)
        => windSpeed > HighWindThreshold ? "red" : "gray";

    public static string IconUrl(string? icon)
        => string.IsNullOrEmpty(icon) ? string.Empty : $"https://openweathermap.org/img/wn/{icon}.png";
}
