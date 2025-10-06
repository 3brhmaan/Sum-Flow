using Microsoft.EntityFrameworkCore;
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
builder.Services.AddGrpcReflection();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); 
}

// Configure the HTTP request pipeline.
app.MapGrpcService<AdditionService>();
app.MapGrpcReflectionService();

app.Run();
