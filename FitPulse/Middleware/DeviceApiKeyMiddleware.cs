

namespace FitPulse.Middleware;

public class DeviceApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, FitPulseDbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue("x-device-id", out var deviceIdHeader) ||
            !int.TryParse(deviceIdHeader, out var deviceId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid device id.");
            return;
        }

        if (!context.Request.Headers.TryGetValue("x-api-key", out var providedKeyHeader) ||
            string.IsNullOrWhiteSpace(providedKeyHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing API key.");
            return;
        }

        var device = await dbContext.Devices.FindAsync(deviceId);
        if (device is null || !device.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unknown or inactive device.");
            return;
        }

        var providedKeyBytes = Encoding.UTF8.GetBytes(providedKeyHeader.ToString());
        var storedKeyBytes = Encoding.UTF8.GetBytes(device.ApiKey);

        var keyMatches = providedKeyBytes.Length == storedKeyBytes.Length &&
                          CryptographicOperations.FixedTimeEquals(providedKeyBytes, storedKeyBytes);

        if (!keyMatches)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API key.");
            return;
        }

        await _next(context);
    }
}