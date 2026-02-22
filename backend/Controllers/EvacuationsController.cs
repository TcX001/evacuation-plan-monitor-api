using EvacuationAPI.DTOs;
using EvacuationAPI.Models;
using EvacuationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvacuationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvacuationsController : ControllerBase
    {
        private readonly EvacuationService _evacuationService;

        public EvacuationsController(EvacuationService evacuationService)
        {
            _evacuationService = evacuationService;
        }

        [HttpPost("plan")]
        public async Task<ActionResult<List<AssignmentResponse>>> CreatePlan()
        {
            var plan = await _evacuationService.GeneratePlanAsync();
            return Ok(plan);
        }

        [HttpGet("status")]
        public async Task<ActionResult<List<EvacuationStatusResponse>>> GetStatus()
        {
            var status = await _evacuationService.GetStatusAsync();
            return Ok(status);
        }

        [HttpPut("update")]
        public async Task<ActionResult<EvacuationStatusResponse>> UpdateEvacuation([FromBody] UpdateEvacuationRequest request)
        {
            var result = await _evacuationService.UpdateEvacuationAsync(request);
            return Ok(result);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAll([FromBody] ClearDataRequest request)
        {
            await _evacuationService.ClearAllDataAsync(request.Confirmation);
            return Ok(new { message = "All evacuation data cleared successfully" });
        }
    }
}
