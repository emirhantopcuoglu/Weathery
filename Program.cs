using Weathery.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<WeatherApiOptions>()
    .Bind(builder.Configuration.GetSection(WeatherApiOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<OpenMeteoService>();
builder.Services.AddScoped<IWeatherService>(sp => sp.GetRequiredService<OpenMeteoService>());
builder.Services.AddScoped<IForecastService>(sp => sp.GetRequiredService<OpenMeteoService>());

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=WeatherInfo}/{action=Index}/{id?}");

app.Run();
