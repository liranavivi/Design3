using EntitiesManager.Core.Exceptions;
using EntitiesManager.IntegrationTests.Builders;
using EntitiesManager.IntegrationTests.Infrastructure;
using Xunit;

namespace EntitiesManager.IntegrationTests.RepositoryTests;

public class SourceEntityRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAsync_ShouldCreateSourceEntity_WhenValidEntity()
    {
        // Arrange
        var entity = SourceEntityBuilder.Create()
            .WithRandomData()
            .WithCreatedBy("TestUser")
            .Build();

        // Act
        var created = await SourceRepository.CreateAsync(entity);

        // Assert
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(entity.Address, created.Address);
        Assert.Equal(entity.Version, created.Version);
        Assert.Equal(entity.Name, created.Name);
        Assert.Equal("TestUser", created.CreatedBy);
        Assert.True(created.CreatedAt > DateTime.MinValue);
        Assert.False(created.IsNew);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowDuplicateKeyException_WhenDuplicateCompositeKey()
    {
        // Arrange
        var entity1 = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source 1")
            .Build();

        var entity2 = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source 2")
            .Build();

        await SourceRepository.CreateAsync(entity1);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateKeyException>(() => SourceRepository.CreateAsync(entity2));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var entity = SourceEntityBuilder.Create()
            .WithRandomData()
            .Build();
        var created = await SourceRepository.CreateAsync(entity);

        // Act
        var retrieved = await SourceRepository.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(created.Address, retrieved.Address);
        Assert.Equal(created.Version, retrieved.Version);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Act
        var retrieved = await SourceRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetByCompositeKeyAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var entity = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source")
            .Build();
        await SourceRepository.CreateAsync(entity);

        // Act
        var retrieved = await SourceRepository.GetByCompositeKeyAsync("test-address_1.0.0");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-address", retrieved.Address);
        Assert.Equal("1.0.0", retrieved.Version);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity_WhenValidEntity()
    {
        // Arrange
        var entity = SourceEntityBuilder.Create()
            .WithRandomData()
            .WithCreatedBy("TestUser")
            .Build();
        var created = await SourceRepository.CreateAsync(entity);

        created.Name = "Updated Name";
        created.UpdatedBy = "UpdateUser";

        // Act
        var updated = await SourceRepository.UpdateAsync(created);

        // Assert
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("UpdateUser", updated.UpdatedBy);
        Assert.True(updated.UpdatedAt > updated.CreatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity_WhenEntityExists()
    {
        // Arrange
        var entity = SourceEntityBuilder.Create()
            .WithRandomData()
            .Build();
        var created = await SourceRepository.CreateAsync(entity);

        // Act
        var deleted = await SourceRepository.DeleteAsync(created.Id);

        // Assert
        Assert.True(deleted);
        var retrieved = await SourceRepository.GetByIdAsync(created.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // Act
        var deleted = await SourceRepository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await CleanupDatabaseAsync();
        
        var entities = new[]
        {
            SourceEntityBuilder.Create().WithRandomData().Build(),
            SourceEntityBuilder.Create().WithRandomData().Build(),
            SourceEntityBuilder.Create().WithRandomData().Build()
        };

        foreach (var entity in entities)
        {
            await SourceRepository.CreateAsync(entity);
        }

        // Act
        var retrieved = await SourceRepository.GetAllAsync();

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
            var entity = SourceEntityBuilder.Create().WithRandomData().Build();
            await SourceRepository.CreateAsync(entity);
        }

        // Act
        var page1 = await SourceRepository.GetPagedAsync(1, 2);
        var page2 = await SourceRepository.GetPagedAsync(2, 2);

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
            var entity = SourceEntityBuilder.Create().WithRandomData().Build();
            await SourceRepository.CreateAsync(entity);
        }

        // Act
        var count = await SourceRepository.CountAsync();

        // Assert
        Assert.Equal(3, count);
    }
}
