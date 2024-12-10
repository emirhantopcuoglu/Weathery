namespace Weathery.Models
{
    public class WeatherInfo
    {
        public string? City { get; set; }
        public string? Icon { get; set; }
        public double Temperature { get; set; }
        public string? Description { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public double Rain { get; set; }
        public DateTime SunRise { get; set; }
        public DateTime SunSet { get; set; }

    }

    
}