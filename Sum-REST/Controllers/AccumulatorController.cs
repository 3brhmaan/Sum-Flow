using Microsoft.AspNetCore.Mvc;
using Sum_REST.Services;

namespace Sum_REST.Controllers;

[Route("accumulator")]
[ApiController]
public class AccumulatorController : ControllerBase
{
    private readonly AccumlatorService _accumlatorService;

    public AccumulatorController(AccumlatorService accumlatorService)
    {
        _accumlatorService = accumlatorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccumlatorValue()
    {
        var value = await _accumlatorService.ReadAccumlatorValueAsync();

        return Ok(new { value });
    }
}
