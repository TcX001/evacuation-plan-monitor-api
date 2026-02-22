using EvacuationAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EvacuationAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Zone> Zones { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<EvacuationLog> EvacuationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Map entities to tables explicitly if needed (optional since DbSet names are used by default)
            modelBuilder.Entity<Zone>().ToTable("EvacuationZones");
            modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
            modelBuilder.Entity<EvacuationLog>().ToTable("EvacuationLogs");
        }
    }
}
