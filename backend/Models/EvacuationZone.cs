using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvacuationAPI.Models
{
    public class EvacuationZone
    {
        [Key]
        [MaxLength(50)]
        public string ZoneId { get; set; } = null!;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int NumberOfPeople { get; set; }

        [Range(1, 5)]
        public int UrgencyLevel { get; set; }

        public int Evacuated { get; set; } = 0;

        [MaxLength(50)]
        public string? LastVehicleUsed { get; set; }

        [NotMapped]
        public int Remaining => NumberOfPeople - Evacuated;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
