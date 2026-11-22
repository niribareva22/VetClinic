using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

if (args.Length > 2 ||
    !Uri.TryCreate(args.FirstOrDefault() ?? "http://localhost:5129", UriKind.Absolute, out var baseAddress) ||
    baseAddress.Scheme is not ("http" or "https"))
{
    Console.Error.WriteLine("Usage: VetKlinika.ApiClient [server-url] [search]");
    return 1;
}

var endpoint = new Uri(baseAddress, "/api/services");
if (args.Length == 2)
    endpoint = new UriBuilder(endpoint) { Query = $"search={Uri.EscapeDataString(args[1])}" }.Uri;

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
try
{
    var services = await http.GetFromJsonAsync<List<ServiceResponse>>(endpoint) ?? [];
    if (services.Count == 0)
        Console.WriteLine("No services found.");

    foreach (var service in services)
    {
        Console.WriteLine($"{service.Id}. {service.Name} | {service.Price.ToString("0.00", CultureInfo.InvariantCulture)} {service.Currency}");
        if (!string.IsNullOrWhiteSpace(service.Description))
            Console.WriteLine($"   {service.Description}");
    }

    return 0;
}
catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
{
    Console.Error.WriteLine($"Could not load the service catalog. {exception.Message}");
    return 1;
}

record ServiceResponse(int Id, string Name, string? Description, decimal Price, string Currency);
