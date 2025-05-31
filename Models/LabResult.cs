using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{    public enum LabResultStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Reviewed
    }

    public class LabResult
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string TestName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TestType { get; set; } = string.Empty;        [StringLength(1000)]
        public string Results { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        [StringLength(500)]
        public string ReferenceRange { get; set; } = string.Empty;

        [StringLength(200)]
        public string Unit { get; set; } = string.Empty;

        public LabResultStatus Status { get; set; } = LabResultStatus.Pending;

        [StringLength(1000)]
        public string Comments { get; set; } = string.Empty;

        public DateTime DateOrdered { get; set; }

        public DateTime? DateCompleted { get; set; }

        public DateTime TestDate { get; set; }

        public DateTime? ResultDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Patient Patient { get; set; } = null!;
    }
}
