namespace Kerko.Http;

public static class ClientInfo
{
    public static string GetClientIpAddress(HttpRequest request)
    {
        // API Gateway can't override X-Forwarded-For, so it sends X-Client-IP instead
        var xClientIp = request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xClientIp))
        {
            return xClientIp.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
        }

        var xForwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    public static string SimplifyUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Detect browser. Order matters: Chromium-derivatives (Edge, Opera, Brave,
        // Samsung) all include "Chrome" in their UA, so check them before falling
        // through to plain Chrome.
        var browser = "Unknown";
        if (userAgent.Contains("Edg"))
            browser = "Edge";
        else if (userAgent.Contains("OPR") || userAgent.Contains("Opera"))
            browser = "Opera";
        else if (userAgent.Contains("SamsungBrowser"))
            browser = "Samsung Internet";
        else if (userAgent.Contains("Brave"))
            browser = "Brave";
        else if (userAgent.Contains("Firefox"))
            browser = "Firefox";
        else if (userAgent.Contains("Chrome"))
            browser = "Chrome";
        else if (userAgent.Contains("Safari"))
            browser = "Safari";

        // Detect OS
        var os = "Unknown";
        if (userAgent.Contains("Windows"))
            os = "Windows";
        else if (userAgent.Contains("Mac OS X") || userAgent.Contains("Macintosh"))
            os = "macOS";
        else if (userAgent.Contains("Linux"))
            os = "Linux";
        else if (userAgent.Contains("Android"))
            os = "Android";
        else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
            os = "iOS";

        // Detect if mobile
        var isMobile = userAgent.Contains("Mobile") || userAgent.Contains("Android") ||
                       userAgent.Contains("iPhone") || userAgent.Contains("iPad");
        var deviceType = isMobile ? "Mobile" : "Desktop";

        return $"{browser}/{os}/{deviceType}";
    }
}
