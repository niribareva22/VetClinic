using System.Globalization;
using ClosedXML.Excel;

namespace VetKlinika.Reporting;

public static class ExcelReport
{
    public static byte[] Create(AppointmentReport report)
    {
        using var workbook = new XLWorkbook();
        var months = workbook.AddWorksheet("Monthly bookings");
        months.Cell(1, 1).Value = "Month";
        months.Cell(1, 2).Value = "Bookings";
        foreach (var month in report.Months)
        {
            months.Cell(month.Month + 1, 1).Value = new DateTime(report.Year, month.Month, 1)
                .ToString("MMM yyyy", CultureInfo.InvariantCulture);
            months.Cell(month.Month + 1, 2).Value = month.Count;
        }

        months.Cell(14, 1).Value = "Total";
        months.Cell(14, 2).Value = report.TotalBookings;
        months.Row(14).Style.Font.Bold = true;

        var services = workbook.AddWorksheet("Service popularity");
        services.Cell(1, 1).Value = "Service";
        services.Cell(1, 2).Value = "Bookings";
        for (var index = 0; index < report.Services.Count; index++)
        {
            var service = report.Services[index];
            services.Cell(index + 2, 1).Value = service.Name;
            services.Cell(index + 2, 2).Value = service.Count;
        }

        foreach (var sheet in workbook.Worksheets)
        {
            sheet.Row(1).Style.Font.Bold = true;
            sheet.Column(1).Width = 40;
            sheet.Column(2).Width = 15;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
