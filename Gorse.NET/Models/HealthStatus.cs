namespace Gorse.NET.Models;

/// <summary>Response of api/health/live and api/health/ready.</summary>
public class HealthStatus
{
    public bool Ready { get; set; }
    public bool DataStoreConnected { get; set; }
    public bool CacheStoreConnected { get; set; }
}
