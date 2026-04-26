using Weathery.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<WeatherApiOptions>()
    .Bind(builder.Configuration.GetSection(WeatherApiOptions.SectionName))
    .Configure(opts => opts.ApiKey = builder.Configuration["WeatherApiKey"] ?? opts.ApiKey)
    .ValidateOnStart();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<OpenWeatherService>();
builder.Services.AddScoped<IWeatherService>(sp => sp.GetRequiredService<OpenWeatherService>());
builder.Services.AddScoped<IForecastService>(sp => sp.GetRequiredService<OpenWeatherService>());

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
