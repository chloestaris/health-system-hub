using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class HealthRecord
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public DateTime RecordDate 
        { 
            get => _recordDate;
            set => _recordDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime _recordDate;

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Treatment { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Medications { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        // Navigation properties
        public Patient? Patient { get; set; }
        public ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
    }
} 