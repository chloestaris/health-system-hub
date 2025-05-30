using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models.DTOs
{
    public class InsuranceClaimDTO
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public DateTime ClaimDate { get; set; }
        public string InsuranceProvider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
} 