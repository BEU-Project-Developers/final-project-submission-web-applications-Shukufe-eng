using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly HospitalDbContext _context;

        public AdminController(HospitalDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var appointmentStats = new
            {
                Total = await _context.Appointments.CountAsync(),
                Scheduled = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Scheduled),
                Confirmed = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed),
                InProgress = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.InProgress),
                Completed = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed),
                Cancelled = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Cancelled),
                NoShow = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.NoShow)
            };

            ViewBag.AppointmentStats = appointmentStats;
            return View();
        }

        // Manage Appointments
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // Update Appointment Status
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Appointment status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating appointment status: " + ex.Message });
            }
        }

        // Update Appointment Notes
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentNotes(int appointmentId, string notes)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            appointment.Notes = notes ?? string.Empty;
            appointment.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Appointment notes updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating appointment notes: " + ex.Message });
            }
        }

        // Get Available Time Slots for a specific doctor and date
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            // Define all possible time slots
            var allTimeSlots = new List<string>
            {
                "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
                "12:00", "12:30", "14:00", "14:30", "15:00", "15:30",
                "16:00", "16:30", "17:00", "17:30"
            };

            // Get booked time slots for the doctor on the specified date
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date && 
                           a.Status != AppointmentStatus.Cancelled &&
                           a.Status != AppointmentStatus.NoShow)
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Filter out booked slots
            var availableSlots = allTimeSlots.Except(bookedSlots).ToList();

            return Json(availableSlots);
        }

        // Reschedule Appointment
        [HttpPost]
        public async Task<IActionResult> RescheduleAppointment(int appointmentId, DateTime newDate, string newTime)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            // Check if the new time slot is available
            var conflictingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.DoctorId == appointment.DoctorId &&
                                         a.AppointmentDate.Date == newDate.Date &&
                                         a.AppointmentTime == newTime &&
                                         a.Id != appointmentId &&
                                         a.Status != AppointmentStatus.Cancelled &&
                                         a.Status != AppointmentStatus.NoShow);

            if (conflictingAppointment != null)
            {
                return Json(new { success = false, message = "The selected time slot is already booked" });
            }

            appointment.AppointmentDate = newDate;
            appointment.AppointmentTime = newTime;
            appointment.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Appointment rescheduled successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error rescheduling appointment: " + ex.Message });
            }
        }
    }
}
