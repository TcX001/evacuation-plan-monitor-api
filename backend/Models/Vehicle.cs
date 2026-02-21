using System.ComponentModel.DataAnnotations;

namespace EvacuationAPI.Models
{
    public class Vehicle
    {
        [Key]
        [MaxLength(50)]
        public string VehicleId { get; set; } = null!;

        public int Capacity { get; set; }

        [MaxLength(20)]
        public string Type { get; set; } = null!; // bus, van, boat

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double Speed { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
