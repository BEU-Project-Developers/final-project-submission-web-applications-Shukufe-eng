using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Blood type cannot exceed 10 characters")]
        [Display(Name = "Blood Type")]
        public string BloodType { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Emergency contact cannot exceed 200 characters")]
        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Medical history cannot exceed 1000 characters")]
        [Display(Name = "Medical History")]
        public string MedicalHistory { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Insurance cannot exceed 200 characters")]
        public string Insurance { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
