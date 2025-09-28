namespace Sum_REST.Services;

public class AccumlatorService
{
    private readonly string _filePath;
    public AccumlatorService(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath , "result.txt");
    }

    public async Task<string> ReadAccumlatorValueAsync()
    {
        string content = await File.ReadAllTextAsync(_filePath);

        return content;
    }

    public async Task UpdateAccumlatorValueAsync(int newAccumlatorValue)
    {
        await File.WriteAllTextAsync(_filePath , newAccumlatorValue.ToString());
    }
}
