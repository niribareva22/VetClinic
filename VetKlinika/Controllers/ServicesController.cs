using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

[Authorize]
public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ServiceCatalog _catalog;

    public ServicesController(ApplicationDbContext context, ServiceCatalog catalog)
    {
        _context = context;
        _catalog = catalog;
    }

    public async Task<IActionResult> Index(string? searchString, string? sortOrder)
    {
        ViewData["SearchString"] = searchString;
        ViewData["PriceSort"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";
        ViewData["NameSort"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";

        return View(await _catalog.ListAsync(searchString, sortOrder, HttpContext.RequestAborted));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var service = await _catalog.FindAsync(id.Value, HttpContext.RequestAborted);
        if (service == null) return NotFound();
        return View(service);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Service service)
    {
        if (ModelState.IsValid)
        {
            _context.Add(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(service);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();
        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Service service)
    {
        if (id != service.Id) return NotFound();
        var existingService = await _context.Services.FindAsync(id);
        if (existingService == null) return NotFound();
        if (ModelState.IsValid)
        {
            existingService.ServiceName = service.ServiceName;
            existingService.Price = service.Price;
            existingService.Description = service.Description;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(service);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var service = await _catalog.FindAsync(id.Value, HttpContext.RequestAborted);
        if (service == null) return NotFound();
        return View(service);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();
        if (await _context.Appointments.AnyAsync(a => a.ServiceId == id))
        {
            ModelState.AddModelError(string.Empty, "This service has appointments and cannot be deleted.");
            return View("Delete", service);
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}