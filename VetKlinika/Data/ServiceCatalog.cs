using Microsoft.EntityFrameworkCore;
using VetKlinika.Models;

namespace VetKlinika.Data;

public class ServiceCatalog(ApplicationDbContext context)
{
    public Task<List<Service>> ListAsync(
        string? searchString, string? sortOrder, CancellationToken cancellationToken = default)
    {
        var services = context.Services.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var keyword = searchString.Trim();
            services = services.Where(service => service.ServiceName.Contains(keyword) ||
                (service.Description != null && service.Description.Contains(keyword)));
        }

        var sorted = sortOrder switch
        {
            "price_asc" => services.OrderBy(service => service.Price),
            "price_desc" => services.OrderByDescending(service => service.Price),
            "name_desc" => services.OrderByDescending(service => service.ServiceName),
            _ => services.OrderBy(service => service.ServiceName)
        };

        return sorted.ThenBy(service => service.Id).ToListAsync(cancellationToken);
    }

    public Task<Service?> FindAsync(int id, CancellationToken cancellationToken = default) =>
        context.Services.AsNoTracking().SingleOrDefaultAsync(service => service.Id == id, cancellationToken);
}
