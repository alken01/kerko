using Microsoft.AspNetCore.Mvc;
using Kerko.Services;
using Kerko.Authentication;

namespace Kerko.Controllers;

[ApiController]
[Route("api")]
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
            _logger.LogInformation($"Name: {emri ?? "N/A"} {mbiemri ?? "N/A"} | IP: {GetClientIpAddress()} | {SimplifyUserAgent(Request.Headers["User-Agent"].ToString())}");
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
            _logger.LogInformation($"Targa: {numriTarges} | IP: {GetClientIpAddress()} | {SimplifyUserAgent(Request.Headers["User-Agent"].ToString())}");

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
            _logger.LogInformation($"Telefon: {numriTelefonit ?? "N/A"} | IP: {GetClientIpAddress()} | {SimplifyUserAgent(Request.Headers["User-Agent"].ToString())}");
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

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("OK");
    }

    [HttpGet("search-logs")]
    [RequireApiKey]
    public async Task<IActionResult> GetSearchLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            _logger.LogInformation("Search logs accessed");
            var logs = await _searchService.GetSearchLogsAsync(startDate, endDate);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving search logs");
            return StatusCode(500, "An error occurred while retrieving search logs");
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

    private string SimplifyUserAgent(string? userAgent)
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
