using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class InsuranceClaim
    {
        public int Id { get; set; }

        [Required]
        public int HealthRecordId { get; set; }

        [Required]
        [StringLength(100)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ClaimDate 
        { 
            get => _claimDate;
            set => _claimDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime _claimDate;

        [Required]
        [StringLength(100)]
        public string InsuranceProvider { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public HealthRecord? HealthRecord { get; set; }
    }
} 