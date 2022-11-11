// See https://aka.ms/new-console-template for more information

using HostPingerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args);
if (OperatingSystem.IsWindows())
{
    builder.UseWindowsService();
}

builder.ConfigureServices(services =>
    {
        services.AddHostedService<PingerService>();
    }
);

IHost app = builder.Build();
await app.RunAsync();