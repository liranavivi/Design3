using EntitiesManager.Core.Entities;
using EntitiesManager.IntegrationTests.Builders;
using EntitiesManager.IntegrationTests.Infrastructure;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EntitiesManager.IntegrationTests;

public class ManualCrudTests : IntegrationTestBase
{
    [Fact]
    public async Task FullCrudWorkflow_Sources_ShouldWork()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source")
            .WithConfiguration(new Dictionary<string, object>
            {
                ["stringValue"] = "test-config",
                ["intValue"] = 42,
                ["boolValue"] = true
            })
            .WithCreatedBy("TestUser")
            .Build();

        // Act & Assert - CREATE
        var created = await SourceRepository.CreateAsync(source);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("test-address", created.Address);
        Assert.Equal("1.0.0", created.Version);
        Assert.Equal("Test Source", created.Name);
        Assert.Equal("TestUser", created.CreatedBy);
        Assert.True(created.CreatedAt > DateTime.MinValue);

        // Act & Assert - READ by ID
        var retrieved = await SourceRepository.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("test-address", retrieved.Address);
        Assert.Equal("1.0.0", retrieved.Version);

        // Act & Assert - READ by Composite Key
        var retrievedByKey = await SourceRepository.GetByCompositeKeyAsync("test-address_1.0.0");
        Assert.NotNull(retrievedByKey);
        Assert.Equal(created.Id, retrievedByKey.Id);

        // Act & Assert - UPDATE
        retrieved.Name = "Updated Test Source";
        retrieved.UpdatedBy = "UpdateUser";
        var updated = await SourceRepository.UpdateAsync(retrieved);
        Assert.Equal("Updated Test Source", updated.Name);
        Assert.Equal("UpdateUser", updated.UpdatedBy);
        Assert.True(updated.UpdatedAt > updated.CreatedAt);

        // Act & Assert - DELETE
        var deleted = await SourceRepository.DeleteAsync(created.Id);
        Assert.True(deleted);

        // Verify deletion
        var deletedEntity = await SourceRepository.GetByIdAsync(created.Id);
        Assert.Null(deletedEntity);
    }

    [Fact]
    public async Task FullCrudWorkflow_Destinations_ShouldWork()
    {
        // Arrange
        var destination = DestinationEntityBuilder.Create()
            .WithName("test-destination")
            .WithVersion("2.0.0")
            .WithInputSchema("{ \"type\": \"object\", \"properties\": { \"name\": { \"type\": \"string\" } } }")
            .WithCreatedBy("TestUser")
            .Build();

        // Act & Assert - CREATE
        var created = await DestinationRepository.CreateAsync(destination);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("test-destination", created.Name);
        Assert.Equal("2.0.0", created.Version);
        Assert.Equal("TestUser", created.CreatedBy);
        Assert.True(created.CreatedAt > DateTime.MinValue);

        // Act & Assert - READ by ID
        var retrieved = await DestinationRepository.GetByIdAsync(created.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("test-destination", retrieved.Name);
        Assert.Equal("2.0.0", retrieved.Version);

        // Act & Assert - READ by Composite Key
        var retrievedByKey = await DestinationRepository.GetByCompositeKeyAsync("test-destination_2.0.0");
        Assert.NotNull(retrievedByKey);
        Assert.Equal(created.Id, retrievedByKey.Id);

        // Act & Assert - UPDATE
        retrieved.InputSchema = "{ \"type\": \"object\", \"properties\": { \"updated\": { \"type\": \"boolean\" } } }";
        retrieved.UpdatedBy = "UpdateUser";
        var updated = await DestinationRepository.UpdateAsync(retrieved);
        Assert.Contains("updated", updated.InputSchema);
        Assert.Equal("UpdateUser", updated.UpdatedBy);
        Assert.True(updated.UpdatedAt > updated.CreatedAt);

        // Act & Assert - DELETE
        var deleted = await DestinationRepository.DeleteAsync(created.Id);
        Assert.True(deleted);

        // Verify deletion
        var deletedEntity = await DestinationRepository.GetByIdAsync(created.Id);
        Assert.Null(deletedEntity);
    }

    [Fact]
    public async Task BulkOperations_ShouldWork()
    {
        // Arrange - Create multiple entities
        var sources = new List<SourceEntity>();
        for (int i = 0; i < 5; i++)
        {
            var source = SourceEntityBuilder.Create()
                .WithAddress($"bulk-address-{i}")
                .WithVersion($"1.{i}.0")
                .WithName($"Bulk Source {i}")
                .WithConfiguration(new Dictionary<string, object>
                {
                    ["index"] = i,
                    ["isBulk"] = true
                })
                .Build();
            
            var created = await SourceRepository.CreateAsync(source);
            sources.Add(created);
        }

        // Act & Assert - Get All
        var allSources = await SourceRepository.GetAllAsync();
        Assert.True(allSources.Count() >= 5);

        // Act & Assert - Get Paged
        var pagedSources = await SourceRepository.GetPagedAsync(1, 3);
        Assert.Equal(3, pagedSources.Count());

        // Act & Assert - Count
        var count = await SourceRepository.CountAsync();
        Assert.True(count >= 5);

        // Cleanup
        foreach (var source in sources)
        {
            await SourceRepository.DeleteAsync(source.Id);
        }
    }

    [Fact]
    public async Task DuplicateCompositeKey_ShouldThrowException()
    {
        // Arrange
        var source1 = SourceEntityBuilder.Create()
            .WithAddress("duplicate-test")
            .WithVersion("1.0.0")
            .WithName("First Source")
            .Build();

        var source2 = SourceEntityBuilder.Create()
            .WithAddress("duplicate-test")
            .WithVersion("1.0.0")
            .WithName("Second Source")
            .Build();

        // Act & Assert
        await SourceRepository.CreateAsync(source1);
        
        await Assert.ThrowsAsync<EntitiesManager.Core.Exceptions.DuplicateKeyException>(
            () => SourceRepository.CreateAsync(source2));
    }
}
