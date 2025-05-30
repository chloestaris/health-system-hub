using System.ComponentModel.DataAnnotations;

namespace HealthHub.API.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth 
        { 
            get => _dateOfBirth;
            set => _dateOfBirth = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        private DateTime _dateOfBirth;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
} 