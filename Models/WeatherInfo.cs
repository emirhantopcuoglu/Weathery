namespace Weathery.Models;

public class WeatherInfo
{
    public string? City { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Icon { get; set; }
    public string? Condition { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public string? Description { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int WindDeg { get; set; }
    public double Pressure { get; set; }
    public double VisibilityKm { get; set; }
    public double Rain { get; set; }
    public DateTime SunRise { get; set; }
    public DateTime SunSet { get; set; }
    public DateTime Date { get; set; }
}
