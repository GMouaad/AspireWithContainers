using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

//*** Telemetry ***
// Custom metrics for the application
const string API_SERVICE_TELEMETRY = "AspireWithContainers.ApiService";
var apiServiceMeter = new Meter("AspireWithContainers.ApiService", "1.0.0");
var countExternalApiCalls = apiServiceMeter.CreateCounter<int>("apiService.calls.external.count", description: "Counts the number of external API calls");
var countInternalApiCalls = apiServiceMeter.CreateCounter<int>("apiService.calls.internal.count", description: "Counts the number of internal API calls");
// Custom ActivitySource for the application
var apiServiceActivitySource = new ActivitySource(API_SERVICE_TELEMETRY);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    using var activity = apiServiceActivitySource.StartActivity("weatherforecast");
    countInternalApiCalls.Add(1);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapGet("/hello", async ([FromServices] ILogger<Program> logger, [FromServices] HttpClient client) => {
    using var activity = apiServiceActivitySource.StartActivity("ExternalApiCall");
    logger.LogInformation("Calling remote API");

    activity?.AddEvent(new("ApiService.hello"));
    countExternalApiCalls.Add(1);
    var response = await client.GetFromJsonAsync<Payload>("http://HelloWorldApp/");
    logger.LogInformation("Received response payload: {Payload}", response);
    return response;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record Payload(string Message);