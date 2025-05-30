using EntitiesManager.Core.Entities;

namespace EntitiesManager.IntegrationTests.Builders;

public class DestinationEntityBuilder
{
    private DestinationEntity _entity = new();

    public static DestinationEntityBuilder Create() => new();

    public DestinationEntityBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    public DestinationEntityBuilder AsNew()
    {
        _entity.Id = Guid.Empty; // MongoDB will generate
        return this;
    }

    public DestinationEntityBuilder WithName(string name)
    {
        _entity.Name = name;
        return this;
    }

    public DestinationEntityBuilder WithVersion(string version)
    {
        _entity.Version = version;
        return this;
    }

    public DestinationEntityBuilder WithInputSchema(string inputSchema)
    {
        _entity.InputSchema = inputSchema;
        return this;
    }

    public DestinationEntityBuilder WithRandomData()
    {
        var random = new Random();
        _entity.Name = $"destination-{random.Next(1000, 9999)}";
        _entity.Version = $"v{random.Next(1, 10)}.{random.Next(0, 10)}.{random.Next(0, 10)}";
        _entity.InputSchema = $"{{\"type\":\"object\",\"id\":{random.Next(1, 1000)}}}";
        return this;
    }

    public DestinationEntityBuilder WithCreatedBy(string createdBy)
    {
        _entity.CreatedBy = createdBy;
        return this;
    }

    public DestinationEntity Build() => _entity;
}
