using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Shopping.Orchestrator;

public class Handler
{
    private readonly ILogger _logger;

    public Handler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Handler>();
    }

    [Function("Handler")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
        FunctionContext executionContext)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}