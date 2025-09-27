using Sum_gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AdditionService>();

app.Run();
