using EntitiesManager.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace EntitiesManager.IntegrationTests.ApiTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public CustomWebApplicationFactory()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27017, true)
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.12-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDB"] = _mongoContainer.GetConnectionString(),
                ["MongoDB:DatabaseName"] = "EntitiesManagerTestDb",
                ["RabbitMQ:Host"] = _rabbitMqContainer.Hostname,
                ["RabbitMQ:Port"] = _rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                ["RabbitMQ:Username"] = "guest",
                ["RabbitMQ:Password"] = "guest",
                ["RabbitMQ:VirtualHost"] = "/",
                ["OpenTelemetry:Endpoint"] = "http://localhost:4317"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Additional test-specific service configuration can go here
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        // Wait a bit for containers to be fully ready
        await Task.Delay(2000);
    }

    public new async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
