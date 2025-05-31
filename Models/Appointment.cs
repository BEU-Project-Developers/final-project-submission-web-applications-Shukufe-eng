using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Confirmed,
        InProgress,
        Completed,
        Cancelled,
        NoShow
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(10)]
        public string AppointmentTime { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}
