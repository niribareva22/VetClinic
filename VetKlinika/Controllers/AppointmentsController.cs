using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? sortOrder)
    {
        ViewData["SortOrder"] = sortOrder;
        ViewData["NextDateSort"] = sortOrder == "date" ? "date_desc" : "date";
        IQueryable<Appointment> appointments = _context.VisibleAppointments(User)
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Service)
            .Include(a => a.Vet);
        appointments = sortOrder == "date_desc"
            ? appointments.OrderByDescending(a => a.AppointmentDateTime)
            : appointments.OrderBy(a => a.AppointmentDateTime);
        return View(await appointments.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.VisibleAppointments(User)
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Service)
            .Include(a => a.Vet)
            .FirstOrDefaultAsync(a => a.Id == id);
        return appointment == null ? NotFound() : View(appointment);
    }

    [Authorize(Roles = "Admin,Vet")]
    public IActionResult Create()
    {
        PopulateSelections();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        var patient = await _context.Patients.FindAsync(appointment.PatientId);
        if (patient == null)
        {
            ModelState.AddModelError(nameof(appointment.PatientId), "Select a valid patient.");
        }
        else
        {
            appointment.ClientId = patient.ClientId;
        }
        if (!await _context.Vets.AnyAsync(v => v.Id == appointment.VetId))
            ModelState.AddModelError(nameof(appointment.VetId), "Select a valid vet.");
        if (!await _context.Services.AnyAsync(s => s.Id == appointment.ServiceId))
            ModelState.AddModelError(nameof(appointment.ServiceId), "Select a valid service.");

        ValidateAppointmentDateTime(appointment.AppointmentDateTime);
        if (ModelState.IsValid)
        {
            appointment.AppointmentDateTime = appointment.AppointmentDateTime.ToUniversalTime();
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateSelections(appointment);
        return View(appointment);
    }

    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null) return NotFound();
        appointment.AppointmentDateTime = appointment.AppointmentDateTime.ToLocalTime();
        PopulateSelections(appointment);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();
        var existingAppointment = await _context.Appointments.FindAsync(id);
        if (existingAppointment == null) return NotFound();

        var patient = await _context.Patients.FindAsync(appointment.PatientId);
        if (patient == null)
            ModelState.AddModelError(nameof(appointment.PatientId), "Select a valid patient.");
        if (!await _context.Vets.AnyAsync(v => v.Id == appointment.VetId))
            ModelState.AddModelError(nameof(appointment.VetId), "Select a valid vet.");
        if (!await _context.Services.AnyAsync(s => s.Id == appointment.ServiceId))
            ModelState.AddModelError(nameof(appointment.ServiceId), "Select a valid service.");

        ValidateAppointmentDateTime(appointment.AppointmentDateTime);
        if (ModelState.IsValid && patient != null)
        {
            existingAppointment.PatientId = patient.Id;
            existingAppointment.ClientId = patient.ClientId;
            existingAppointment.VetId = appointment.VetId;
            existingAppointment.ServiceId = appointment.ServiceId;
            existingAppointment.AppointmentDateTime = appointment.AppointmentDateTime.ToUniversalTime();
            existingAppointment.Status = appointment.Status;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateSelections(appointment);
        return View(appointment);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Patient)
            .Include(a => a.Service)
            .Include(a => a.Vet)
            .FirstOrDefaultAsync(a => a.Id == id);
        return appointment == null ? NotFound() : View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateAppointmentDateTime(DateTime dateTime)
    {
        if (dateTime == default || TimeZoneInfo.Local.IsInvalidTime(dateTime))
            ModelState.AddModelError(nameof(Appointment.AppointmentDateTime), "Enter a valid appointment date and time.");
    }

    private void PopulateSelections(Appointment? appointment = null)
    {
        ViewData["PatientId"] = new SelectList(
            _context.Patients.OrderBy(p => p.Name), "Id", "Name", appointment?.PatientId);
        ViewData["VetId"] = new SelectList(
            _context.Vets.OrderBy(v => v.LastName)
                .Select(v => new { v.Id, Name = v.FirstName + " " + v.LastName }),
            "Id", "Name", appointment?.VetId);
        ViewData["ServiceId"] = new SelectList(
            _context.Services.OrderBy(s => s.ServiceName), "Id", "ServiceName", appointment?.ServiceId);
    }
}
