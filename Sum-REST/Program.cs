using Sum_REST.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<AccumlatorService>();
builder.Services.AddHostedService<AccumlatorConsumerService>();

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
