using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MongoDB;
using EntitiesManager.Infrastructure.Repositories;
using EntitiesManager.IntegrationTests.Builders;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xunit;

namespace EntitiesManager.IntegrationTests;

public class BasicTests
{
    [Fact]
    public void CanCreateSourceEntity()
    {
        // Arrange
        var entity = new SourceEntity
        {
            Address = "test-address",
            Version = "1.0.0",
            Name = "Test Source",
            Configuration = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42
            }
        };

        // Act & Assert
        Assert.Equal("test-address_1.0.0", entity.GetCompositeKey());
        Assert.True(entity.IsNew);
        Assert.Equal("test-address", entity.Address);
        Assert.Equal("1.0.0", entity.Version);
        Assert.Equal("Test Source", entity.Name);
        Assert.Equal(2, entity.Configuration.Count);
    }

    [Fact]
    public void CanCreateDestinationEntity()
    {
        // Arrange
        var entity = new DestinationEntity
        {
            Name = "Test Destination",
            Version = "2.0.0",
            InputSchema = "{ \"type\": \"object\" }"
        };

        // Act & Assert
        Assert.Equal("Test Destination_2.0.0", entity.GetCompositeKey());
        Assert.True(entity.IsNew);
        Assert.Equal("Test Destination", entity.Name);
        Assert.Equal("2.0.0", entity.Version);
        Assert.Equal("{ \"type\": \"object\" }", entity.InputSchema);
    }

    [Fact]
    public void BsonConfigurationDoesNotThrow()
    {
        // Act & Assert - Should not throw
        BsonConfiguration.Configure();
    }

    [Fact]
    public void CanUseSourceEntityBuilder()
    {
        // Arrange & Act
        var entity = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source")
            .WithCreatedBy("TestUser")
            .Build();

        // Assert
        Assert.Equal("test-address", entity.Address);
        Assert.Equal("1.0.0", entity.Version);
        Assert.Equal("Test Source", entity.Name);
        Assert.Equal("TestUser", entity.CreatedBy);
        Assert.Equal("test-address_1.0.0", entity.GetCompositeKey());
    }

    [Fact]
    public void CanUseDestinationEntityBuilder()
    {
        // Arrange & Act
        var entity = DestinationEntityBuilder.Create()
            .WithName("Test Destination")
            .WithVersion("2.0.0")
            .WithInputSchema("{ \"type\": \"object\" }")
            .WithCreatedBy("TestUser")
            .Build();

        // Assert
        Assert.Equal("Test Destination", entity.Name);
        Assert.Equal("2.0.0", entity.Version);
        Assert.Equal("{ \"type\": \"object\" }", entity.InputSchema);
        Assert.Equal("TestUser", entity.CreatedBy);
        Assert.Equal("Test Destination_2.0.0", entity.GetCompositeKey());
    }

    [Fact]
    public void CanCreateRandomEntities()
    {
        // Arrange & Act
        var source = SourceEntityBuilder.Create().WithRandomData().Build();
        var destination = DestinationEntityBuilder.Create().WithRandomData().Build();

        // Assert
        Assert.NotNull(source.Address);
        Assert.NotNull(source.Version);
        Assert.NotNull(source.Name);
        Assert.NotEmpty(source.Configuration);

        Assert.NotNull(destination.Name);
        Assert.NotNull(destination.Version);
        Assert.NotNull(destination.InputSchema);
    }
}
