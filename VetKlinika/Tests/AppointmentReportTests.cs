using ClosedXML.Excel;
using Moq;
using VetKlinika.Reporting;
using Xunit;

namespace VetKlinika.Tests;

public class AppointmentReportTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = Start.AddYears(1);
    private readonly Mock<IAppointmentReportRepository> _repository = new(MockBehavior.Strict);

    [Fact]
    public async Task EmptyYearHasTwelveZeroMonthsAndNoServices()
    {
        SetUp([], []);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        Assert.Equal(2026, report.Year);
        Assert.Equal(Enumerable.Range(1, 12), report.Months.Select(month => month.Month));
        Assert.All(report.Months, month => Assert.Equal(0, month.Count));
        Assert.Empty(report.Services);
    }

    [Fact]
    public async Task BookingsArePlacedInTheCorrectMonths()
    {
        SetUp([new(12, 4), new(2, 3)], []);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        Assert.Equal(3, report.Months[1].Count);
        Assert.Equal(0, report.Months[2].Count);
        Assert.Equal(4, report.Months[11].Count);
    }

    [Fact]
    public async Task TotalComesFromMonthlyBookings()
    {
        SetUp([new(1, 2), new(12, 5)], []);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        Assert.Equal(7, report.TotalBookings);
    }

    [Fact]
    public async Task ServicesAreRankedByBookings()
    {
        SetUp([], [new("Vaccination", 2), new("Examination", 8), new("Dental care", 3)]);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        Assert.Equal(new[] { "Examination", "Dental care", "Vaccination" },
            report.Services.Select(service => service.Name));
    }

    [Fact]
    public async Task TiedServicesAreSortedByNameIgnoringCase()
    {
        SetUp([], [new("Vaccination", 3), new("dental care", 3)]);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        Assert.Equal("dental care", report.Services[0].Name);
    }

    [Fact]
    public async Task BothQueriesUseTheRequestedUtcYearAndCancellationToken()
    {
        using var cancellation = new CancellationTokenSource();
        SetUp([], [], cancellation.Token);

        await new AppointmentReportService(_repository.Object).GetAsync(2026, cancellation.Token);

        _repository.Verify(repository => repository.GetMonthlyBookingsAsync(Start, End, cancellation.Token), Times.Once);
        _repository.Verify(repository => repository.GetServiceBookingsAsync(Start, End, cancellation.Token), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9999)]
    public async Task InvalidYearDoesNotQueryTheRepository(int year)
    {
        var service = new AppointmentReportService(_repository.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetAsync(year));

        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExportContainsMonthlyTotalsAndServiceCounts()
    {
        SetUp([new(1, 3)], [new("Examination", 3)]);
        var report = await new AppointmentReportService(_repository.Object).GetAsync(2026);

        using var stream = new MemoryStream(ExcelReport.Create(report));
        using var workbook = new XLWorkbook(stream);

        Assert.Equal(2, workbook.Worksheets.Count);
        Assert.Equal("Jan 2026", workbook.Worksheet("Monthly bookings").Cell(2, 1).GetString());
        Assert.Equal(3, workbook.Worksheet("Monthly bookings").Cell(2, 2).GetValue<int>());
        Assert.Equal(3, workbook.Worksheet("Monthly bookings").Cell(14, 2).GetValue<int>());
        Assert.Equal("Examination", workbook.Worksheet("Service popularity").Cell(2, 1).GetString());
        Assert.Equal(3, workbook.Worksheet("Service popularity").Cell(2, 2).GetValue<int>());
    }

    private void SetUp(List<MonthlyBookings> months, List<ServiceBookings> services,
        CancellationToken cancellationToken = default)
    {
        _repository.Setup(repository => repository.GetMonthlyBookingsAsync(Start, End, cancellationToken))
            .ReturnsAsync(months);
        _repository.Setup(repository => repository.GetServiceBookingsAsync(Start, End, cancellationToken))
            .ReturnsAsync(services);
    }
}
