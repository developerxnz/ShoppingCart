using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string provider = "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider";

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication((context, builder) =>
    {
        
    })
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
        
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule? defaultRule = 
                options
                    .Rules
                    .FirstOrDefault(rule => rule.ProviderName == provider);
            
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    
    .Build();

host.Run();