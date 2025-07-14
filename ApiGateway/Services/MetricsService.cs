using System.Collections.Concurrent;

namespace ApiGateway.Services;

public class MetricsService
{
    private readonly ConcurrentDictionary<string, ServiceStats> _stats = new();

    public void RecordRequest(string service, string method, string path, string user, long elapsedMs)
    {
        var stat = _stats.GetOrAdd(service, _ => new ServiceStats(service));
        stat.TotalRequests++;
        stat.TotalElapsedTime += elapsedMs;
        stat.LastCalled = DateTime.UtcNow;

        stat.MethodCounts.AddOrUpdate(method, 1, (_, v) => v + 1);
        stat.Users.AddOrUpdate(user, 1, (_, v) => v + 1);
        stat.Paths.AddOrUpdate(path, 1, (_, v) => v + 1);
    }

    public IEnumerable<ServiceStats> GetStatistics() => _stats.Values;
}

public class ServiceStats(string serviceName)
{
    public string ServiceName { get; } = serviceName;
    public int TotalRequests { get; set; } = 0;
    public long TotalElapsedTime { get; set; } = 0; // milliseconds
    public DateTime LastCalled { get; set; } = DateTime.MinValue;

    public ConcurrentDictionary<string, int> MethodCounts { get; } = new();
    public ConcurrentDictionary<string, int> Users { get; } = new();
    public ConcurrentDictionary<string, int> Paths { get; } = new();

    public double AvgResponseTime => TotalRequests == 0 ? 0 : (double)TotalElapsedTime / TotalRequests;
}