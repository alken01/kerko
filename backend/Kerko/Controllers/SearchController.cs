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
    public async Task<IActionResult> Kerko([FromQuery] string? emri, [FromQuery] string? mbiemri)
    {
        try
        {
            var clientIp = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            _logger.LogInformation("Search request - Name: {emri} {mbiemri} | IP: {clientIp} | UserAgent: {userAgent}",
                emri ?? "N/A", mbiemri ?? "N/A", clientIp, userAgent);

            var result = await _searchService.KerkoAsync(mbiemri, emri);
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
    public async Task<IActionResult> Targat([FromQuery] string? numriTarges)
    {
        try
        {
            var clientIp = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            _logger.LogInformation("License plate search - Targa: {numriTarges} | IP: {clientIp} | UserAgent: {userAgent}",
                numriTarges ?? "N/A", clientIp, userAgent);

            var result = await _searchService.TargatAsync(numriTarges);
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
    public async Task<IActionResult> Telefon([FromQuery] string? numriTelefonit)
    {
        try
        {
            var clientIp = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();
            _logger.LogInformation("Phone search - Telefon: {numriTelefonit} | IP: {clientIp} | UserAgent: {userAgent}",
                numriTelefonit ?? "N/A", clientIp, userAgent);

            var result = await _searchService.TelefonAsync(numriTelefonit);
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
}
