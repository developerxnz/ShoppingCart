using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Shopping.Handler;

public static class HeaderHandler
{
    [Function("HeaderHandler")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
        FunctionContext executionContext)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }
        
        var logger = executionContext.GetLogger("HeaderHandler");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult($"Welcome to Azure Functions, using IActionResult!");
    }
}
