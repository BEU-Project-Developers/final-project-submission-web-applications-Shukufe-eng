using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly HospitalDbContext _context;

        public AppointmentController(HospitalDbContext context)
        {
            _context = context;
        }

        // GET: Show appointment form
        public async Task<IActionResult> Index()
        {
            ViewBag.Doctors = await _context.Doctors
                .Where(d => d.IsAvailable)
                .Select(d => new { d.Id, d.FirstName, d.LastName, d.Specialization })
                .ToListAsync();

            return View();
        }        // POST: Book appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(string name, string email, string phone, 
            DateTime appointmentDate, string appointmentTime, int doctorId, string department, string message)
        {
            try
            {
                // Check if patient exists by email
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == email);

                // If patient doesn't exist, create a temporary patient record
                if (patient == null)
                {
                    // For guest appointments, we'll create a basic patient record
                    // You might want to send them a signup link later
                    patient = new Patient
                    {
                        FirstName = name.Split(' ').FirstOrDefault() ?? name,
                        LastName = name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        Email = email,
                        PhoneNumber = phone,
                        DateOfBirth = DateTime.Now.AddYears(-30), // Default age
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
                    DoctorId = doctorId,
                    AppointmentDate = appointmentDate,
                    AppointmentTime = appointmentTime ?? "09:00",
                    Department = department,
                    Reason = message,
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment booked successfully! We will contact you soon.";
                return RedirectToAction("Index", "Home");            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while booking your appointment. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Get available time slots for a doctor on a specific date
        [HttpGet]        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            // Get existing appointments for the doctor on the specified date
            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.AppointmentTime)
                .ToListAsync();            // Define available time slots with 30-minute intervals (matching PatientController)
            var allSlots = new List<string>
            {
                "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "14:00", "14:30", "15:00", "15:30", "16:00", "16:30"
            };            // Filter out booked slots and format for display
            var availableSlots = allSlots
                .Select(slot => new 
                { 
                    value = slot,
                    text = DateTime.ParseExact(slot, "HH:mm", null).ToString("h:mm tt"),
                    available = !existingAppointments.Contains(slot)
                })
                .ToList();

            return Json(availableSlots);
        }

        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
