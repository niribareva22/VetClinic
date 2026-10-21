using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

namespace VetKlinika.Controllers
{
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

            var vet = await _context.Vets
                .FirstOrDefaultAsync(m => m.VetID == id);
            if (vet == null) return NotFound();

            return View(vet);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VetID,FirstName,LastName,Specialization")] Vet vet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vet);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vet = await _context.Vets.FindAsync(id);
            if (vet == null) return NotFound();
            return View(vet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VetID,FirstName,LastName,Specialization")] Vet vet)
        {
            if (id != vet.VetID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vets.Any(e => e.VetID == vet.VetID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vet);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vet = await _context.Vets
                .FirstOrDefaultAsync(m => m.VetID == id);
            if (vet == null) return NotFound();

            return View(vet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vet = await _context.Vets.FindAsync(id);
            if (vet != null)
            {
                _context.Vets.Remove(vet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}