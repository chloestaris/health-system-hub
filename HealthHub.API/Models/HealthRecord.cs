using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class HealthRecord
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Treatment { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Medications { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
    }
} 