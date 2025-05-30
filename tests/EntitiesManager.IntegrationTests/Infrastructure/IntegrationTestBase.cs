using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MongoDB;
using EntitiesManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace EntitiesManager.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    protected IMongoDatabase Database { get; private set; } = null!;
    protected ISourceEntityRepository SourceRepository { get; private set; } = null!;
    protected IDestinationEntityRepository DestinationRepository { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    protected IntegrationTestBase()
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
        Database = mongoClient.GetDatabase("EntitiesManagerTestDb");

        // Setup service collection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton(Database);
        services.AddScoped<ISourceEntityRepository, SourceEntityRepository>();
        services.AddScoped<IDestinationEntityRepository, DestinationEntityRepository>();

        ServiceProvider = services.BuildServiceProvider();

        // Get repositories
        SourceRepository = ServiceProvider.GetRequiredService<ISourceEntityRepository>();
        DestinationRepository = ServiceProvider.GetRequiredService<IDestinationEntityRepository>();

        // Ensure indexes are created
        await CreateIndexesAsync();
    }

    public async Task DisposeAsync()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
        await _mongoContainer.DisposeAsync();
    }

    private async Task CreateIndexesAsync()
    {
        // Create indexes for Source entities
        var sourceCollection = Database.GetCollection<EntitiesManager.Core.Entities.SourceEntity>("sources");
        var sourceIndexKeys = Builders<EntitiesManager.Core.Entities.SourceEntity>.IndexKeys
            .Ascending(x => x.Address)
            .Ascending(x => x.Version);
        var sourceIndexOptions = new CreateIndexOptions { Unique = true };
        await sourceCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<EntitiesManager.Core.Entities.SourceEntity>(sourceIndexKeys, sourceIndexOptions));

        // Create indexes for Destination entities
        var destinationCollection = Database.GetCollection<EntitiesManager.Core.Entities.DestinationEntity>("destinations");
        var destinationIndexKeys = Builders<EntitiesManager.Core.Entities.DestinationEntity>.IndexKeys
            .Ascending(x => x.Name)
            .Ascending(x => x.Version);
        var destinationIndexOptions = new CreateIndexOptions { Unique = true };
        await destinationCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<EntitiesManager.Core.Entities.DestinationEntity>(destinationIndexKeys, destinationIndexOptions));
    }

    protected async Task CleanupDatabaseAsync()
    {
        await Database.GetCollection<EntitiesManager.Core.Entities.SourceEntity>("sources").DeleteManyAsync(Builders<EntitiesManager.Core.Entities.SourceEntity>.Filter.Empty);
        await Database.GetCollection<EntitiesManager.Core.Entities.DestinationEntity>("destinations").DeleteManyAsync(Builders<EntitiesManager.Core.Entities.DestinationEntity>.Filter.Empty);
    }
}
