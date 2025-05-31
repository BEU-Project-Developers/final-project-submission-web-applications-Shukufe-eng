using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace HospitalManagementSystem.Controllers
{    public class PatientController : Controller
    {
        private readonly HospitalDbContext _context;

        public PatientController(HospitalDbContext context)
        {
            _context = context;
        }

        private async Task SetCurrentPatientInViewBag()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId != null)
            {
                var patient = await _context.Patients.FindAsync(patientId.Value);
                ViewBag.CurrentPatient = patient;
            }
        }

        // Patient Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            await SetCurrentPatientInViewBag();

            var patient = await _context.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.LabResults)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View(patient);
        }        // Get patient appointments
        public async Task<IActionResult> Appointments()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            await SetCurrentPatientInViewBag();

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }        // Get patient lab results
        public async Task<IActionResult> LabResults()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }            await SetCurrentPatientInViewBag();            var labResults = await _context.LabResults
                .Where(lr => lr.PatientId == patientId)
                .OrderByDescending(lr => lr.DateOrdered)
                .ToListAsync();

            return View(labResults);
        }

        // Book appointment
        [HttpGet]
        public async Task<IActionResult> BookAppointment()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            await SetCurrentPatientInViewBag();            var viewModel = new BookAppointmentViewModel
            {
                Doctors = await _context.Doctors
                    .Where(d => d.IsAvailable)
                    .ToListAsync(),
                Departments = await _context.Doctors
                    .Where(d => d.IsAvailable)
                    .Select(d => d.Specialization)
                    .Distinct()
                    .ToListAsync()
            };

            return View(viewModel);
        }        [HttpPost]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }            if (ModelState.IsValid)
            {
                // Parse the appointment time
                var appointmentDateTime = model.AppointmentDate.Date.Add(TimeSpan.Parse(model.AppointmentTime));                var appointment = new Appointment
                {
                    PatientId = patientId.Value,
                    DoctorId = model.DoctorId,
                    AppointmentDate = appointmentDateTime,
                    AppointmentTime = model.AppointmentTime,
                    Department = model.Department,
                    Reason = model.ReasonForVisit,
                    Notes = model.Notes,
                    Status = AppointmentStatus.Scheduled,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Appointment booked successfully!";
                return RedirectToAction("Appointments");
            }            // If we got this far, something failed, redisplay form
            model.Doctors = await _context.Doctors
                .Where(d => d.IsAvailable)
                .ToListAsync();
            model.Departments = await _context.Doctors
                .Where(d => d.IsAvailable)
                .Select(d => d.Specialization)
                .Distinct()
                .ToListAsync();

            return View(model);
        }

        // Cancel appointment
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId);

            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Appointment cancelled successfully!";
            }

            return RedirectToAction("Appointments");
        }        // Patient Profile
        public async Task<IActionResult> Profile()
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            await SetCurrentPatientInViewBag();

            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .Include(p => p.LabResults)
                .FirstOrDefaultAsync(p => p.Id == patientId);
            
            if (patient == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Patient model)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var existingPatient = await _context.Patients.FindAsync(patientId);
            if (existingPatient == null)
            {
                return RedirectToAction("Login", "Home");
            }            // Update patient information
            var nameParts = model.Name?.Split(' ', 2) ?? new[] { "", "" };
            existingPatient.FirstName = nameParts[0];
            existingPatient.LastName = nameParts.Length > 1 ? nameParts[1] : "";
            existingPatient.Email = model.Email;
            existingPatient.PhoneNumber = model.Phone;
            existingPatient.Address = model.Address;
            existingPatient.DateOfBirth = model.DateOfBirth;
            existingPatient.Gender = model.Gender;
            existingPatient.BloodType = model.BloodType;
            existingPatient.EmergencyContact = model.EmergencyContact;
            existingPatient.MedicalHistory = model.MedicalHistory;
            existingPatient.Insurance = model.Insurance;

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            await SetCurrentPatientInViewBag();
            return View("Profile", existingPatient);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation password do not match.";
                return RedirectToAction("Profile");
            }

            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null || patient.Password != HashPassword(CurrentPassword))
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            patient.Password = HashPassword(NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Download lab result
        public async Task<IActionResult> DownloadLabResult(int id)
        {
            var patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var labResult = await _context.LabResults
                .Include(lr => lr.Patient)
                .FirstOrDefaultAsync(lr => lr.Id == id && lr.PatientId == patientId);

            if (labResult == null)
            {
                return NotFound();
            }

            // Generate a simple text report
            var content = $@"LAB RESULT REPORT
===========================

Patient: {labResult.Patient.FirstName} {labResult.Patient.LastName}
Test Type: {labResult.TestType}
Date Ordered: {labResult.DateOrdered:yyyy-MM-dd}
Date Completed: {labResult.DateCompleted:yyyy-MM-dd}
Status: {labResult.Status}

Results:
{labResult.Results}

Notes:
{labResult.Notes}

Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";

            var bytes = Encoding.UTF8.GetBytes(content);
            var fileName = $"LabResult_{labResult.TestType}_{labResult.DateCompleted:yyyyMMdd}.txt";
            
            return File(bytes, "text/plain", fileName);
        }

        // Get available time slots for a doctor on a specific date
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            // Get existing appointments for the doctor on the specified date
            var existingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.NoShow)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Define available time slots with 30-minute intervals
            var allSlots = new List<string>
            {
                "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "14:00", "14:30", "15:00", "15:30", "16:00", "16:30"
            };

            // Filter out booked slots and format for display
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
