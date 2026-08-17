using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimsService _claimsService;

        public ClaimsController(ILogger<ClaimsController> logger, IClaimsService claimsService)
        {
            _logger = logger;
            _claimsService = claimsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Claim>>> GetAsync()
        {
            var claims = await _claimsService.GetClaimsAsync();
            return Ok(claims);
        }

        [HttpPost]
        public async Task<ActionResult<Claim>> CreateAsync(Claim claim)
        {
            try
            {
                var createdClaim = await _claimsService.CreateClaimAsync(claim);
                return Ok(createdClaim);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            await _claimsService.DeleteClaimAsync(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Claim>> GetAsync(string id)
        {
            var claim = await _claimsService.GetClaimAsync(id);
            if (claim is null)
            {
                return NotFound();
            }
            return Ok(claim);
        }
    }

}