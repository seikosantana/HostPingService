using System.Diagnostics.Eventing.Reader;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostPingerService;

public class PingerService : BackgroundService
{
    private ILogger<PingerService> Logger { get; }
    private PingSettings? Settings { get; }
    private IHost Host { get; }

    public PingerService(ILogger<PingerService> logger, IConfiguration configuration, IHost host)
    {
        Logger = logger;
        Settings = configuration.GetSection("PingSettings").Get<PingSettings>();
        Host = host;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Pinger Service Starting");
        Logger.LogInformation("Loading Configuration");
        bool canStart = true;
        if (Settings is null)
        {
            Logger.LogError("Missing PingSettings configuration");
            canStart = false;
        }
        else if (Settings.Hosts is null || Settings.Hosts.Count == 0)
        {
            Logger.LogError("Missing or empty PingSettings Hosts configuration");
            canStart = false;
        }
        else
        {
            if (!Settings.IntervalMs.HasValue)
            {
                Logger.LogWarning("IntervalMs is missing. Using default 60s (60000ms)");
                Settings.IntervalMs = 60000;
            }

            if (!Settings.TimeoutMs.HasValue)
            {
                Logger.LogWarning("TimeoutMs is missing. Using default 5s (5000ms)");
                Settings.TimeoutMs = 5000;
            }
        }

        if (!canStart)
        {
            Logger.LogError("Service cannot continue without PingSettings specified");
            await Host.StopAsync(stoppingToken);
        }

        using Ping pinger = new Ping();
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (string host in Settings!.Hosts!)
            {
                try
                {
                    Logger.LogInformation("Pinging {Host}", host);
                    PingReply reply = await pinger.SendPingAsync(host, Settings!.TimeoutMs!.Value);
                    if (reply.Status == IPStatus.Success)
                    {
                        Logger.LogInformation("Ping {Host} success, {Ms}ms", host, reply.RoundtripTime);
                    }
                    else
                    {
                        Logger.LogError("Ping {Host} {Status}, {Ms}ms",
                            host,
                            Enum.GetName(reply.Status),
                            reply.RoundtripTime
                        );
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unable to perform ping {Host}", host);
                }
            }
            Logger.LogInformation("Next ping in {Ms}ms", Settings.IntervalMs!.Value);
            await Task.Delay(Settings!.IntervalMs!.Value, stoppingToken);
        }
    }
}