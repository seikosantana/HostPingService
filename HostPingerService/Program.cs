// See https://aka.ms/new-console-template for more information

using HostPingerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

IHostBuilder builder = Host.CreateDefaultBuilder(args);
builder.UseContentRoot(AppContext.BaseDirectory);
if (OperatingSystem.IsWindows())
{
    builder.UseWindowsService();
}

builder.ConfigureServices(services => { services.AddHostedService<PingerService>(); }
);

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    if (OperatingSystem.IsWindows())
    {
        logging.AddEventLog(new EventLogSettings()
        {
            SourceName = "HostPinger"
        });
    }
});

IHost app = builder.Build();
await app.RunAsync();