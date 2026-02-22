using EvacuationAPI.Data;
using EvacuationAPI.DTOs;
using EvacuationAPI.Helpers;
using EvacuationAPI.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace EvacuationAPI.Services
{
    public class ZoneService
    {
        private readonly AppDbContext _context;

        public ZoneService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Zone> CreateZoneAsync(CreateZoneRequest request)
        {
            var existingZone = await _context.Zones.FindAsync(request.ZoneId);
            if (existingZone != null)
            {
                throw new AppException($"Zone with ID '{request.ZoneId}' already exists.", HttpStatusCode.Conflict);
            }

            var zone = new Zone
            {
                ZoneId = request.ZoneId,
                Latitude = request.LocationCoordinates.Latitude,
                Longitude = request.LocationCoordinates.Longitude,
                NumberOfPeople = request.NumberOfPeople,
                UrgencyLevel = request.UrgencyLevel
            };

            _context.Zones.Add(zone);
            await _context.SaveChangesAsync();

            return zone;
        }

        public async Task<Zone?> GetZoneByIdAsync(string id)
        {
            return await _context.Zones.FindAsync(id);
        }

        public async Task<bool> DeleteZoneAsync(string id)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return false;

            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Zone?> UpdateZoneAsync(string id, UpdateZoneRequest request)
        {
            var zone = await _context.Zones.FindAsync(id);
            if (zone == null) return null;

            if (request.LocationCoordinates != null)
            {
                zone.Latitude = request.LocationCoordinates.Latitude;
                zone.Longitude = request.LocationCoordinates.Longitude;
            }

            if (request.NumberOfPeople.HasValue)
                zone.NumberOfPeople = request.NumberOfPeople.Value;

            if (request.UrgencyLevel.HasValue)
                zone.UrgencyLevel = request.UrgencyLevel.Value;

            await _context.SaveChangesAsync();
            return zone;
        }
    }
}
