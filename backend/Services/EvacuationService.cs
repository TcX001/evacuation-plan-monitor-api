using EvacuationAPI.Data;
using EvacuationAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System.Text.Json;
using EvacuationAPI.Helpers;
using EvacuationAPI.DTOs;
using System.Net;

namespace EvacuationAPI.Services
{
    public class EvacuationService
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<EvacuationService> _logger;
        private const double MaxReasonableDistanceKm = 50.0; 

        public EvacuationService(AppDbContext context, IConnectionMultiplexer redis, ILogger<EvacuationService> logger)
        {
            _context = context;
            _redis = redis;
            _logger = logger;
        }

        public async Task<List<AssignmentResponse>> GeneratePlanAsync()
        {
            _logger.LogInformation("Generating new evacuation plan.");
            var assignments = new List<AssignmentResponse>();

            // Step 1: Get active zones sorted by urgency
            var zones = await _context.Zones
                .Where(z => z.NumberOfPeople > z.Evacuated)
                .OrderByDescending(z => z.UrgencyLevel)
                .ToListAsync();

            if (!zones.Any())
            {
                _logger.LogInformation("No zones require evacuation.");
                return assignments;
            }

            // Step 2: Get available vehicles
            var availableVehicles = await _context.Vehicles
                .Where(v => v.IsAvailable)
                .ToListAsync();

            if (!availableVehicles.Any())
            {
                _logger.LogWarning("Cannot generate plan: No available vehicles.");
                throw new AppException("No available vehicles to assign.", HttpStatusCode.BadRequest);
            }

            // Track vehicles to be marked as unavailable
            var vehiclesToDeactivate = new List<Vehicle>();

            // Step 3: Assignment Algorithm
            foreach (var zone in zones)
            {
                int remainingPeople = zone.Remaining;
                
                while (remainingPeople > 0 && availableVehicles.Any())
                {
                    // Score and sort vehicles for this specific zone
                    var vehicleScores = availableVehicles
                        .Select(v => new
                        {
                            Vehicle = v,
                            Distance = HaversineHelper.CalculateDistance(zone.Latitude, zone.Longitude, v.Latitude, v.Longitude)
                        })
                        .Where(vs => vs.Distance <= MaxReasonableDistanceKm) // Step 3.1: Reasonable distance
                        .Select(vs => new
                        {
                            vs.Vehicle,
                            vs.Distance,
                            // Step 3.2: Weighted Score
                            CapacityScore = vs.Vehicle.Capacity >= remainingPeople ? 100 : (double)vs.Vehicle.Capacity / remainingPeople * 100,
                            DistanceScore = Math.Max(0, 100 - (vs.Distance / MaxReasonableDistanceKm * 100)),
                            ETA = HaversineHelper.CalculateETA(vs.Distance, vs.Vehicle.Speed)
                        })
                        .Select(vs => new
                        {
                            vs.Vehicle,
                            vs.Distance,
                            vs.ETA,
                            // Weighted formula: 60% Capacity (prefer larger buses for big zones), 40% Distance
                            TotalScore = (vs.CapacityScore * 0.6) + (vs.DistanceScore * 0.4)
                        })
                        .OrderByDescending(vs => vs.TotalScore) // Step 3.3: Sort by score DESC
                        .ToList();

                    if (!vehicleScores.Any())
                    {
                        _logger.LogWarning($"No available vehicles within a reasonable distance for Zone {zone.ZoneId}.");
                        break; 
                    }

                    // Step 3.4: Assign the best vehicle
                    var bestMatch = vehicleScores.First();
                    var selectedVehicle = bestMatch.Vehicle;
                    
                    int peopleToEvacuate = Math.Min(selectedVehicle.Capacity, remainingPeople);
                    
                    var assignment = new AssignmentResponse
                    {
                        ZoneId = zone.ZoneId,
                        VehicleId = selectedVehicle.VehicleId,
                        ETA = $"{Math.Round(bestMatch.ETA, 1)} minutes",
                        NumberOfPeople = peopleToEvacuate,
                        DistanceKm = Math.Round(bestMatch.Distance, 2)
                    };
                    
                    assignments.Add(assignment);
                    
                    _logger.LogInformation($"Assigned Vehicle {selectedVehicle.VehicleId} ({selectedVehicle.Type}) -> Zone {zone.ZoneId} | ETA: {assignment.ETA} | People: {peopleToEvacuate} | Dist: {assignment.DistanceKm}km");

                    // Update state for the remainder of the algorithm
                    remainingPeople -= peopleToEvacuate;
                    availableVehicles.Remove(selectedVehicle); // Mark as used for this planning session
                    
                    // Track vehicle to be marked unavailable
                    if (!vehiclesToDeactivate.Contains(selectedVehicle))
                    {
                        vehiclesToDeactivate.Add(selectedVehicle);
                    }
                }

                // Step 3.5: Warning if capacity insufficient
                if (remainingPeople > 0)
                {
                    _logger.LogWarning($"Zone {zone.ZoneId}: {remainingPeople} people could not be assigned (insufficient vehicles/capacity).");
                }
            }

            if (!assignments.Any())
            {
                throw new AppException("No available vehicles within a reasonable distance.", HttpStatusCode.BadRequest);
            }

            // Persist vehicle availability changes to database
            foreach (var vehicle in vehiclesToDeactivate)
            {
                vehicle.IsAvailable = false;
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Marked {vehiclesToDeactivate.Count} vehicles as unavailable after plan generation.");

            // Step 4: Save plan to Redis
            var db = _redis.GetDatabase();
            await db.StringSetAsync("plan:current", JsonSerializer.Serialize(assignments));
            _logger.LogInformation("Saved new plan to Redis.");

            return assignments;
        }

        public async Task<List<EvacuationStatusResponse>> GetStatusAsync()
        {
            var zones = await _context.Zones.ToListAsync();
            var statusList = new List<EvacuationStatusResponse>();
            var db = _redis.GetDatabase();

            foreach (var zone in zones)
            {
                var cacheKey = $"evacuation_status:{zone.ZoneId}";
                var cachedData = await db.HashGetAllAsync(cacheKey);

                if (cachedData.Length > 0)
                {
                    var evacuatedEntry = cachedData.FirstOrDefault(x => x.Name == "TotalEvacuated");
                    var remainingEntry = cachedData.FirstOrDefault(x => x.Name == "RemainingPeople");
                    var lastVehicleEntry = cachedData.FirstOrDefault(x => x.Name == "LastVehicleUsed");

                    statusList.Add(new EvacuationStatusResponse
                    {
                        ZoneId = zone.ZoneId,
                        TotalEvacuated = !evacuatedEntry.Equals(default(HashEntry)) ? (int)evacuatedEntry.Value : 0,
                        RemainingPeople = !remainingEntry.Equals(default(HashEntry)) ? (int)remainingEntry.Value : 0,
                        LastVehicleUsed = !lastVehicleEntry.Equals(default(HashEntry)) ? lastVehicleEntry.Value.ToString() : null
                    });
                }
                else
                {
                    var status = new EvacuationStatusResponse
                    {
                        ZoneId = zone.ZoneId,
                        TotalEvacuated = zone.Evacuated,
                        RemainingPeople = zone.Remaining,
                        LastVehicleUsed = zone.LastVehicleUsed
                    };
                    statusList.Add(status);

                    await db.HashSetAsync(cacheKey, new HashEntry[]
                    {
                        new HashEntry("TotalEvacuated", status.TotalEvacuated),
                        new HashEntry("RemainingPeople", status.RemainingPeople),
                        new HashEntry("LastVehicleUsed", status.LastVehicleUsed ?? string.Empty)
                    });
                }
            }

            return statusList;
        }

        public async Task<EvacuationStatusResponse> UpdateEvacuationAsync(UpdateEvacuationRequest request)
        {
            var db = _redis.GetDatabase();
            var lockKey = $"lock:vehicle:{request.VehicleId}";

            bool isLocked = await db.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(5), When.NotExists);

            if (!isLocked)
            {
                _logger.LogWarning($"Vehicle {request.VehicleId} is currently locked by another operation.");
                throw new AppException($"Vehicle {request.VehicleId} is currently busy. Please try again later.", HttpStatusCode.Conflict);
            }

            try
            {
                var zone = await _context.Zones.FirstOrDefaultAsync(z => z.ZoneId == request.ZoneId);
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);

                if (zone == null || vehicle == null)
                    throw new AppException("Zone or Vehicle not found.", HttpStatusCode.NotFound);

                if (request.PeopleEvacuated <= 0)
                    throw new AppException("PeopleEvacuated must be greater than 0.", HttpStatusCode.BadRequest);

                if (request.PeopleEvacuated > vehicle.Capacity)
                    throw new AppException($"Vehicle {vehicle.VehicleId} can only carry {vehicle.Capacity} people.", HttpStatusCode.BadRequest);

                if (request.PeopleEvacuated > zone.Remaining)
                    throw new AppException($"Zone {zone.ZoneId} only has {zone.Remaining} people remaining.", HttpStatusCode.BadRequest);

                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    zone.Evacuated += request.PeopleEvacuated;
                    zone.LastVehicleUsed = vehicle.VehicleId;

                    var log = new EvacuationLog
                    {
                        ZoneId = zone.ZoneId,
                        VehicleId = vehicle.VehicleId,
                        PeopleCount = request.PeopleEvacuated
                    };

                    _context.EvacuationLogs.Add(log);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to update evacuation. Transaction rolled back.");
                    throw;
                }

                var cacheKey = $"evacuation_status:{zone.ZoneId}";
                await db.HashSetAsync(cacheKey, new HashEntry[]
                {
                    new HashEntry("TotalEvacuated", zone.Evacuated),
                    new HashEntry("RemainingPeople", zone.Remaining),
                    new HashEntry("LastVehicleUsed", zone.LastVehicleUsed ?? string.Empty)
                });

                _logger.LogInformation($"Update: Vehicle {vehicle.VehicleId} evacuated {request.PeopleEvacuated} people from Zone {zone.ZoneId} | Total: {zone.Evacuated}/{zone.NumberOfPeople}");

                return new EvacuationStatusResponse
                {
                    ZoneId = zone.ZoneId,
                    TotalEvacuated = zone.Evacuated,
                    RemainingPeople = zone.Remaining,
                    LastVehicleUsed = zone.LastVehicleUsed
                };
            }
            finally
            {
                await db.KeyDeleteAsync(lockKey);
            }
        }

        public async Task ClearAllDataAsync(string confirmation)
        {
            _logger.LogWarning("Clearing all evacuation data. This action cannot be undone.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"EvacuationLogs\"");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Vehicles\"");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"EvacuationZones\"");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to clear all data. Transaction rolled back.");
                throw;
            }

            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            
            // Delete keys by prefix (safer than FlushDatabaseAsync)
            var prefixes = new[] { "plan:", "evacuation_status:", "lock:vehicle:" };
            foreach (var prefix in prefixes)
            {
                var keys = server.Keys(pattern: prefix + "*").ToArray();
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
                _logger.LogInformation("Deleted {Count} keys with prefix '{Prefix}'.", keys.Length, prefix);
            }
            
            _logger.LogInformation("All data cleared from PostgreSQL and Redis.");
        }
    }
}
