using System.Security.Cryptography;
using System.Text;

namespace Kerko.Authentication;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";
    
    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var endpoint = context.GetEndpoint();
        var requiresApiKey = endpoint?.Metadata.GetMetadata<RequireApiKeyAttribute>() != null;
        
        if (requiresApiKey)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "API Key missing" });
                return;
            }

            var apiKey = configuration["AdminApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(apiKey), 
                Encoding.UTF8.GetBytes(extractedApiKey.ToString())))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid API Key" });
                return;
            }
        }

        await _next(context);
    }
}

public static class ApiKeyAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyAuthMiddleware>();
    }
} 

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireApiKeyAttribute : Attribute
{
} 