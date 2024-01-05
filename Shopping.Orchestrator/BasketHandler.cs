using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Shopping.Orchestrator;

public class BasketHandler
{
    private readonly ILogger _logger;

    public BasketHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Handler>();
    }

    //Trigger when item added to basket
    [Function("Handler")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}