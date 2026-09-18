using Gorse.NET.Models;
using RestSharp;

namespace Gorse.NET;

public partial class Gorse
{
    /// <summary>Liveness probe: succeeds once the node serves HTTP.</summary>
    public Task<HealthStatus> GetLivenessAsync(CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<HealthStatus, object>(Method.Get, "api/health/live", null, cancellationToken)!;
    }

    /// <summary>
    /// Readiness probe: Gorse answers 503 (surfaced as a GorseException) while its
    /// data or cache store is unreachable.
    /// </summary>
    public Task<HealthStatus> GetReadinessAsync(CancellationToken cancellationToken = default)
    {
        return _client.RequestAsync<HealthStatus, object>(Method.Get, "api/health/ready", null, cancellationToken)!;
    }
}
