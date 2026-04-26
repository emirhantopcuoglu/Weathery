namespace Weathery.Helpers;

public readonly record struct WeatherDescriptor(string Description, string Condition, string Icon);

public static class WmoWeatherCode
{
    public static WeatherDescriptor Describe(int code, bool isDay = true)
    {
        return code switch
        {
            0 => new("Açık", isDay ? "clear-day" : "clear-night",
                     isDay ? "bi-sun-fill" : "bi-moon-stars-fill"),
            1 => new("Çoğunlukla açık", isDay ? "clear-day" : "clear-night",
                     isDay ? "bi-brightness-high" : "bi-moon"),
            2 => new("Parçalı bulutlu", "clouds",
                     isDay ? "bi-cloud-sun-fill" : "bi-cloud-moon-fill"),
            3 => new("Kapalı", "clouds", "bi-clouds-fill"),

            45 or 48 => new("Sisli", "mist", "bi-cloud-fog2-fill"),

            51 => new("Hafif çisenti", "rain", "bi-cloud-drizzle"),
            53 => new("Çisenti", "rain", "bi-cloud-drizzle-fill"),
            55 => new("Yoğun çisenti", "rain", "bi-cloud-drizzle-fill"),
            56 or 57 => new("Donan çisenti", "snow", "bi-cloud-sleet-fill"),

            61 => new("Hafif yağmur", "rain", "bi-cloud-rain"),
            63 => new("Yağmurlu", "rain", "bi-cloud-rain-fill"),
            65 => new("Şiddetli yağmur", "rain", "bi-cloud-rain-heavy-fill"),
            66 or 67 => new("Donan yağmur", "snow", "bi-cloud-sleet-fill"),

            71 => new("Hafif kar", "snow", "bi-cloud-snow"),
            73 => new("Karlı", "snow", "bi-cloud-snow-fill"),
            75 => new("Yoğun kar", "snow", "bi-cloud-snow-fill"),
            77 => new("Kar taneleri", "snow", "bi-snow"),

            80 => new("Hafif sağanak", "rain", "bi-cloud-rain"),
            81 => new("Sağanak yağış", "rain", "bi-cloud-rain-heavy"),
            82 => new("Şiddetli sağanak", "rain", "bi-cloud-rain-heavy-fill"),

            85 or 86 => new("Kar sağanağı", "snow", "bi-cloud-snow-fill"),

            95 => new("Gök gürültülü fırtına", "thunder", "bi-cloud-lightning-rain-fill"),
            96 or 99 => new("Dolu fırtınası", "thunder", "bi-cloud-lightning-rain-fill"),

            _ => new("Bilinmeyen", "default", "bi-question-circle")
        };
    }
}
