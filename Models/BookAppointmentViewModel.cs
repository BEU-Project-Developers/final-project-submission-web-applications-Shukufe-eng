using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class BookAppointmentViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string AppointmentTime { get; set; } = string.Empty;        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ReasonForVisit { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // For populating dropdowns
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<string> Departments { get; set; } = new List<string>();
    }
}
