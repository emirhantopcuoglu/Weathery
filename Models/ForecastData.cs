namespace Weathery.Models;

public class ForecastData
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<HourlyForecast> Forecasts { get; set; } = new();
    public List<DailySummary> Daily { get; set; } = new();
}

public class HourlyForecast
{
    public DateTime DateTime { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
}

public class DailySummary
{
    public DateTime Date { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public List<HourlyForecast> Slots { get; set; } = new();
}
