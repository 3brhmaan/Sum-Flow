using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sum_gRPC.Data;
using Sum_gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddLogging();
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
    );
});
builder.Services.AddHostedService<OutboxProcessor>();


//builder.Services.AddOpenTelemetry()
//    .WithMetrics(builder =>
//    {
//        builder.SetResourceBuilder(
//            ResourceBuilder.CreateDefault().AddService("Sum-Flow")
//        );
//        builder.AddMeter("Custom-Meter");

//        builder.AddAspNetCoreInstrumentation();
//        builder.AddRuntimeInstrumentation();
//        builder.AddProcessInstrumentation();

//        builder.AddOtlpExporter(opts =>
//        {
//            opts.Endpoint = new Uri("http://localhost:4317");
//        });
//    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AdditionService>();

app.Run();
