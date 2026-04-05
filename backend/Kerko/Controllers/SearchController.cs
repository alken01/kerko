using Microsoft.AspNetCore.Mvc;
using Kerko.Services;

namespace Kerko.Controllers;

[ApiController]
[Route("api")]
[ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]
public class Controller : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<Controller> _logger;

    public Controller(ISearchService searchService, ILogger<Controller> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet("kerko")]
    public async Task<IActionResult> Kerko([FromQuery] string? emri, [FromQuery] string? mbiemri,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Search request | emri: {Emri} mbiemri: {Mbiemri} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", emri ?? "-", mbiemri ?? "-", pageNumber, pageSize, GetClientIpAddress(), SimplifyUserAgent(Request.Headers.UserAgent.ToString()));
            var result = await _searchService.KerkoAsync(mbiemri, emri, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("targat")]
    public async Task<IActionResult> Targat([FromQuery] string? numriTarges,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Targat request | numriTarges: {NumriTarges} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", numriTarges ?? "-", pageNumber, pageSize, GetClientIpAddress(), SimplifyUserAgent(Request.Headers.UserAgent.ToString()));

            var result = await _searchService.TargatAsync(numriTarges, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for targat");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("telefon")]
    public async Task<IActionResult> Telefon([FromQuery] string? numriTelefonit,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Telefon request | numriTelefonit: {NumriTelefonit} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", numriTelefonit ?? "-", pageNumber, pageSize, GetClientIpAddress(), SimplifyUserAgent(Request.Headers.UserAgent.ToString()));
            var result = await _searchService.TelefonAsync(numriTelefonit, pageNumber, pageSize);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for telefon");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    private string GetClientIpAddress()
    {
        var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',').FirstOrDefault()?.Trim() ?? "Unknown";
        }

        var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string SimplifyUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return "Unknown";
        }

        // Detect browser
        var browser = "Unknown";
        if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
            browser = "Chrome";
        else if (userAgent.Contains("Edg"))
            browser = "Edge";
        else if (userAgent.Contains("Firefox"))
            browser = "Firefox";
        else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
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
