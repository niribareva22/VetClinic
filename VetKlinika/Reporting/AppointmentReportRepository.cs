using Microsoft.EntityFrameworkCore;
using VetKlinika.Data;

namespace VetKlinika.Reporting;

public record MonthlyBookings(int Month, int Count);
public record ServiceBookings(string Name, int Count);

public interface IAppointmentReportRepository
{
    Task<List<MonthlyBookings>> GetMonthlyBookingsAsync(DateTime from, DateTime until, CancellationToken cancellationToken);
    Task<List<ServiceBookings>> GetServiceBookingsAsync(DateTime from, DateTime until, CancellationToken cancellationToken);
}

public class hAppointmentReportRepository(ApplicationDbContext context) : IAppointmentReportRepository
{
    public Task<List<MonthlyBookings>> GetMonthlyBookingsAsync(
        DateTime from, DateTime until, CancellationToken cancellationToken) =>
        context.Appointments
            .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime < until)
            .GroupBy(a => a.AppointmentDateTime.Month)
            .Select(group => new MonthlyBookings(group.Key, group.Count()))
            .ToListAsync(cancellationToken);

    public Task<List<ServiceBookings>> GetServiceBookingsAsync(
        DateTime from, DateTime until, CancellationToken cancellationToken) =>
        context.Appointments
            .Where(a => a.AppointmentDateTime >= from && a.AppointmentDateTime < until)
            .GroupBy(a => new { a.ServiceId, a.Service!.ServiceName })
            .Select(group => new ServiceBookings(group.Key.ServiceName, group.Count()))
            .ToListAsync(cancellationToken);
}
