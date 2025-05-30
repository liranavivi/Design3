using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Core.Interfaces.Services;
using EntitiesManager.Infrastructure.MongoDB;
using EntitiesManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace EntitiesManager.IntegrationTests.ProcessingChainTests;

public abstract class ProcessingChainIntegrationTestBase : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    protected IMongoDatabase Database { get; private set; } = null!;
    protected IProcessingChainEntityRepository ProcessingChainRepository { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    protected ProcessingChainIntegrationTestBase()
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
        Database = mongoClient.GetDatabase("ProcessingChainTestDb");

        // Setup service collection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton(Database);

        // Add mock event publisher for testing
        services.AddScoped<IEventPublisher, MockEventPublisher>();
        services.AddScoped<IProcessingChainEntityRepository, ProcessingChainEntityRepository>();

        ServiceProvider = services.BuildServiceProvider();
        ProcessingChainRepository = ServiceProvider.GetRequiredService<IProcessingChainEntityRepository>();
    }

    public async Task DisposeAsync()
    {
        (ServiceProvider as IDisposable)?.Dispose();
        await _mongoContainer.DisposeAsync();
    }

    protected ProcessingChainEntity CreateTestProcessingChain(string name = "TestProcessingChain", string version = "1.0.0", string address = "test://")
    {
        return new ProcessingChainEntity
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
