using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kerko.Infrastructure;

public class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentException ae:
                context.Result = new BadRequestObjectResult(ae.Message);
                context.ExceptionHandled = true;
                return;
            default:
                logger.LogError(context.Exception,
                    "Unhandled exception in {Action}",
                    context.ActionDescriptor.DisplayName);
                context.Result = new ObjectResult("An error occurred while processing your request")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                context.ExceptionHandled = true;
                return;
        }
    }
}
