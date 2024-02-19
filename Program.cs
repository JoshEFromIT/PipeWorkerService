using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<PipeWorkerService>();
    })
    .UseWindowsService() // Corrected the method name and removed the extra semicolon.
    .Build();

await host.RunAsync();

