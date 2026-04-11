using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Kerko.Admin;

public class AdminAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly string? _expectedToken = configuration["KERKO_ADMIN_TOKEN"];

    public async Task InvokeAsync(HttpContext context)
    {
        var expectedToken = _expectedToken;

        // Defense in depth: if the expected token is missing or empty for any reason,
        // refuse all requests. Program.cs already fails fast at startup when the env var
        // is unset in non-Testing environments, but we must never allow an "empty == empty"
        // FixedTimeEquals match to grant access.
        if (string.IsNullOrEmpty(expectedToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        var providedToken = context.Request.Headers["X-Admin-Token"].FirstOrDefault() ?? string.Empty;

        var expectedBytes = Encoding.UTF8.GetBytes(expectedToken);
        var providedBytes = Encoding.UTF8.GetBytes(providedToken);

        // Constant-time compare
        bool valid = expectedBytes.Length == providedBytes.Length &&
                     CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);

        if (!valid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    }
}
