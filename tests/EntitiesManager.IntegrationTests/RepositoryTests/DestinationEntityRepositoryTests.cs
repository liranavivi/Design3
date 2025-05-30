using EntitiesManager.Core.Exceptions;
using EntitiesManager.IntegrationTests.Builders;
using EntitiesManager.IntegrationTests.Infrastructure;
using Xunit;

namespace EntitiesManager.IntegrationTests.RepositoryTests;

public class DestinationEntityRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ShouldCreateDestinationEntity_WhenValidEntity()
    {
        // Arrange
        var entity = DestinationEntityBuilder.Create()
            .WithRandomData()
            .WithCreatedBy("TestUser")
            .Build();

        // Act
        var created = await DestinationRepository.CreateAsync(entity);

        // Assert
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(entity.Name, created.Name);
        Assert.Equal(entity.Version, created.Version);
        Assert.Equal(entity.InputSchema, created.InputSchema);
        Assert.Equal("TestUser", created.CreatedBy);
        Assert.True(created.CreatedAt > DateTime.MinValue);
        Assert.False(created.IsNew);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowDuplicateKeyException_WhenDuplicateCompositeKey()
    {
        // Arrange
        var entity1 = DestinationEntityBuilder.Create()
            .WithName("test-destination")
            .WithVersion("1.0.0")
            .WithInputSchema("{ \"type\": \"object\" }")
            .Build();

        var entity2 = DestinationEntityBuilder.Create()
            .WithName("test-destination")
            .WithVersion("1.0.0")
            .WithInputSchema("{ \"type\": \"string\" }")
            .Build();

        await DestinationRepository.CreateAsync(entity1);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateKeyException>(() => DestinationRepository.CreateAsync(entity2));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var entity = DestinationEntityBuilder.Create()
            .WithRandomData()
            .Build();
        var created = await DestinationRepository.CreateAsync(entity);

        // Act
        var retrieved = await DestinationRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Name, retrieved.Name);
        Assert.Equal(created.Version, retrieved.Version);
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var entity = DestinationEntityBuilder.Create()
            .WithName("test-destination")
            .WithVersion("1.0.0")
            .WithInputSchema("{ \"type\": \"object\" }")
            .Build();
        await DestinationRepository.CreateAsync(entity);

        // Act
        var retrieved = await DestinationRepository.GetByCompositeKeyAsync("test-destination_1.0.0");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-destination", retrieved.Name);
        Assert.Equal("1.0.0", retrieved.Version);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity_WhenValidEntity()
    {
        // Arrange
        var entity = DestinationEntityBuilder.Create()
            .WithRandomData()
            .WithCreatedBy("TestUser")
            .Build();
        var created = await DestinationRepository.CreateAsync(entity);

        created.InputSchema = "{ \"type\": \"updated\" }";
        created.UpdatedBy = "UpdateUser";

        // Act
        var updated = await DestinationRepository.UpdateAsync(created);

        // Assert
        Assert.Equal("{ \"type\": \"updated\" }", updated.InputSchema);
        Assert.Equal("UpdateUser", updated.UpdatedBy);
        Assert.True(updated.UpdatedAt > updated.CreatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity_WhenEntityExists()
    {
        // Arrange
        var entity = DestinationEntityBuilder.Create()
            .WithRandomData()
            .Build();
        var created = await DestinationRepository.CreateAsync(entity);

        // Act
        var deleted = await DestinationRepository.DeleteAsync(created.Id);

        // Assert
        Assert.True(deleted);
        var retrieved = await DestinationRepository.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        var entities = new[]
        {
            DestinationEntityBuilder.Create().WithRandomData().Build(),
            DestinationEntityBuilder.Create().WithRandomData().Build(),
            DestinationEntityBuilder.Create().WithRandomData().Build()
        };

        foreach (var entity in entities)
        {
            await DestinationRepository.CreateAsync(entity);
        }

        // Act
        var retrieved = await DestinationRepository.GetAllAsync();

        // Assert
        Assert.Equal(3, retrieved.Count());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        for (int i = 0; i < 5; i++)
        {
            var entity = DestinationEntityBuilder.Create().WithRandomData().Build();
            await DestinationRepository.CreateAsync(entity);
        }

        // Act
        var page1 = await DestinationRepository.GetPagedAsync(1, 2);
        var page2 = await DestinationRepository.GetPagedAsync(2, 2);

        // Assert
        Assert.Equal(2, page1.Count());
        Assert.Equal(2, page2.Count());
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        for (int i = 0; i < 3; i++)
        {
            var entity = DestinationEntityBuilder.Create().WithRandomData().Build();
            await DestinationRepository.CreateAsync(entity);
        }

        // Act
        var count = await DestinationRepository.CountAsync();

        // Assert
        Assert.Equal(3, count);
    }
}
