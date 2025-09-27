using Microsoft.AspNetCore.Mvc;
using Sum_REST.Models;

namespace Sum_REST.Controllers;

[Route("api/sum")]
[ApiController]
public class SumController : ControllerBase
{
    private readonly string _filePath;

    public SumController(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath , "result.txt");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSum(SumRequest request)
    {
        string content = await System.IO.File.ReadAllTextAsync(_filePath);

        if (int.TryParse(content , out var result))
        {
            result += request.Addition;
            await System.IO.File.WriteAllTextAsync(_filePath , result.ToString());

            return Ok(new SumResponse { Result = result });
        }
        else
        {
            await System.IO.File.WriteAllTextAsync(_filePath , request.Addition.ToString());

            return Ok(new SumResponse { Result = request.Addition });
        }
    }
}
