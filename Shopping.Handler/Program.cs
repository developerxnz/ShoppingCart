using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Shopping.Handler;

var host = new HostBuilder()
    .ConfigureWebHost(x =>
    {
        
    })
    .ConfigureFunctionsWebApplication(x => { })
    .ConfigureServices(x =>
    {
        x.AddWebJobs(j => { return; });
    })
    .ConfigureAppConfiguration(x => { })
    .ConfigureWebHost(x =>
    {
        x.Configure(f => { });
        x.AddExtension<BindingExtensionProvider>();
    })
    .Build();

host.Run();