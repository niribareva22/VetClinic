using Microsoft.AspNetCore.Diagnostics;

namespace VetKlinika.Infrastructure;

public class RequestExceptionHandler(IWebHostEnvironment environment, ILogger<RequestExceptionHandler> logger)
    : IExceptionHandler
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var logDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "logs");

        await _writeLock.WaitAsync(CancellationToken.None);
        try
        {
            Directory.CreateDirectory(logDirectory);
            var logFile = Path.Combine(logDirectory, $"errors-{now:yyyy-MM-dd}.log");
            await File.AppendAllTextAsync(logFile,
                $"[{now:O}] {context.TraceIdentifier} {context.Request.Method}{Environment.NewLine}" +
                $"{exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception logError) when (logError is IOException or UnauthorizedAccessException)
        {
            logger.LogError(logError, "Could not write the error log for request {RequestId}.", context.TraceIdentifier);
        }
        finally
        {
            _writeLock.Release();
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
            return false;

        await Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
            title: "The request could not be completed.").ExecuteAsync(context);
        return true;
    }
}
