using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvacuationAPI.Models
{
    public class EvacuationLog
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string ZoneId { get; set; } = null!;

        [MaxLength(50)]
        public string VehicleId { get; set; } = null!;

        public int PeopleCount { get; set; }

        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ZoneId")]
        public EvacuationZone? Zone { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
