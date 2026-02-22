using EvacuationAPI.Data;
using EvacuationAPI.DTOs;
using EvacuationAPI.Helpers;
using EvacuationAPI.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace EvacuationAPI.Services
{
    public class VehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Vehicle> CreateVehicleAsync(CreateVehicleRequest request)
        {
            var existingVehicle = await _context.Vehicles.FindAsync(request.VehicleId);
            if (existingVehicle != null)
            {
                throw new AppException($"Vehicle with ID '{request.VehicleId}' already exists.", HttpStatusCode.Conflict);
            }

            var vehicle = new Vehicle
            {
                VehicleId = request.VehicleId,
                Capacity = request.Capacity,
                Type = request.Type.ToLower(),
                Latitude = request.LocationCoordinates.Latitude,
                Longitude = request.LocationCoordinates.Longitude,
                Speed = request.Speed,
                IsAvailable = request.IsAvailable
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return vehicle;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(string id)
        {
            return await _context.Vehicles.FindAsync(id);
        }

        public async Task<bool> DeleteVehicleAsync(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return false;

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Vehicle?> UpdateVehicleAsync(string id, UpdateVehicleRequest request)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return null;

            if (request.Capacity.HasValue)
                vehicle.Capacity = request.Capacity.Value;
            
            if (request.Type != null)
                vehicle.Type = request.Type.ToLower();
            
            if (request.LocationCoordinates != null)
            {
                vehicle.Latitude = request.LocationCoordinates.Latitude;
                vehicle.Longitude = request.LocationCoordinates.Longitude;
            }
            
            if (request.Speed.HasValue)
                vehicle.Speed = request.Speed.Value;
            
            if (request.IsAvailable.HasValue)
                vehicle.IsAvailable = request.IsAvailable.Value;

            await _context.SaveChangesAsync();
            return vehicle;
        }
    }
}
