using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class InsuranceClaim
    {
        public int Id { get; set; }

        public int HealthRecordId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ClaimDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string InsuranceProvider { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Navigation property
        public virtual HealthRecord HealthRecord { get; set; } = null!;
    }
} 