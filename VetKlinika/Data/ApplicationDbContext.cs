using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Models;

namespace VetKlinika.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Vet> Vets { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public IQueryable<Client> VisibleClients(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Vet"))
                return Clients;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Clients.Where(client => false);

            return Clients.Where(client => client.IdentityUserId == userId);
        }

        public IQueryable<Patient> VisiblePatients(ClaimsPrincipal user)
        {
            var clients = VisibleClients(user);
            return Patients.Where(patient => clients.Any(client => client.Id == patient.ClientId));
        }

        public IQueryable<Appointment> VisibleAppointments(ClaimsPrincipal user)
        {
            var clients = VisibleClients(user);
            return Appointments.Where(appointment =>
                clients.Any(client => client.Id == appointment.ClientId) &&
                clients.Any(client => client.Id == appointment.Patient!.ClientId));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .HasOne<IdentityUser>()
                .WithOne()
                .HasForeignKey<Client>(client => client.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Patients)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Vet)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}