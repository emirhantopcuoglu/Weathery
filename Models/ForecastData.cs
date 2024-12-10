namespace Weathery.Models
{
    public class ForecastData
    {
        public string City { get; set; }
        public string Country { get; set; }
        public List<HourlyForecast> Forecasts { get; set; }
    }

    public class HourlyForecast
    {
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
    }
}