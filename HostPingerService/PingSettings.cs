namespace HostPingerService;

public class PingSettings
{
    public List<string>? Hosts { get; set; }
    public int? IntervalMs { get; set; }
    public int? TimeoutMs { get; set; }
}