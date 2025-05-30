using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;

namespace EntitiesManager.Api.HealthChecks;

public class OpenTelemetryHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public OpenTelemetryHealthCheck(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = _configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
            
            // For OTLP gRPC endpoint, we'll check if the port is accessible
            // Since gRPC health checks are complex, we'll do a basic connectivity check
            var uri = new Uri(endpoint);
            var httpEndpoint = $"http://{uri.Host}:{uri.Port}";
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            // Try to connect to the OpenTelemetry collector
            var response = await _httpClient.GetAsync(httpEndpoint, cts.Token);
            
            // Even if we get a 404 or other HTTP error, it means the service is running
            // gRPC endpoints typically don't respond to HTTP GET requests
            return HealthCheckResult.Healthy($"OpenTelemetry collector is accessible at {endpoint}");
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Unhealthy("OpenTelemetry collector connection timeout");
        }
        catch (HttpRequestException ex)
        {
            // If we get a connection refused, the service is down
            if (ex.Message.Contains("Connection refused") || ex.Message.Contains("No connection could be made"))
            {
                return HealthCheckResult.Unhealthy($"OpenTelemetry collector is not accessible: {ex.Message}");
            }
            
            // Other HTTP errors might indicate the service is running but not responding to HTTP
            return HealthCheckResult.Healthy($"OpenTelemetry collector appears to be running (HTTP error: {ex.Message})");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"OpenTelemetry health check failed: {ex.Message}");
        }
    }
}
