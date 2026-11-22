using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;
using VetKlinika.Models;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ClientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.VisibleClients(User).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var client = await _context.VisibleClients(User).FirstOrDefaultAsync(m => m.Id == id);
        if (client == null) return NotFound();
        return View(client);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        await PopulateAccountsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Client client)
    {
        await ValidateAccountAsync(client.IdentityUserId, null);
        if (ModelState.IsValid)
        {
            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateAccountsAsync();
        return View(client);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();
        await PopulateAccountsAsync(client.Id);
        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id) return NotFound();
        var existingClient = await _context.Clients.FindAsync(id);
        if (existingClient == null) return NotFound();
        await ValidateAccountAsync(client.IdentityUserId, id);
        if (ModelState.IsValid)
        {
            existingClient.FirstName = client.FirstName;
            existingClient.LastName = client.LastName;
            existingClient.Email = client.Email;
            existingClient.Phone = client.Phone;
            existingClient.IdentityUserId = client.IdentityUserId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateAccountsAsync(id);
        return View(client);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
        if (client == null) return NotFound();
        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return NotFound();
        if (await _context.Patients.AnyAsync(p => p.ClientId == id) ||
            await _context.Appointments.AnyAsync(a => a.ClientId == id))
        {
            ModelState.AddModelError(string.Empty, "This client has patients or appointments and cannot be deleted.");
            return View("Delete", client);
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAccountsAsync(int? clientId = null)
    {
        var accounts = await AvailableAccounts(clientId).OrderBy(user => user.UserName).ToListAsync();
        ViewData["IdentityUserId"] = new SelectList(accounts, "Id", "UserName");
    }

    private async Task ValidateAccountAsync(string? userId, int? clientId)
    {
        if (userId != null && !await AvailableAccounts(clientId).AnyAsync(user => user.Id == userId))
            ModelState.AddModelError(nameof(Client.IdentityUserId), "Select an available client login account.");
    }

    private IQueryable<IdentityUser> AvailableAccounts(int? clientId) =>
        from user in _context.Users
        join membership in _context.UserRoles on user.Id equals membership.UserId
        join role in _context.Roles on membership.RoleId equals role.Id
        where role.Name == "Client" &&
              !_context.Clients.Any(client => client.IdentityUserId == user.Id && client.Id != clientId)
        select user;
}
