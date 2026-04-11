using Microsoft.AspNetCore.Mvc;
using Kerko.Services;
using Kerko.Http;

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

    [HttpGet("version")]
    [ResponseCache(NoStore = true)]
    public IActionResult Version() => Ok(new { version = "1.1.0", deployed = DateTime.UtcNow.ToString("o") });

    [HttpGet("kerko")]
    public async Task<IActionResult> Kerko([FromQuery] string? emri, [FromQuery] string? mbiemri,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Search request | emri: {Emri} mbiemri: {Mbiemri} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", emri ?? "-", mbiemri ?? "-", pageNumber, pageSize, ClientInfo.GetClientIpAddress(Request), ClientInfo.SimplifyUserAgent(Request.Headers.UserAgent.ToString()));
            var result = await _searchService.KerkoAsync(mbiemri, emri, pageNumber, pageSize);
            HttpContext.Items["Kerko.ResultCount"] = result.Person.Pagination.TotalItems;
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
            _logger.LogInformation("Targat request | numriTarges: {NumriTarges} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", numriTarges ?? "-", pageNumber, pageSize, ClientInfo.GetClientIpAddress(Request), ClientInfo.SimplifyUserAgent(Request.Headers.UserAgent.ToString()));

            var result = await _searchService.TargatAsync(numriTarges, pageNumber, pageSize);
            HttpContext.Items["Kerko.ResultCount"] = result.Pagination.TotalItems;
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
            _logger.LogInformation("Telefon request | numriTelefonit: {NumriTelefonit} page: {PageNumber}/{PageSize} | IP: {IP} | {UA}", numriTelefonit ?? "-", pageNumber, pageSize, ClientInfo.GetClientIpAddress(Request), ClientInfo.SimplifyUserAgent(Request.Headers.UserAgent.ToString()));
            var result = await _searchService.TelefonAsync(numriTelefonit, pageNumber, pageSize);
            HttpContext.Items["Kerko.ResultCount"] = result.Pagination.TotalItems;
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
}
