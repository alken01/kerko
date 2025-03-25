using Microsoft.AspNetCore.Mvc;
using Kerko.Services;

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
            _logger.LogInformation("Kerko: emri: {emri}, mbiemri: {mbiemri}, {IpAddress}", 
                emri, mbiemri, HttpContext.Connection.RemoteIpAddress);

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
            _logger.LogInformation("Targat: numriTarges: {numriTarges}, {IpAddress}", 
                numriTarges, HttpContext.Connection.RemoteIpAddress);

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


    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok("OK");
    }

    // TODO: fix later
    // [HttpGet("db-status")]
    // public async Task<IActionResult> DbStatus()
    // {
    //     try
    //     {
    //         var result = await _searchService.DbStatusAsync();
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error occurred while checking database status");
    //         return StatusCode(500, "An error occurred while checking database status");
    //     }
    // }
} 
