using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
} 