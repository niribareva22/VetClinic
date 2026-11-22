namespace VetKlinika.Reporting;

public record AppointmentReport(int Year, IReadOnlyList<MonthlyBookings> Months, IReadOnlyList<ServiceBookings> Services)
{
    public int TotalBookings => Months.Sum(month => month.Count);
}

public class AppointmentReportService(IAppointmentReportRepository repository)
{
    public async Task<AppointmentReport> GetAsync(int year, CancellationToken cancellationToken = default)
    {
        if (year is < 1 or > 9998)
            throw new ArgumentOutOfRangeException(nameof(year));

        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = from.AddYears(1);
        var bookings = await repository.GetMonthlyBookingsAsync(from, until, cancellationToken);
        var countsByMonth = bookings.ToDictionary(month => month.Month, month => month.Count);
        var months = Enumerable.Range(1, 12)
            .Select(month => new MonthlyBookings(month, countsByMonth.GetValueOrDefault(month)))
            .ToArray();

        var services = await repository.GetServiceBookingsAsync(from, until, cancellationToken);
        return new AppointmentReport(year, months, services
            .OrderByDescending(service => service.Count)
            .ThenBy(service => service.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray());
    }
}
