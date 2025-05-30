using Microsoft.EntityFrameworkCore;
using HealthHub.API.Models;

namespace HealthHub.API.Data
{
    public class HealthHubContext : DbContext
    {
        public HealthHubContext(DbContextOptions<HealthHubContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PostgreSQL-specific settings
            modelBuilder.UseIdentityColumns();

            // Configure relationships
            modelBuilder.Entity<HealthRecord>()
                .HasOne(h => h.Patient)
                .WithMany(p => p.HealthRecords)
                .HasForeignKey(h => h.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InsuranceClaim>()
                .HasOne(i => i.HealthRecord)
                .WithMany(h => h.InsuranceClaims)
                .HasForeignKey(i => i.HealthRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 