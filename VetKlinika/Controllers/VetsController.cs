using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

[Authorize]
public class VetsController : Controller
{
    private readonly ApplicationDbContext _context;

    public VetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Vets.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var vet = await _context.Vets.FirstOrDefaultAsync(m => m.Id == id);
        if (vet == null) return NotFound();
        return View(vet);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Vet vet)
    {
        if (ModelState.IsValid)
        {
            _context.Add(vet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vet);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var vet = await _context.Vets.FindAsync(id);
        if (vet == null) return NotFound();
        return View(vet);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Vet vet)
    {
        if (id != vet.Id) return NotFound();
        var existingVet = await _context.Vets.FindAsync(id);
        if (existingVet == null) return NotFound();
        if (ModelState.IsValid)
        {
            existingVet.FirstName = vet.FirstName;
            existingVet.LastName = vet.LastName;
            existingVet.Specialization = vet.Specialization;
            existingVet.Email = vet.Email;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vet);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var vet = await _context.Vets.FirstOrDefaultAsync(m => m.Id == id);
        if (vet == null) return NotFound();
        return View(vet);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vet = await _context.Vets.FindAsync(id);
        if (vet == null) return NotFound();
        if (await _context.Appointments.AnyAsync(a => a.VetId == id))
        {
            ModelState.AddModelError(string.Empty, "This vet has appointments and cannot be deleted.");
            return View("Delete", vet);
        }

        _context.Vets.Remove(vet);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}