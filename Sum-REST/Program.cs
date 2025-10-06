using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Sum_REST.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<AccumlatorService>();
builder.Services.AddHostedService<AccumlatorConsumerService>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.SetResourceBuilder(
            ResourceBuilder.CreateDefault().AddService("Sum-Flow")
        );
        builder.AddMeter("Custom-Meter");

        builder.AddAspNetCoreInstrumentation();
        builder.AddRuntimeInstrumentation();
        builder.AddProcessInstrumentation();

        builder.AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri("http://localhost:4317");
        });
    });

var app = builder.Build();

EnsureResultFileExist();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void EnsureResultFileExist()
{
    string fileName = "result.txt";
    string filePath = Path.Combine(Directory.GetCurrentDirectory() , fileName);

    if (!File.Exists(filePath))
    {
        File.Create(filePath);
    }
}
