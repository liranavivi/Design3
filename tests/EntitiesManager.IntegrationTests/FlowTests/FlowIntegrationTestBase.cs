using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Core.Interfaces.Services;
using EntitiesManager.Infrastructure.MongoDB;
using EntitiesManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace EntitiesManager.IntegrationTests.FlowTests;

public abstract class FlowIntegrationTestBase : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    protected IMongoDatabase Database { get; private set; } = null!;
    protected IFlowEntityRepository FlowRepository { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    protected FlowIntegrationTestBase()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27017, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        // Configure BSON serialization
        BsonConfiguration.Configure();

        // Setup MongoDB client and database
        var connectionString = _mongoContainer.GetConnectionString();
        var mongoClient = new MongoClient(connectionString);
        Database = mongoClient.GetDatabase("FlowTestDb");

        // Setup service collection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton(Database);

        // Add mock event publisher for testing
        services.AddScoped<IEventPublisher, MockEventPublisher>();
        services.AddScoped<IFlowEntityRepository, FlowEntityRepository>();

        ServiceProvider = services.BuildServiceProvider();
        FlowRepository = ServiceProvider.GetRequiredService<IFlowEntityRepository>();
    }

    public async Task DisposeAsync()
    {
        (ServiceProvider as IDisposable)?.Dispose();
        await _mongoContainer.DisposeAsync();
    }

    protected FlowEntity CreateTestFlow(string name = "TestFlow", string version = "1.0.0", string address = "test://")
    {
        return new FlowEntity
        {
            Address = address,
            Version = version,
            Name = name,
            Configuration = new Dictionary<string, object>
            {
                ["testProperty"] = "testValue",
                ["numericProperty"] = 42,
                ["booleanProperty"] = true
            },
            CreatedBy = "TestUser"
        };
    }
}

// Mock event publisher for testing
public class MockEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T eventData) where T : class
    {
        // Mock implementation - just return completed task
        return Task.CompletedTask;
    }
}
