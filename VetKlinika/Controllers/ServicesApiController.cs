using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinika.Data;
using VetKlinika.Models;

[ApiController]
[AllowAnonymous]
[Route("api/services")]
public class ServicesApiController(ServiceCatalog catalog) : ControllerBase
{
    public record ServiceResponse(int Id, string Name, string? Description, decimal Price, string Currency);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> List(
        string? search, CancellationToken cancellationToken)
    {
        var services = await catalog.ListAsync(search, null, cancellationToken);
        return Ok(services.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse>> Get(int id, CancellationToken cancellationToken)
    {
        var service = await catalog.FindAsync(id, cancellationToken);
        return service == null ? NotFound() : ToResponse(service);
    }

    private static ServiceResponse ToResponse(Service service) =>
        new(service.Id, service.ServiceName, service.Description, service.Price, "EUR");
}
