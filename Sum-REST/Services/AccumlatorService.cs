namespace Sum_REST.Services;

public class AccumlatorService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1 , 1);

    public AccumlatorService(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath , "result.txt");

        EnsureResultFileExist();
    }

    public async Task<string> ReadAccumlatorValueAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            return await File.ReadAllTextAsync(_filePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAccumlatorValueAsync(int newAccumlatorValue)
    {
        await _fileLock.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_filePath , newAccumlatorValue.ToString());
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddAsync(int delta)
    {
        await _fileLock.WaitAsync();
        try
        {
            string content = await File.ReadAllTextAsync(_filePath);
            int current = int.TryParse(content , out var c) ? c : 0;
            await File.WriteAllTextAsync(_filePath , (current + delta).ToString());
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private void EnsureResultFileExist()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath , "0");
        }
    }

    public void Dispose()
    {
        _fileLock?.Dispose();
    }
}
