using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly HospitalDbContext _context;

        public HomeController(HospitalDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Departments()
        {
            return View();
        }

        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsAvailable)
                .ToListAsync();
            
            return View(doctors);
        }

        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult Testimonials()
        {
            return View();
        }

        public IActionResult Gallery()
        {
            return View();
        }        // Login GET
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var hashedPassword = HashPassword(model.Password);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == model.Email && p.Password == hashedPassword);

            if (patient != null)
            {
                // Set session
                HttpContext.Session.SetInt32("PatientId", patient.Id);
                HttpContext.Session.SetString("PatientName", $"{patient.FirstName} {patient.LastName}");
                
                TempData["Success"] = "Login successful!";
                return RedirectToAction("Dashboard", "Patient");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }        // SignUp GET
        public IActionResult SignUp()
        {
            return View(new SignUpViewModel());
        }

        // SignUp POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == model.Email);

            if (existingPatient != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Create new patient from the ViewModel
            var patient = new Patient
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = model.Address ?? string.Empty,
                BloodType = model.BloodType ?? string.Empty,
                EmergencyContact = model.EmergencyContact ?? string.Empty,
                MedicalHistory = model.MedicalHistory ?? string.Empty,
                Insurance = model.Insurance ?? string.Empty,
                Password = HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully! Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult Contact()
        {
            return View();
        }        public async Task<IActionResult> Appointment()
        {
            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.IsAvailable)
                .ToListAsync();
            
            // Get unique departments from available doctors
            ViewBag.Departments = await _context.Doctors
                .Where(d => d.IsAvailable)
                .Select(d => d.Specialization)
                .Distinct()
                .ToListAsync();
            
            return View();
        }// Process appointment booking from the main appointment page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessAppointment(string name, string email, string phone, 
            DateTime date, int doctor, string department, string message)
        {
            try
            {
                // Extract time from the datetime
                string appointmentTime = date.ToString("HH:mm");
                DateTime appointmentDate = date.Date;
                
                // Check if patient exists by email
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == email);

                // If patient doesn't exist, create a basic patient record
                if (patient == null)
                {
                    var nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    patient = new Patient
                    {
                        FirstName = nameParts.FirstOrDefault() ?? name,
                        LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                        Email = email,
                        PhoneNumber = phone,
                        DateOfBirth = DateTime.Now.AddYears(-30), // Default
                        Gender = "Not Specified",
                        Password = HashPassword(Guid.NewGuid().ToString()), // Temporary password
                        CreatedAt = DateTime.Now
                    };

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                }                // Create appointment
                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = doctor,
                    AppointmentDate = appointmentDate,
                    AppointmentTime = appointmentTime,
                    Department = department,
                    Reason = message,
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment booked successfully! We will contact you soon.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while booking your appointment. Please try again.";
            }

            return RedirectToAction("Appointment");
        }

        // Contact form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitContact(string name, string email, string subject, string message)
        {
            // In a real application, you would save this to database or send email
            // For now, we'll just show a success message
            TempData["Success"] = "Thank you for your message! We will get back to you soon.";
            return RedirectToAction("Contact");
        }

        // Appointment request form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitAppointmentRequest(string name, string email, string phone, string date, string department, string doctor, string message)
        {
            // In a real application, you would save this to database or send notification
            // For now, we'll just show a success message
            TempData["Success"] = "Thank you for your appointment request! We will contact you soon to confirm your appointment.";
            return RedirectToAction("Appointment");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
