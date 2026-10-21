using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

namespace VetKlinika.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments.Include(a => a.Patient).Include(a => a.Service).Include(a => a.Vet);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.AppointmentID == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        public IActionResult Create()
        {
            ViewData["PatientID"] = new SelectList(_context.Patients, "PatientID", "Name");
            ViewData["ServiceID"] = new SelectList(_context.Services, "ServiceID", "ServiceName");
            ViewData["VetID"] = new SelectList(_context.Vets, "VetID", "FirstName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentID,PatientID,VetID,ServiceID,AppointmentDateTime,Status")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientID"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
            ViewData["ServiceID"] = new SelectList(_context.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewData["VetID"] = new SelectList(_context.Vets, "VetID", "FirstName", appointment.VetID);
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            ViewData["PatientID"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
            ViewData["ServiceID"] = new SelectList(_context.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewData["VetID"] = new SelectList(_context.Vets, "VetID", "FirstName", appointment.VetID);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentID,PatientID,VetID,ServiceID,AppointmentDateTime,Status")] Appointment appointment)
        {
            if (id != appointment.AppointmentID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.AppointmentID == appointment.AppointmentID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientID"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
            ViewData["ServiceID"] = new SelectList(_context.Services, "ServiceID", "ServiceName", appointment.ServiceID);
            ViewData["VetID"] = new SelectList(_context.Vets, "VetID", "FirstName", appointment.VetID);
            return View(appointment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Vet)
                .FirstOrDefaultAsync(m => m.AppointmentID == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}