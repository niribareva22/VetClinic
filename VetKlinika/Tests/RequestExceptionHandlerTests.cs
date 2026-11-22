using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetKlinika.Infrastructure;
using Xunit;

namespace VetKlinika.Tests;

public class RequestExceptionHandlerTests
{
    [Fact]
    public async Task ApiErrorsAreLoggedWithoutExposingExceptionDetails()
    {
        var directory = Directory.CreateTempSubdirectory("vetklinika-errors-");
        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(host => host.ContentRootPath).Returns(directory.FullName);
            using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
            using var response = new MemoryStream();
            var context = new DefaultHttpContext { RequestServices = services, TraceIdentifier = "test-request" };
            context.Request.Path = "/api/services";
            context.Response.Body = response;
            var handler = new RequestExceptionHandler(environment.Object, NullLogger<RequestExceptionHandler>.Instance);

            Assert.True(await handler.TryHandleAsync(context, new InvalidOperationException("Internal failure"), default));
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            response.Position = 0;
            using var problem = await JsonDocument.ParseAsync(response);
            Assert.Equal("The request could not be completed.", problem.RootElement.GetProperty("title").GetString());
            Assert.DoesNotContain("Internal failure", problem.RootElement.GetRawText());

            var log = Assert.Single(Directory.GetFiles(Path.Combine(directory.FullName, "App_Data", "logs")));
            var contents = await File.ReadAllTextAsync(log);
            Assert.Contains("test-request", contents);
            Assert.Contains("Internal failure", contents);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
