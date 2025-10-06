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
            var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
            opts.Endpoint = new Uri(endpoint);
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
