namespace EvacuationAPI.DTOs
{
    public class UpdateEvacuationRequest
    {
        public string ZoneId { get; set; } = null!;
        public string VehicleId { get; set; } = null!;
        public int PeopleEvacuated { get; set; }
    }

    public class AssignmentResponse
    {
        public string ZoneId { get; set; } = null!;
        public string VehicleId { get; set; } = null!;
        public string ETA { get; set; } = null!;
        public int NumberOfPeople { get; set; }
        public double DistanceKm { get; set; }
    }

    public class EvacuationStatusResponse
    {
        public string ZoneId { get; set; } = null!;
        public int TotalEvacuated { get; set; }
        public int RemainingPeople { get; set; }
        public string? LastVehicleUsed { get; set; }
    }

    public class ClearDataRequest
    {
        public string Confirmation { get; set; } = null!;
    }
}
