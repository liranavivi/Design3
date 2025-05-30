using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Core.Interfaces.Services;
using EntitiesManager.Infrastructure.MassTransit.Events;
using EntitiesManager.Infrastructure.Repositories.Base;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EntitiesManager.Infrastructure.Repositories;

public class ProtocolEntityRepository : BaseRepository<ProtocolEntity>, IProtocolEntityRepository
{
    public ProtocolEntityRepository(IMongoDatabase database, ILogger<ProtocolEntityRepository> logger, IEventPublisher eventPublisher)
        : base(database, "protocols", logger, eventPublisher)
    {
    }

    protected override FilterDefinition<ProtocolEntity> CreateCompositeKeyFilter(string compositeKey)
    {
        var parts = compositeKey.Split('_', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Invalid composite key format for ProtocolEntity. Expected format: 'address_version'");

        return Builders<ProtocolEntity>.Filter.And(
            Builders<ProtocolEntity>.Filter.Eq(x => x.Address, parts[0]),
            Builders<ProtocolEntity>.Filter.Eq(x => x.Version, parts[1])
        );
    }

    protected override void CreateIndexes()
    {
        // Composite key index for uniqueness
        var compositeKeyIndex = Builders<ProtocolEntity>.IndexKeys
            .Ascending(x => x.Address)
            .Ascending(x => x.Version);

        var indexOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<ProtocolEntity>(compositeKeyIndex, indexOptions));

        // Additional indexes for common queries
        _collection.Indexes.CreateOne(new CreateIndexModel<ProtocolEntity>(
            Builders<ProtocolEntity>.IndexKeys.Ascending(x => x.Name)));
        _collection.Indexes.CreateOne(new CreateIndexModel<ProtocolEntity>(
            Builders<ProtocolEntity>.IndexKeys.Ascending(x => x.Address)));
        _collection.Indexes.CreateOne(new CreateIndexModel<ProtocolEntity>(
            Builders<ProtocolEntity>.IndexKeys.Ascending(x => x.Version)));
    }

    public async Task<IEnumerable<ProtocolEntity>> GetByAddressAsync(string address)
    {
        var filter = Builders<ProtocolEntity>.Filter.Eq(x => x.Address, address);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<ProtocolEntity>> GetByVersionAsync(string version)
    {
        var filter = Builders<ProtocolEntity>.Filter.Eq(x => x.Version, version);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<ProtocolEntity>> GetByNameAsync(string name)
    {
        var filter = Builders<ProtocolEntity>.Filter.Eq(x => x.Name, name);
        return await _collection.Find(filter).ToListAsync();
    }

    protected override async Task PublishCreatedEventAsync(ProtocolEntity entity)
    {
        var createdEvent = new ProtocolCreatedEvent
        {
            Id = entity.Id,
            Address = entity.Address,
            Version = entity.Version,
            Name = entity.Name,
            Description = entity.Description,
            Configuration = entity.Configuration,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
        await _eventPublisher.PublishAsync(createdEvent);
    }

    protected override async Task PublishUpdatedEventAsync(ProtocolEntity entity)
    {
        var updatedEvent = new ProtocolUpdatedEvent
        {
            Id = entity.Id,
            Address = entity.Address,
            Version = entity.Version,
            Name = entity.Name,
            Description = entity.Description,
            Configuration = entity.Configuration,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
        await _eventPublisher.PublishAsync(updatedEvent);
    }

    protected override async Task PublishDeletedEventAsync(Guid id, string deletedBy)
    {
        var deletedEvent = new ProtocolDeletedEvent
        {
            Id = id,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = deletedBy
        };
        await _eventPublisher.PublishAsync(deletedEvent);
    }
}
