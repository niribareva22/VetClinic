using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

[Authorize]
public class PatientsController : Controller
{
    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PatientsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(string? searchString, string? speciesFilter, string? sortOrder, int pageNumber = 1)
    {
        const int pageSize = 10;
        var patients = _context.Patients.Include(p => p.Client).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var keyword = searchString.Trim();
            patients = patients.Where(p =>
                p.Name.Contains(keyword) ||
                (p.Breed != null && p.Breed.Contains(keyword)) ||
                p.Species.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(speciesFilter))
        {
            patients = patients.Where(p => p.Species == speciesFilter);
        }

        patients = sortOrder switch
        {
            "name_desc" => patients.OrderByDescending(p => p.Name),
            "species" => patients.OrderBy(p => p.Species).ThenBy(p => p.Name),
            "species_desc" => patients.OrderByDescending(p => p.Species).ThenBy(p => p.Name),
            "age" => patients.OrderBy(p => p.Age).ThenBy(p => p.Name),
            "age_desc" => patients.OrderByDescending(p => p.Age).ThenBy(p => p.Name),
            _ => patients.OrderBy(p => p.Name)
        };

        var totalCount = await patients.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        pageNumber = totalPages == 0 ? 1 : Math.Clamp(pageNumber, 1, totalPages);

        ViewData["SearchString"] = searchString;
        ViewData["SpeciesFilter"] = speciesFilter;
        ViewData["SortOrder"] = sortOrder;
        ViewData["PageNumber"] = pageNumber;
        ViewData["TotalPages"] = totalPages;
        ViewData["SpeciesList"] = new SelectList(
            await _context.Patients.Select(p => p.Species).Distinct().OrderBy(s => s).ToListAsync(),
            speciesFilter);

        return View(await patients.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var patient = await _context.Patients.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id);
        return patient == null ? NotFound() : View(patient);
    }

    [Authorize(Roles = "Admin,Vet")]
    public IActionResult Create()
    {
        PopulateClients();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Create(Patient patient)
    {
        await ValidatePhotoAsync(patient);
        if (!await _context.Clients.AnyAsync(c => c.Id == patient.ClientId))
        {
            ModelState.AddModelError(nameof(patient.ClientId), "Select a valid client.");
        }

        if (ModelState.IsValid)
        {
            await SavePhotoAsync(patient);
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateClients(patient.ClientId);
        return View(patient);
    }

    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();
        PopulateClients(patient.ClientId);
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Vet")]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.Id) return NotFound();
        var existingPatient = await _context.Patients.FindAsync(id);
        if (existingPatient == null) return NotFound();

        await ValidatePhotoAsync(patient);
        if (!await _context.Clients.AnyAsync(c => c.Id == patient.ClientId))
        {
            ModelState.AddModelError(nameof(patient.ClientId), "Select a valid client.");
        }

        if (ModelState.IsValid)
        {
            existingPatient.Name = patient.Name;
            existingPatient.Species = patient.Species;
            existingPatient.Breed = patient.Breed;
            existingPatient.Age = patient.Age;
            existingPatient.ClientId = patient.ClientId;
            if (patient.PhotoFile is { Length: > 0 })
            {
                await SavePhotoAsync(patient);
                existingPatient.PhotoPath = patient.PhotoPath;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        patient.PhotoPath = existingPatient.PhotoPath;
        PopulateClients(patient.ClientId);
        return View(patient);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var patient = await _context.Patients.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id);
        return patient == null ? NotFound() : View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();
        if (await _context.Appointments.AnyAsync(a => a.PatientId == id))
        {
            ModelState.AddModelError(string.Empty, "This patient has appointments and cannot be deleted.");
            return View("Delete", await _context.Patients.Include(p => p.Client).FirstAsync(p => p.Id == id));
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateClients(int? selectedId = null)
    {
        ViewData["ClientId"] = new SelectList(
            _context.Clients.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName }),
            "Id",
            "Name",
            selectedId);
    }

    private async Task ValidatePhotoAsync(Patient patient)
    {
        var photo = patient.PhotoFile;
        if (photo == null || photo.Length == 0) return;

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var allowedTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp"
        };

        if (photo.Length > MaxPhotoSize)
        {
            ModelState.AddModelError(nameof(patient.PhotoFile), "The image must be 5 MB or smaller.");
            return;
        }

        if (!allowedTypes.TryGetValue(extension, out var expectedContentType) ||
            !string.Equals(photo.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(patient.PhotoFile), "Upload a JPEG, PNG, GIF, or WebP image.");
            return;
        }

        var header = new byte[12];
        await using (var stream = photo.OpenReadStream())
        {
            var bytesRead = await stream.ReadAsync(header.AsMemory());
            if (!HasValidImageSignature(header, bytesRead, extension))
            {
                ModelState.AddModelError(nameof(patient.PhotoFile), "The uploaded file is not a valid image.");
                return;
            }
        }

    }

    private async Task SavePhotoAsync(Patient patient)
    {
        var photo = patient.PhotoFile;
        if (photo == null || photo.Length == 0) return;

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);
        await using (var output = System.IO.File.Create(filePath))
        await using (var input = photo.OpenReadStream())
        {
            await input.CopyToAsync(output);
        }

        patient.PhotoPath = $"/uploads/{fileName}";
    }

    private static bool HasValidImageSignature(byte[] header, int bytesRead, string extension) =>
        extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".gif" => bytesRead >= 6 &&
                      (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
            ".webp" => bytesRead >= 12 &&
                       header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                       header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
}
