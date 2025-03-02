using SiteService.Model;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;


var builder = WebApplication.CreateBuilder(args);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});


var logger = loggerFactory.CreateLogger("Program");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("SiteService", "diogen", Assembly.GetExecutingAssembly().GetName().Version!.ToString())
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("service.environment", "debug")
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("SiteService")
        .AddConsoleExporter()
    )
    //.WithMetrics(metrics =>metrics
    //    .AddAspNetCoreInstrumentation()
    //    .AddConsoleExporter()
    //)
    .WithLogging(logging=> logging
        .AddConsoleExporter()
    );


var app = builder.Build();
logger.LogDebug("Starting application");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    logger.LogInformation("called weather");
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/site/{id}", (int siteId) =>
{
    logger.LogInformation($"calling site {siteId}", siteId);
    Task.Delay(2000);


    return new Site
    {
        Id = siteId,
        Name = "Site " + siteId,
        Latitude = (float)(Random.Shared.NextDouble() * 60),
        Longitude = (float)(Random.Shared.NextDouble() * 360 - 180)
    };
})
.WithName("GetSite")
.WithOpenApi();

app.MapGet("/site", () =>
{
    logger.LogInformation("calling sites");
    Task.Delay(2000);
    return Enumerable.Range(1, 5).Select(i => new Site
    {
        Id = i,
        Name = "Site " + i,
        Latitude = (float)(Random.Shared.NextDouble() * 60),
        Longitude = (float)(Random.Shared.NextDouble() * 360 - 180)
    });
});


app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
