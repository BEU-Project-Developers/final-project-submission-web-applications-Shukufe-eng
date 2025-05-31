using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
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
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [StringLength(200)]
        public string EmergencyContact { get; set; } = string.Empty;

        [StringLength(1000)]
        public string MedicalHistory { get; set; } = string.Empty;

        [StringLength(200)]
        public string Insurance { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Computed properties for view compatibility
        public string Name => $"{FirstName} {LastName}";
        public string Phone => PhoneNumber;
        public DateTime RegistrationDate => CreatedAt;

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    }
}
