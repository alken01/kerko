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

    [HttpGet("version")]
    [ResponseCache(NoStore = true)]
    public IActionResult Version() => Ok(new { sha = Environment.GetEnvironmentVariable("GIT_SHA") ?? "unknown", deployed = Environment.GetEnvironmentVariable("DEPLOY_TIME") ?? "unknown" });

    [HttpGet("kerko")]
    public async Task<IActionResult> Kerko([FromQuery] string? emri, [FromQuery] string? mbiemri,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
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
            _logger.LogError(ex, "Error searching kerko");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("targat")]
    public async Task<IActionResult> Targat([FromQuery] string? numriTarges,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
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
            _logger.LogError(ex, "Error searching targat");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("telefon")]
    public async Task<IActionResult> Telefon([FromQuery] string? numriTelefonit,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
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
            _logger.LogError(ex, "Error searching telefon");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
