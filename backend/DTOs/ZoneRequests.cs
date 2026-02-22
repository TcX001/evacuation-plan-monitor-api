namespace EvacuationAPI.DTOs
{
    public class CreateZoneRequest
    {
        public string ZoneId { get; set; } = null!;

        public LocationCoordinatesDto LocationCoordinates { get; set; } = new();

        public int NumberOfPeople { get; set; }

        public int UrgencyLevel { get; set; }
    }

    public class UpdateZoneRequest
    {
        public string? ZoneId { get; set; }

        public LocationCoordinatesDto? LocationCoordinates { get; set; }

        public int? NumberOfPeople { get; set; }

        public int? UrgencyLevel { get; set; }
    }
}
