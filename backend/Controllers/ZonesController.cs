using EvacuationAPI.DTOs;
using EvacuationAPI.Models;
using EvacuationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EvacuationAPI.Controllers
{
    [ApiController]
    [Route("api/evacuation-zones")]
    public class ZonesController : ControllerBase
    {
        private readonly ZoneService _zoneService;

        public ZonesController(ZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Zone>> GetZone(string id)
        {
            var zone = await _zoneService.GetZoneByIdAsync(id);
            if (zone == null) return NotFound();
            return Ok(zone);
        }

        [HttpPost]
        public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest request)
        {
            var zone = await _zoneService.CreateZoneAsync(request);
            return CreatedAtAction(nameof(GetZone), new { id = zone.ZoneId }, zone);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateZone(string id, [FromBody] UpdateZoneRequest request)
        {
            var zone = await _zoneService.UpdateZoneAsync(id, request);
            if (zone == null) return NotFound();
            return Ok(zone);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(string id)
        {
            var result = await _zoneService.DeleteZoneAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
