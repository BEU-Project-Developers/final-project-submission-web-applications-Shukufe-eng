using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Data
{
    public class HospitalDbContext : DbContext
    {
        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<LabResult> LabResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            // Configure Doctor entity
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Status).HasConversion<string>();

                // Configure relationships
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LabResult entity
            modelBuilder.Entity<LabResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Status).HasConversion<string>();

                // Configure relationships
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.LabResults)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default admin patient for testing
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@hospital.com",
                    PhoneNumber = "+994551234567",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "Other",
                    Address = "Hospital Admin Office",
                    BloodType = "O+",
                    EmergencyContact = "+994551234567",
                    MedicalHistory = "N/A",
                    Insurance = "Admin Insurance",
                    Password = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // Password: admin123 (hashed)
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Doctors
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    FirstName = "Walter",
                    LastName = "White",
                    Email = "walter.white@medilab.com",
                    PhoneNumber = "+994551234567",
                    Specialization = "Cardiology",
                    LicenseNumber = "MD001",
                    Bio = "Experienced medical leader focused on high patient care standards",
                    ImageUrl = "~/img/doctors/doctors-1.jpg",
                    IsAvailable = true,                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Doctor
                {
                    Id = 2,
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@medilab.com",
                    PhoneNumber = "+994551234568",
                    Specialization = "Neurology",
                    LicenseNumber = "MD002",
                    Bio = "Specialist in neurological disorders and brain surgery",
                    ImageUrl = "~/img/doctors/doctors-2.jpg",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Doctor
                {
                    Id = 3,
                    FirstName = "Michael",
                    LastName = "Brown",
                    Email = "michael.brown@medilab.com",
                    PhoneNumber = "+994551234569",
                    Specialization = "Pediatrics",
                    LicenseNumber = "MD003",
                    Bio = "Dedicated pediatrician with expertise in child healthcare",
                    ImageUrl = "~/img/doctors/doctors-3.jpg",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Doctor
                {
                    Id = 4,
                    FirstName = "Amanda",
                    LastName = "Davis",
                    Email = "amanda.davis@medilab.com",
                    PhoneNumber = "+994551234570",
                    Specialization = "Orthopedics",
                    LicenseNumber = "MD004",
                    Bio = "Expert in orthopedic surgery and sports medicine",
                    ImageUrl = "~/img/doctors/doctors-4.jpg",
                    IsAvailable = true,
                    CreatedAt = new DateTime(2024, 1, 1)                }
            );

            // Seed sample lab results for admin user
            modelBuilder.Entity<LabResult>().HasData(
                new LabResult
                {
                    Id = 1,
                    PatientId = 1,
                    TestName = "Complete Blood Count",
                    TestType = "Blood Test",
                    Results = "Normal values across all parameters",
                    Notes = "All blood components within normal range",
                    ReferenceRange = "Various",
                    Unit = "Various",
                    Status = LabResultStatus.Completed,
                    Comments = "Patient is healthy",
                    DateOrdered = new DateTime(2024, 5, 15),
                    DateCompleted = new DateTime(2024, 5, 16),
                    TestDate = new DateTime(2024, 5, 16),
                    ResultDate = new DateTime(2024, 5, 16),
                    CreatedAt = new DateTime(2024, 5, 16)
                },
                new LabResult
                {
                    Id = 2,
                    PatientId = 1,
                    TestName = "Cholesterol Panel",
                    TestType = "Blood Test",
                    Results = "Total: 180 mg/dL, LDL: 110 mg/dL, HDL: 50 mg/dL",
                    Notes = "Cholesterol levels are within acceptable range",
                    ReferenceRange = "Total <200 mg/dL",
                    Unit = "mg/dL",
                    Status = LabResultStatus.Completed,
                    Comments = "Continue healthy diet",
                    DateOrdered = new DateTime(2024, 4, 10),
                    DateCompleted = new DateTime(2024, 4, 11),
                    TestDate = new DateTime(2024, 4, 11),
                    ResultDate = new DateTime(2024, 4, 11),
                    CreatedAt = new DateTime(2024, 4, 11)
                }
            );
        }
    }
}
