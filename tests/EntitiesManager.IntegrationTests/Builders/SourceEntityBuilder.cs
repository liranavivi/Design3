using EntitiesManager.Core.Entities;

namespace EntitiesManager.IntegrationTests.Builders;

public class SourceEntityBuilder
{
    private SourceEntity _entity = new();

    public static SourceEntityBuilder Create() => new();

    public SourceEntityBuilder WithId(Guid id)
    {
        _entity.Id = id;
        return this;
    }

    public SourceEntityBuilder AsNew()
    {
        _entity.Id = Guid.Empty; // MongoDB will generate
        return this;
    }

    public SourceEntityBuilder WithAddress(string address)
    {
        _entity.Address = address;
        return this;
    }

    public SourceEntityBuilder WithVersion(string version)
    {
        _entity.Version = version;
        return this;
    }

    public SourceEntityBuilder WithName(string name)
    {
        _entity.Name = name;
        return this;
    }

    public SourceEntityBuilder WithConfiguration(Dictionary<string, object> configuration)
    {
        _entity.Configuration = configuration;
        return this;
    }

    public SourceEntityBuilder WithRandomData()
    {
        var random = new Random();
        _entity.Address = $"address-{random.Next(1000, 9999)}";
        _entity.Version = $"v{random.Next(1, 10)}.{random.Next(0, 10)}.{random.Next(0, 10)}";
        _entity.Name = $"source-{random.Next(1000, 9999)}";
        _entity.Configuration = new Dictionary<string, object>
        {
            ["stringValue"] = "test-value",
            ["intValue"] = random.Next(1, 100),
            ["boolValue"] = true,
            ["doubleValue"] = random.NextDouble() * 100
        };
        return this;
    }

    public SourceEntityBuilder WithCreatedBy(string createdBy)
    {
        _entity.CreatedBy = createdBy;
        return this;
    }

    public SourceEntity Build() => _entity;
}
