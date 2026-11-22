using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetKlinika.Reporting;

[Authorize(Roles = "Admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ReportsController(AppointmentReportService reports) : Controller
{
    public async Task<IActionResult> Index(int? year, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || year is < 1 or > 9998)
            return BadRequest("Enter a year between 1 and 9998.");

        return View(await reports.GetAsync(year ?? DateTime.UtcNow.Year, cancellationToken));
    }

    public async Task<IActionResult> Export(int? year, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || year is < 1 or > 9998)
            return BadRequest("Enter a year between 1 and 9998.");

        var report = await reports.GetAsync(year ?? DateTime.UtcNow.Year, cancellationToken);
        return File(ExcelReport.Create(report),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"appointments-{report.Year}.xlsx");
    }
}
