namespace Weathery.Models;

public class ForecastData
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<HourlyForecast> Forecasts { get; set; } = new();
}

public class HourlyForecast
{
    public DateTime DateTime { get; set; }
    public double Temperature { get; set; }
    public string Description { get; set; } = string.Empty;
}
