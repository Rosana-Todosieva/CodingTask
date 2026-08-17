using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoversService _coversService;
    private readonly ILogger<CoversController> _logger;

    public CoversController(ICoversService coversService, ILogger<CoversController> logger)
    {
        _coversService = coversService;
        _logger = logger;
    }

    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        try
        {
            var result = _coversService.ComputePremium(startDate, endDate, coverType);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        var results = await _coversService.GetCoversAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        var cover = await _coversService.GetCoverAsync(id);
        if (cover is null)
        {
            return NotFound();
        }
        return Ok(cover);
    }

    [HttpPost]
    public async Task<ActionResult<Cover>> CreateAsync(Cover cover)
    {
        try
        {
            var createdCover = await _coversService.CreateCoverAsync(cover);
            return Ok(createdCover);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _coversService.DeleteCoverAsync(id);
        return NoContent();
    }
}