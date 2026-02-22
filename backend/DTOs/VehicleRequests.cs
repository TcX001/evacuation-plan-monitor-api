namespace EvacuationAPI.DTOs
{
    public class CreateVehicleRequest
    {
        public string VehicleId { get; set; } = null!;

        public int Capacity { get; set; }

        public string Type { get; set; } = null!;

        public LocationCoordinatesDto LocationCoordinates { get; set; } = null!;

        public double Speed { get; set; }
        
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateVehicleRequest
    {
        public string? VehicleId { get; set; }

        public int? Capacity { get; set; }

        public string? Type { get; set; }

        public LocationCoordinatesDto? LocationCoordinates { get; set; }

        public double? Speed { get; set; }
        
        public bool? IsAvailable { get; set; }
    }
}
