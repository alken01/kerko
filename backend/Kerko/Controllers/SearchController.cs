using Kerko.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kerko.Controllers;

[ApiController]
[Route("api")]
[ResponseCache(Duration = 60, VaryByQueryKeys = ["*"])]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet("version")]
    [ResponseCache(NoStore = true)]
    public IActionResult Version()
    {
        return Ok(new
        {
            sha = Environment.GetEnvironmentVariable("GIT_SHA") ?? "unknown",
            deployed = Environment.GetEnvironmentVariable("DEPLOY_TIME") ?? "unknown"
        });
    }

    [HttpGet("kerko")]
    public async Task<IActionResult> Kerko([FromQuery] string? emri, [FromQuery] string? mbiemri,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await searchService.KerkoAsync(mbiemri, emri, pageNumber, pageSize);
        HttpContext.Items["Kerko.ResultCount"] = result.Person.Pagination.TotalItems;
        return Ok(result);
    }

    [HttpGet("targat")]
    public async Task<IActionResult> Targat([FromQuery] string? numriTarges,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await searchService.TargatAsync(numriTarges, pageNumber, pageSize);
        HttpContext.Items["Kerko.ResultCount"] = result.Pagination.TotalItems;
        return Ok(result);
    }

    [HttpGet("telefon")]
    public async Task<IActionResult> Telefon([FromQuery] string? numriTelefonit,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await searchService.TelefonAsync(numriTelefonit, pageNumber, pageSize);
        HttpContext.Items["Kerko.ResultCount"] = result.Pagination.TotalItems;
        return Ok(result);
    }
}
