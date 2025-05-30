using EntitiesManager.Core.Entities;
using EntitiesManager.IntegrationTests.Builders;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EntitiesManager.IntegrationTests.ApiTests;

public class SourcesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public SourcesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoSources()
    {
        // Act
        var response = await _client.GetAsync("/api/sources");

        // Assert
        response.EnsureSuccessStatusCode();
        var sources = await response.Content.ReadFromJsonAsync<List<SourceEntity>>(_jsonOptions);
        Assert.NotNull(sources);
    }

    [Fact]
    public async Task Create_ShouldCreateSource_WhenValidSource()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithRandomData()
            .AsNew()
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(source.Address, created.Address);
        Assert.Equal(source.Version, created.Version);
    }

    [Fact]
    public async Task Create_ShouldReturnConflict_WhenDuplicateCompositeKey()
    {
        // Arrange
        var source1 = SourceEntityBuilder.Create()
            .WithAddress("duplicate-address")
            .WithVersion("1.0.0")
            .WithName("Source 1")
            .AsNew()
            .Build();

        var source2 = SourceEntityBuilder.Create()
            .WithAddress("duplicate-address")
            .WithVersion("1.0.0")
            .WithName("Source 2")
            .AsNew()
            .Build();

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/sources", source1, _jsonOptions);
        var response2 = await _client.PostAsJsonAsync("/api/sources", source2, _jsonOptions);

        // Assert
        response1.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnSource_WhenSourceExists()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithRandomData()
            .AsNew()
            .Build();

        var createResponse = await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/sources/{created!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrieved = await response.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenSourceDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/sources/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCompositeKey_ShouldReturnSource_WhenSourceExists()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithAddress("test-address")
            .WithVersion("1.0.0")
            .WithName("Test Source")
            .AsNew()
            .Build();

        await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);

        // Act
        var response = await _client.GetAsync("/api/sources/by-key/test-address/1.0.0");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrieved = await response.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);
        Assert.NotNull(retrieved);
        Assert.Equal("test-address", retrieved.Address);
        Assert.Equal("1.0.0", retrieved.Version);
    }

    [Fact]
    public async Task Update_ShouldUpdateSource_WhenValidSource()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithRandomData()
            .AsNew()
            .Build();

        var createResponse = await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);

        created!.Name = "Updated Name";

        // Act
        var response = await _client.PutAsJsonAsync($"/api/sources/{created.Id}", created, _jsonOptions);

        // Assert
        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
    }

    [Fact]
    public async Task Delete_ShouldDeleteSource_WhenSourceExists()
    {
        // Arrange
        var source = SourceEntityBuilder.Create()
            .WithRandomData()
            .AsNew()
            .Build();

        var createResponse = await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<SourceEntity>(_jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/sources/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/sources/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetPaged_ShouldReturnPagedResults()
    {
        // Arrange - Create multiple sources
        for (int i = 0; i < 5; i++)
        {
            var source = SourceEntityBuilder.Create()
                .WithRandomData()
                .AsNew()
                .Build();
            await _client.PostAsJsonAsync("/api/sources", source, _jsonOptions);
        }

        // Act
        var response = await _client.GetAsync("/api/sources/paged?page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        
        Assert.True(result.TryGetProperty("data", out var dataProperty));
        Assert.True(result.TryGetProperty("page", out var pageProperty));
        Assert.True(result.TryGetProperty("pageSize", out var pageSizeProperty));
        
        Assert.Equal(1, pageProperty.GetInt32());
        Assert.Equal(2, pageSizeProperty.GetInt32());
    }
}
