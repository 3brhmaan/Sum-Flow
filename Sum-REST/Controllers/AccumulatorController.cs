using Microsoft.AspNetCore.Mvc;
using Sum_REST.Models;

namespace Sum_REST.Controllers;

[Route("api/accumulator")]
[ApiController]
public class AccumulatorController : ControllerBase
{
    private readonly string _filePath;
    private readonly ILogger<AccumulatorController> _logger;

    public AccumulatorController(IWebHostEnvironment environment , ILogger<AccumulatorController> logger)
    {
        _filePath = Path.Combine(environment.ContentRootPath , "result.txt");
        _logger = logger;
    }

    [HttpPut]
    public async Task Accumulate(SumRequest request)
    {
        string content = await System.IO.File.ReadAllTextAsync(_filePath);
        int currentSum = request.Addition;

        if (int.TryParse(content , out var result))
        {
            currentSum += result;
            await System.IO.File.WriteAllTextAsync(_filePath , currentSum.ToString());
        }
        else
        {
            await System.IO.File.WriteAllTextAsync(_filePath , currentSum.ToString());
        }

        _logger.LogInformation($"Current Sum: {currentSum}");
    }
}
