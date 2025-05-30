# EntitiesManager Web API - Complete Implementation Plan

## Table of Contents
1. [Project Overview](#project-overview)
2. [Core Architecture & Structure](#core-architecture--structure)
3. [Domain Models Design](#domain-models-design)
4. [Repository Pattern Implementation](#repository-pattern-implementation)
5. [API Controllers Implementation](#api-controllers-implementation)
6. [Infrastructure Integration](#infrastructure-integration)
7. [Message Bus Implementation](#message-bus-implementation)
8. [Testing Infrastructure](#testing-infrastructure)
9. [Production Readiness](#production-readiness)
10. [Implementation Phases](#implementation-phases)
11. [Success Criteria](#success-criteria)

## Project Overview

The EntitiesManager is a .NET Core 9 Web API project designed to manage SourceEntity and DestinationEntity objects with the following key requirements:

- **Database**: MongoDB with auto-generated GUID IDs using MongoDB.Driver directly
- **Message Bus**: MassTransit with RabbitMQ transport for CRUD operations
- **Observability**: OpenTelemetry collector for comprehensive logging, metrics, and tracing
- **Architecture**: Clean architecture with Repository pattern, no Entity Framework, no MediatR, no DTOs/AutoMapper
- **Testing**: Integration tests against real Docker containers
- **Validation**: Composite key uniqueness validation

## Core Architecture & Structure

### Complete Project Structure
```
FlowOrchestrator/
├── FlowOrchestrator.sln
├── src/
│   └── EntitiesManager/
│       ├── EntitiesManager.Api/
│       │   ├── Controllers/
│       │   │   ├── SourcesController.cs
│       │   │   ├── DestinationsController.cs
│       │   │   └── HealthController.cs
│       │   ├── Middleware/
│       │   │   ├── GlobalExceptionMiddleware.cs
│       │   │   └── RequestLoggingMiddleware.cs
│       │   ├── Configuration/
│       │   │   ├── MongoDbConfiguration.cs
│       │   │   ├── MassTransitConfiguration.cs
│       │   │   └── OpenTelemetryConfiguration.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   └── EntitiesManager.Api.csproj
│       ├── EntitiesManager.Core/
│       │   ├── Entities/
│       │   │   ├── Base/
│       │   │   │   └── BaseEntity.cs
│       │   │   ├── SourceEntity.cs
│       │   │   └── DestinationEntity.cs
│       │   ├── Interfaces/
│       │   │   └── Repositories/
│       │   │       ├── IBaseRepository.cs
│       │   │       ├── ISourceEntityRepository.cs
│       │   │       └── IDestinationEntityRepository.cs
│       │   ├── Exceptions/
│       │   │   ├── DuplicateKeyException.cs
│       │   │   ├── EntityNotFoundException.cs
│       │   │   └── ValidationException.cs
│       │   └── EntitiesManager.Core.csproj
│       ├── EntitiesManager.Infrastructure/
│       │   ├── Repositories/
│       │   │   ├── BaseRepository.cs
│       │   │   ├── SourceEntityRepository.cs
│       │   │   └── DestinationEntityRepository.cs
│       │   ├── MongoDB/
│       │   │   ├── GuidGenerator.cs
│       │   │   ├── MongoDbContext.cs
│       │   │   └── BsonConfiguration.cs
│       │   ├── MassTransit/
│       │   │   ├── Consumers/
│       │   │   │   ├── CreateSourceCommandConsumer.cs
│       │   │   │   ├── UpdateSourceCommandConsumer.cs
│       │   │   │   ├── DeleteSourceCommandConsumer.cs
│       │   │   │   ├── GetSourceQueryConsumer.cs
│       │   │   │   ├── CreateDestinationCommandConsumer.cs
│       │   │   │   ├── UpdateDestinationCommandConsumer.cs
│       │   │   │   ├── DeleteDestinationCommandConsumer.cs
│       │   │   │   └── GetDestinationQueryConsumer.cs
│       │   │   ├── Commands/
│       │   │   │   ├── SourceCommands.cs
│       │   │   │   └── DestinationCommands.cs
│       │   │   └── Events/
│       │   │       ├── SourceEvents.cs
│       │   │       └── DestinationEvents.cs
│       │   └── EntitiesManager.Infrastructure.csproj
│       └── EntitiesManager.Application/
│           ├── Services/
│           │   ├── ISourceEntityService.cs
│           │   ├── SourceEntityService.cs
│           │   ├── IDestinationEntityService.cs
│           │   └── DestinationEntityService.cs
│           ├── Validators/
│           │   ├── SourceEntityValidator.cs
│           │   └── DestinationEntityValidator.cs
│           └── EntitiesManager.Application.csproj
├── tests/
│   ├── EntitiesManager.IntegrationTests/
│   │   ├── Controllers/
│   │   │   ├── SourcesControllerTests.cs
│   │   │   └── DestinationsControllerTests.cs
│   │   ├── Repositories/
│   │   │   ├── SourceEntityRepositoryTests.cs
│   │   │   └── DestinationEntityRepositoryTests.cs
│   │   ├── MassTransit/
│   │   │   ├── SourceCommandConsumerTests.cs
│   │   │   └── DestinationCommandConsumerTests.cs
│   │   ├── Infrastructure/
│   │   │   ├── MongoDbTestFixture.cs
│   │   │   ├── MassTransitTestFixture.cs
│   │   │   └── WebApplicationTestFixture.cs
│   │   ├── Builders/
│   │   │   ├── SourceEntityBuilder.cs
│   │   │   └── DestinationEntityBuilder.cs
│   │   └── EntitiesManager.IntegrationTests.csproj
│   └── EntitiesManager.UnitTests/
│       ├── Validators/
│       │   ├── SourceEntityValidatorTests.cs
│       │   └── DestinationEntityValidatorTests.cs
│       └── EntitiesManager.UnitTests.csproj
├── docker/
│   ├── docker-compose.yml
│   ├── docker-compose.test.yml
│   └── otel-collector-config.yaml
└── README.md
```

### Project Dependencies

#### EntitiesManager.Api.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MongoDB.Driver" Version="2.22.0" />
    <PackageReference Include="MassTransit" Version="8.1.1" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.1.1" />
    <PackageReference Include="OpenTelemetry" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.6.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.5.1-beta.1" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.5.1-beta.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
    <PackageReference Include="Serilog.Sinks.OpenTelemetry" Version="1.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="AspNetCore.HealthChecks.MongoDb" Version="7.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.RabbitMQ" Version="7.0.0" />
    <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="7.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EntitiesManager.Core\EntitiesManager.Core.csproj" />
    <ProjectReference Include="..\EntitiesManager.Infrastructure\EntitiesManager.Infrastructure.csproj" />
    <ProjectReference Include="..\EntitiesManager.Application\EntitiesManager.Application.csproj" />
  </ItemGroup>
</Project>
```

#### EntitiesManager.Core.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MongoDB.Bson" Version="2.22.0" />
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>
</Project>
```

#### EntitiesManager.Infrastructure.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MongoDB.Driver" Version="2.22.0" />
    <PackageReference Include="MassTransit" Version="8.1.1" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.1.1" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="7.0.1" />
    <PackageReference Include="System.Diagnostics.DiagnosticSource" Version="7.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EntitiesManager.Core\EntitiesManager.Core.csproj" />
  </ItemGroup>
</Project>
```

#### EntitiesManager.Application.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.7.1" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="7.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\EntitiesManager.Core\EntitiesManager.Core.csproj" />
  </ItemGroup>
</Project>
```

## Domain Models Design

### Base Entity with MongoDB Auto-Generated GUID
```csharp
// EntitiesManager.Core/Entities/Base/BaseEntity.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Core.Entities.Base;

public abstract class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } // MongoDB will auto-generate

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("updatedBy")]
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Abstract method for composite key validation
    /// </summary>
    public abstract string GetCompositeKey();

    /// <summary>
    /// Helper method to check if entity is new (no ID assigned yet)
    /// </summary>
    [BsonIgnore]
    public bool IsNew => Id == Guid.Empty;
}
```

### SourceEntity Implementation
```csharp
// EntitiesManager.Core/Entities/SourceEntity.cs
using EntitiesManager.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Core.Entities;

public class SourceEntity : BaseEntity
{
    [BsonElement("address")]
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("version")]
    [Required(ErrorMessage = "Version is required")]
    [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
    public string Version { get; set; } = string.Empty;

    [BsonElement("name")]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("configuration")]
    public Dictionary<string, object> Configuration { get; set; } = new();

    public override string GetCompositeKey() => $"{Address}_{Version}";
}
```

### DestinationEntity Implementation
```csharp
// EntitiesManager.Core/Entities/DestinationEntity.cs
using EntitiesManager.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Core.Entities;

public class DestinationEntity : BaseEntity
{
    [BsonElement("version")]
    [Required(ErrorMessage = "Version is required")]
    [StringLength(50, ErrorMessage = "Version cannot exceed 50 characters")]
    public string Version { get; set; } = string.Empty;

    [BsonElement("name")]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("inputSchema")]
    [Required(ErrorMessage = "Input schema is required")]
    public string InputSchema { get; set; } = string.Empty;

    public override string GetCompositeKey() => $"{Name}_{Version}";
}
```

### Custom Exception Types
```csharp
// EntitiesManager.Core/Exceptions/DuplicateKeyException.cs
namespace EntitiesManager.Core.Exceptions;

public class DuplicateKeyException : Exception
{
    public DuplicateKeyException(string message) : base(message) { }
    public DuplicateKeyException(string message, Exception innerException) : base(message, innerException) { }
}

// EntitiesManager.Core/Exceptions/EntityNotFoundException.cs
namespace EntitiesManager.Core.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

// EntitiesManager.Core/Exceptions/ValidationException.cs
namespace EntitiesManager.Core.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}
```

## Repository Pattern Implementation

### Base Repository Interface
```csharp
// EntitiesManager.Core/Interfaces/Repositories/IBaseRepository.cs
using EntitiesManager.Core.Entities.Base;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByCompositeKeyAsync(string compositeKey);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string compositeKey);
    Task<bool> ExistsByIdAsync(Guid id);
    Task<long> CountAsync();
}
```

### Specific Repository Interfaces
```csharp
// EntitiesManager.Core/Interfaces/Repositories/ISourceEntityRepository.cs
using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface ISourceEntityRepository : IBaseRepository<SourceEntity>
{
    Task<IEnumerable<SourceEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<SourceEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<SourceEntity>> GetByNameAsync(string name);
}

// EntitiesManager.Core/Interfaces/Repositories/IDestinationEntityRepository.cs
using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IDestinationEntityRepository : IBaseRepository<DestinationEntity>
{
    Task<IEnumerable<DestinationEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<DestinationEntity>> GetByNameAsync(string name);
}
```

### MongoDB GUID Generator
```csharp
// EntitiesManager.Infrastructure/MongoDB/GuidGenerator.cs
using MongoDB.Bson.Serialization;

namespace EntitiesManager.Infrastructure.MongoDB;

public class GuidGenerator : IIdGenerator
{
    public object GenerateId(object container, object document)
    {
        return Guid.NewGuid();
    }

    public bool IsEmpty(object id)
    {
        return id == null || (Guid)id == Guid.Empty;
    }
}
```

### Base Repository Implementation
```csharp
// EntitiesManager.Infrastructure/Repositories/BaseRepository.cs
using EntitiesManager.Core.Entities.Base;
using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Diagnostics;

namespace EntitiesManager.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly ILogger<BaseRepository<T>> _logger;
    private static readonly ActivitySource ActivitySource = new($"EntitiesManager.Repository.{typeof(T).Name}");

    protected BaseRepository(IMongoDatabase database, string collectionName, ILogger<BaseRepository<T>> logger)
    {
        _collection = database.GetCollection<T>(collectionName);
        _logger = logger;
        CreateIndexes();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        using var activity = ActivitySource.StartActivity($"GetById{typeof(T).Name}");
        activity?.SetTag("entity.id", id.ToString());
        activity?.SetTag("entity.type", typeof(T).Name);

        try
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();

            activity?.SetTag("result.found", result != null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID {Id}", typeof(T).Name, id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<T?> GetByCompositeKeyAsync(string compositeKey)
    {
        using var activity = ActivitySource.StartActivity($"GetByCompositeKey{typeof(T).Name}");
        activity?.SetTag("entity.compositeKey", compositeKey);
        activity?.SetTag("entity.type", typeof(T).Name);

        try
        {
            var filter = CreateCompositeKeyFilter(compositeKey);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();

            activity?.SetTag("result.found", result != null);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by composite key {CompositeKey}", typeof(T).Name, compositeKey);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var activity = ActivitySource.StartActivity($"GetAll{typeof(T).Name}");
        activity?.SetTag("entity.type", typeof(T).Name);

        try
        {
            var result = await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();

            activity?.SetTag("result.count", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType}", typeof(T).Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
    {
        using var activity = ActivitySource.StartActivity($"GetPaged{typeof(T).Name}");
        activity?.SetTag("entity.type", typeof(T).Name);
        activity?.SetTag("page", page);
        activity?.SetTag("pageSize", pageSize);

        try
        {
            var skip = (page - 1) * pageSize;
            var result = await _collection
                .Find(Builders<T>.Filter.Empty)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            activity?.SetTag("result.count", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType} (page: {Page}, size: {PageSize})", typeof(T).Name, page, pageSize);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        using var activity = ActivitySource.StartActivity($"Create{typeof(T).Name}");
        activity?.SetTag("entity.type", typeof(T).Name);
        activity?.SetTag("entity.compositeKey", entity.GetCompositeKey());

        try
        {
            // Set timestamps - MongoDB will auto-generate the ID
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Validate composite key uniqueness BEFORE insertion
            if (await ExistsAsync(entity.GetCompositeKey()))
            {
                throw new DuplicateKeyException($"{typeof(T).Name} with composite key '{entity.GetCompositeKey()}' already exists");
            }

            // MongoDB will auto-generate the GUID ID during insertion
            await _collection.InsertOneAsync(entity);

            activity?.SetTag("entity.id", entity.Id.ToString());
            _logger.LogInformation("Created {EntityType} with auto-generated ID {Id} and composite key {CompositeKey}",
                typeof(T).Name, entity.Id, entity.GetCompositeKey());

            return entity;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning("Duplicate key error creating {EntityType}: {Error}", typeof(T).Name, ex.WriteError.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.WriteError.Message);
            throw new DuplicateKeyException($"Duplicate key error: {ex.WriteError.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(T).Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        using var activity = ActivitySource.StartActivity($"Update{typeof(T).Name}");
        activity?.SetTag("entity.type", typeof(T).Name);
        activity?.SetTag("entity.id", entity.Id.ToString());
        activity?.SetTag("entity.compositeKey", entity.GetCompositeKey());

        try
        {
            // Validate that entity has an ID (not new)
            if (entity.IsNew)
            {
                throw new InvalidOperationException($"Cannot update {typeof(T).Name} with empty ID. Use CreateAsync for new entities.");
            }

            entity.UpdatedAt = DateTime.UtcNow;

            // Check if we're changing the composite key and if the new key already exists
            var existing = await GetByIdAsync(entity.Id);
            if (existing == null)
            {
                throw new EntityNotFoundException($"{typeof(T).Name} with ID {entity.Id} not found");
            }

            // If composite key is changing, validate uniqueness
            if (existing.GetCompositeKey() != entity.GetCompositeKey())
            {
                if (await ExistsAsync(entity.GetCompositeKey()))
                {
                    throw new DuplicateKeyException($"{typeof(T).Name} with composite key '{entity.GetCompositeKey()}' already exists");
                }
            }

            var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
            var result = await _collection.ReplaceOneAsync(filter, entity);

            if (result.MatchedCount == 0)
            {
                throw new EntityNotFoundException($"{typeof(T).Name} with ID {entity.Id} not found");
            }

            _logger.LogInformation("Updated {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        using var activity = ActivitySource.StartActivity($"Delete{typeof(T).Name}");
        activity?.SetTag("entity.type", typeof(T).Name);
        activity?.SetTag("entity.id", id.ToString());

        try
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var result = await _collection.DeleteOneAsync(filter);

            var deleted = result.DeletedCount > 0;
            activity?.SetTag("result.deleted", deleted);
            _logger.LogInformation("Deleted {EntityType} with ID {Id}: {Success}", typeof(T).Name, id, deleted);

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(T).Name, id);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(string compositeKey)
    {
        try
        {
            var filter = CreateCompositeKeyFilter(compositeKey);
            var count = await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType} with composite key {CompositeKey}", typeof(T).Name, compositeKey);
            throw;
        }
    }

    public virtual async Task<bool> ExistsByIdAsync(Guid id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            var count = await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 });
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType} with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<long> CountAsync()
    {
        try
        {
            return await _collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType}", typeof(T).Name);
            throw;
        }
    }

    protected abstract FilterDefinition<T> CreateCompositeKeyFilter(string compositeKey);
    protected abstract void CreateIndexes();
}
```

### Specific Repository Implementations
```csharp
// EntitiesManager.Infrastructure/Repositories/SourceEntityRepository.cs
using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EntitiesManager.Infrastructure.Repositories;

public class SourceEntityRepository : BaseRepository<SourceEntity>, ISourceEntityRepository
{
    public SourceEntityRepository(IMongoDatabase database, ILogger<SourceEntityRepository> logger)
        : base(database, "sources", logger)
    {
    }

    protected override FilterDefinition<SourceEntity> CreateCompositeKeyFilter(string compositeKey)
    {
        var parts = compositeKey.Split('_', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Invalid composite key format for SourceEntity. Expected format: 'address_version'");

        return Builders<SourceEntity>.Filter.And(
            Builders<SourceEntity>.Filter.Eq(x => x.Address, parts[0]),
            Builders<SourceEntity>.Filter.Eq(x => x.Version, parts[1])
        );
    }

    protected override void CreateIndexes()
    {
        // Composite key index for uniqueness
        var compositeKeyIndex = Builders<SourceEntity>.IndexKeys
            .Ascending(x => x.Address)
            .Ascending(x => x.Version);

        var indexOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<SourceEntity>(compositeKeyIndex, indexOptions));

        // Additional indexes for common queries
        _collection.Indexes.CreateOne(new CreateIndexModel<SourceEntity>(
            Builders<SourceEntity>.IndexKeys.Ascending(x => x.Name)));
        _collection.Indexes.CreateOne(new CreateIndexModel<SourceEntity>(
            Builders<SourceEntity>.IndexKeys.Ascending(x => x.Address)));
        _collection.Indexes.CreateOne(new CreateIndexModel<SourceEntity>(
            Builders<SourceEntity>.IndexKeys.Ascending(x => x.Version)));
    }

    public async Task<IEnumerable<SourceEntity>> GetByAddressAsync(string address)
    {
        var filter = Builders<SourceEntity>.Filter.Eq(x => x.Address, address);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<SourceEntity>> GetByVersionAsync(string version)
    {
        var filter = Builders<SourceEntity>.Filter.Eq(x => x.Version, version);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<SourceEntity>> GetByNameAsync(string name)
    {
        var filter = Builders<SourceEntity>.Filter.Eq(x => x.Name, name);
        return await _collection.Find(filter).ToListAsync();
    }
}

// EntitiesManager.Infrastructure/Repositories/DestinationEntityRepository.cs
using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EntitiesManager.Infrastructure.Repositories;

public class DestinationEntityRepository : BaseRepository<DestinationEntity>, IDestinationEntityRepository
{
    public DestinationEntityRepository(IMongoDatabase database, ILogger<DestinationEntityRepository> logger)
        : base(database, "destinations", logger)
    {
    }

    protected override FilterDefinition<DestinationEntity> CreateCompositeKeyFilter(string compositeKey)
    {
        var parts = compositeKey.Split('_', 2);
        if (parts.Length != 2)
            throw new ArgumentException("Invalid composite key format for DestinationEntity. Expected format: 'name_version'");

        return Builders<DestinationEntity>.Filter.And(
            Builders<DestinationEntity>.Filter.Eq(x => x.Name, parts[0]),
            Builders<DestinationEntity>.Filter.Eq(x => x.Version, parts[1])
        );
    }

    protected override void CreateIndexes()
    {
        // Composite key index for uniqueness
        var compositeKeyIndex = Builders<DestinationEntity>.IndexKeys
            .Ascending(x => x.Name)
            .Ascending(x => x.Version);

        var indexOptions = new CreateIndexOptions { Unique = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<DestinationEntity>(compositeKeyIndex, indexOptions));

        // Additional indexes for common queries
        _collection.Indexes.CreateOne(new CreateIndexModel<DestinationEntity>(
            Builders<DestinationEntity>.IndexKeys.Ascending(x => x.Name)));
        _collection.Indexes.CreateOne(new CreateIndexModel<DestinationEntity>(
            Builders<DestinationEntity>.IndexKeys.Ascending(x => x.Version)));
    }

    public async Task<IEnumerable<DestinationEntity>> GetByVersionAsync(string version)
    {
        var filter = Builders<DestinationEntity>.Filter.Eq(x => x.Version, version);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<DestinationEntity>> GetByNameAsync(string name)
    {
        var filter = Builders<DestinationEntity>.Filter.Eq(x => x.Name, name);
        return await _collection.Find(filter).ToListAsync();
    }
}
```

## API Controllers Implementation

### Sources Controller (Direct Entity Usage)
```csharp
// EntitiesManager.Api/Controllers/SourcesController.cs
using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly ISourceEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(
        ISourceEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<SourcesController> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SourceEntity>>> GetAll()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all source entities");
            return StatusCode(500, "An error occurred while retrieving source entities");
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var entities = await _repository.GetPagedAsync(page, pageSize);
            var totalCount = await _repository.CountAsync();

            return Ok(new
            {
                Data = entities,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged source entities");
            return StatusCode(500, "An error occurred while retrieving source entities");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SourceEntity>> GetById(Guid id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return NotFound($"Source with ID {id} not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving source entity by ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the source entity");
        }
    }

    [HttpGet("by-key/{address}/{version}")]
    public async Task<ActionResult<SourceEntity>> GetByCompositeKey(string address, string version)
    {
        try
        {
            var compositeKey = $"{address}_{version}";
            var entity = await _repository.GetByCompositeKeyAsync(compositeKey);

            if (entity == null)
                return NotFound($"Source with address '{address}' and version '{version}' not found");

            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving source entity by composite key {Address}_{Version}", address, version);
            return StatusCode(500, "An error occurred while retrieving the source entity");
        }
    }

    [HttpPost]
    public async Task<ActionResult<SourceEntity>> Create([FromBody] SourceEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            entity.CreatedBy = User.Identity?.Name ?? "System";
            entity.Id = Guid.Empty; // Ensure MongoDB generates the ID

            var created = await _repository.CreateAsync(entity);

            if (created.Id == Guid.Empty)
            {
                _logger.LogError("MongoDB failed to generate ID for new SourceEntity");
                return StatusCode(500, "Failed to generate entity ID");
            }

            // Publish event via MassTransit
            await _publishEndpoint.Publish(new SourceCreatedEvent
            {
                Id = created.Id,
                Address = created.Address,
                Version = created.Version,
                Name = created.Name,
                Configuration = created.Configuration,
                CreatedAt = created.CreatedAt,
                CreatedBy = created.CreatedBy
            });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DuplicateKeyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating source entity");
            return StatusCode(500, "An error occurred while creating the source");
        }
    }
}
```

## Infrastructure Integration

### MongoDB Configuration
```csharp
// EntitiesManager.Infrastructure/MongoDB/BsonConfiguration.cs
using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Entities.Base;
using EntitiesManager.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace EntitiesManager.Infrastructure.MongoDB;

public static class BsonConfiguration
{
    public static void Configure()
    {
        // Register custom GUID generator for auto-generation
        BsonSerializer.RegisterIdGenerator(typeof(Guid), new GuidGenerator());

        // Configure GUID serialization as string
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        // Register class maps for entities to ensure proper ID generation
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.Id)
                  .SetIdGenerator(new GuidGenerator())
                  .SetSerializer(new GuidSerializer(BsonType.String));
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(SourceEntity)))
        {
            BsonClassMap.RegisterClassMap<SourceEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(DestinationEntity)))
        {
            BsonClassMap.RegisterClassMap<DestinationEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}

// EntitiesManager.Api/Configuration/MongoDbConfiguration.cs
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MongoDB;
using EntitiesManager.Infrastructure.Repositories;
using MongoDB.Driver;

namespace EntitiesManager.Api.Configuration;

public static class MongoDbConfiguration
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure BSON serialization
        BsonConfiguration.Configure();

        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            // Configure GUID representation
            settings.GuidRepresentation = GuidRepresentation.Standard;

            return new MongoClient(settings);
        });

        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName") ?? "EntitiesManagerDb";
            return client.GetDatabase(databaseName);
        });

        // Register repositories
        services.AddScoped<ISourceEntityRepository, SourceEntityRepository>();
        services.AddScoped<IDestinationEntityRepository, DestinationEntityRepository>();

        return services;
    }
}
```

### MassTransit Configuration
```csharp
// EntitiesManager.Infrastructure/MassTransit/Commands/SourceCommands.cs
namespace EntitiesManager.Infrastructure.MassTransit.Commands;

public class CreateSourceCommand
{
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

public class UpdateSourceCommand
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

public class DeleteSourceCommand
{
    public Guid Id { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
}

public class GetSourceQuery
{
    public Guid? Id { get; set; }
    public string? CompositeKey { get; set; }
}

// EntitiesManager.Infrastructure/MassTransit/Events/SourceEvents.cs
namespace EntitiesManager.Infrastructure.MassTransit.Events;

public class SourceCreatedEvent
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class SourceUpdatedEvent
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class SourceDeletedEvent
{
    public Guid Id { get; set; }
    public DateTime DeletedAt { get; set; }
    public string DeletedBy { get; set; } = string.Empty;
}

// EntitiesManager.Api/Configuration/MassTransitConfiguration.cs
using EntitiesManager.Infrastructure.MassTransit.Consumers;
using MassTransit;

namespace EntitiesManager.Api.Configuration;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Add consumers
            x.AddConsumer<CreateSourceCommandConsumer>();
            x.AddConsumer<UpdateSourceCommandConsumer>();
            x.AddConsumer<DeleteSourceCommandConsumer>();
            x.AddConsumer<GetSourceQueryConsumer>();
            x.AddConsumer<CreateDestinationCommandConsumer>();
            x.AddConsumer<UpdateDestinationCommandConsumer>();
            x.AddConsumer<DeleteDestinationCommandConsumer>();
            x.AddConsumer<GetDestinationQueryConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMQ");

                cfg.Host(rabbitMqSettings["Host"] ?? "localhost", rabbitMqSettings["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitMqSettings["Username"] ?? "guest");
                    h.Password(rabbitMqSettings["Password"] ?? "guest");
                });

                // Configure retry policy
                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(30)
                ));

                // Configure error handling
                cfg.UseInMemoryOutbox();

                // Configure endpoints
                cfg.ReceiveEndpoint("source-commands", e =>
                {
                    e.ConfigureConsumer<CreateSourceCommandConsumer>(context);
                    e.ConfigureConsumer<UpdateSourceCommandConsumer>(context);
                    e.ConfigureConsumer<DeleteSourceCommandConsumer>(context);
                    e.ConfigureConsumer<GetSourceQueryConsumer>(context);
                });

                cfg.ReceiveEndpoint("destination-commands", e =>
                {
                    e.ConfigureConsumer<CreateDestinationCommandConsumer>(context);
                    e.ConfigureConsumer<UpdateDestinationCommandConsumer>(context);
                    e.ConfigureConsumer<DeleteDestinationCommandConsumer>(context);
                    e.ConfigureConsumer<GetDestinationQueryConsumer>(context);
                });

                // Configure OpenTelemetry integration
                cfg.ConfigureOpenTelemetryTracing();
            });
        });

        return services;
    }
}
```

### OpenTelemetry Configuration
```csharp
// EntitiesManager.Api/Configuration/OpenTelemetryConfiguration.cs
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace EntitiesManager.Api.Configuration;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = "EntitiesManager";
        var serviceVersion = "1.0.0";

        // Configure resource
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        // Add OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            // Filter out health check requests
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource("EntitiesManager.*")
                    .AddSource("MassTransit")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                    });
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter("EntitiesManager.*")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                    });
            });

        return services;
    }
}
```

### Configuration Files
```json
// EntitiesManager.Api/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "MassTransit": "Information",
      "MongoDB": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "EntitiesManagerDb"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317"
  }
}

// EntitiesManager.Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information",
      "MassTransit": "Debug",
      "MongoDB": "Debug"
    }
  },
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "EntitiesManagerDb_Dev"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317"
  }
}
```

## Testing Infrastructure

### Docker Compose Setup
```yaml
# docker/docker-compose.yml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    container_name: entitiesmanager-mongodb
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_DATABASE: EntitiesManagerDb
    volumes:
      - mongodb_data:/data/db
    networks:
      - entitiesmanager-network

  rabbitmq:
    image: rabbitmq:3.12-management
    container_name: entitiesmanager-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - entitiesmanager-network

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    container_name: entitiesmanager-otel-collector
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"   # OTLP gRPC receiver
      - "4318:4318"   # OTLP HTTP receiver
      - "8888:8888"   # Prometheus metrics
      - "8889:8889"   # Prometheus exporter metrics
    depends_on:
      - jaeger
    networks:
      - entitiesmanager-network

  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: entitiesmanager-jaeger
    ports:
      - "16686:16686"
      - "14250:14250"
    environment:
      COLLECTOR_OTLP_ENABLED: true
    networks:
      - entitiesmanager-network

volumes:
  mongodb_data:
  rabbitmq_data:

networks:
  entitiesmanager-network:
    driver: bridge

# docker/docker-compose.test.yml
version: '3.8'

services:
  mongodb-test:
    image: mongo:7.0
    container_name: entitiesmanager-mongodb-test
    ports:
      - "27018:27017"
    environment:
      MONGO_INITDB_DATABASE: EntitiesManagerDb_Test
    tmpfs:
      - /data/db
    networks:
      - entitiesmanager-test-network

  rabbitmq-test:
    image: rabbitmq:3.12-management
    container_name: entitiesmanager-rabbitmq-test
    ports:
      - "5673:5672"
      - "15673:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
    tmpfs:
      - /var/lib/rabbitmq
    networks:
      - entitiesmanager-test-network

  otel-collector-test:
    image: otel/opentelemetry-collector-contrib:latest
    container_name: entitiesmanager-otel-collector-test
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4319:4317"   # OTLP gRPC receiver
      - "4320:4318"   # OTLP HTTP receiver
    networks:
      - entitiesmanager-test-network

networks:
  entitiesmanager-test-network:
    driver: bridge
```

### OpenTelemetry Collector Configuration
```yaml
# docker/otel-collector-config.yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 1s
    send_batch_size: 1024
  memory_limiter:
    limit_mib: 512

exporters:
  jaeger:
    endpoint: jaeger:14250
    tls:
      insecure: true

  prometheus:
    endpoint: "0.0.0.0:8889"

  logging:
    loglevel: debug

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [jaeger, logging]

    metrics:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [prometheus, logging]

    logs:
      receivers: [otlp]
      processors: [memory_limiter, batch]
      exporters: [logging]
```

### Test Fixtures and Utilities
```csharp
// tests/EntitiesManager.IntegrationTests/Infrastructure/MongoDbTestFixture.cs
using EntitiesManager.Api.Configuration;
using EntitiesManager.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace EntitiesManager.IntegrationTests.Infrastructure;

public class MongoDbTestFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private IServiceProvider _serviceProvider = null!;

    public MongoDbTestFixture()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithPortBinding(27018, 27017)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MongoDB"] = _mongoContainer.GetConnectionString(),
                ["MongoDB:DatabaseName"] = "EntitiesManagerDb_Test"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());
        services.AddMongoDb(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }

    public T GetService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    public async Task CleanupDatabaseAsync()
    {
        var database = GetService<IMongoDatabase>();
        await database.DropCollectionAsync("sources");
        await database.DropCollectionAsync("destinations");
    }
}

// tests/EntitiesManager.IntegrationTests/Builders/SourceEntityBuilder.cs
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
            ["key1"] = "value1",
            ["key2"] = random.Next(1, 100)
        };
        return this;
    }

    public SourceEntity Build() => _entity;
}
```

## Implementation Phases

### Phase 1: Core Infrastructure Setup (Week 1)
1. **Project Structure Creation**
   - Create solution and project structure
   - Set up project references and dependencies
   - Configure NuGet packages

2. **Domain Models Implementation**
   - Implement BaseEntity with MongoDB auto-generated GUID support
   - Create SourceEntity and DestinationEntity
   - Implement custom exception types

3. **MongoDB Integration**
   - Configure BSON serialization for GUID auto-generation
   - Implement GuidGenerator class
   - Set up MongoDB connection and database configuration

### Phase 2: Repository Pattern Implementation (Week 1-2)
1. **Base Repository**
   - Implement IBaseRepository interface
   - Create BaseRepository abstract class with full CRUD operations
   - Add OpenTelemetry tracing to repository operations

2. **Specific Repositories**
   - Implement SourceEntityRepository with composite key validation
   - Implement DestinationEntityRepository with composite key validation
   - Create and test MongoDB indexes for performance

### Phase 3: API Layer Development (Week 2)
1. **Controllers Implementation**
   - Create SourcesController with full CRUD endpoints
   - Create DestinationsController with full CRUD endpoints
   - Implement proper error handling and validation

2. **Middleware and Configuration**
   - Add global exception handling middleware
   - Configure dependency injection
   - Set up health checks for MongoDB and RabbitMQ

### Phase 4: MassTransit Integration (Week 2-3)
1. **Message Contracts**
   - Define commands for CRUD operations
   - Define events for entity lifecycle notifications
   - Create request/response models

2. **Consumers Implementation**
   - Implement command consumers for both entities
   - Add proper error handling and retry policies
   - Integrate with repository layer

3. **Configuration**
   - Configure RabbitMQ transport
   - Set up message routing and endpoints
   - Add OpenTelemetry tracing for message operations

### Phase 5: Observability Implementation (Week 3)
1. **OpenTelemetry Setup**
   - Configure tracing for ASP.NET Core, MongoDB, and MassTransit
   - Set up metrics collection for performance monitoring
   - Configure log correlation and structured logging

2. **Docker Infrastructure**
   - Create Docker Compose for development environment
   - Set up OpenTelemetry Collector configuration
   - Configure Jaeger for trace visualization

### Phase 6: Testing Infrastructure (Week 3-4)
1. **Test Setup**
   - Create Docker Compose for test environment
   - Implement test fixtures for container management
   - Set up test data builders and utilities

2. **Integration Tests**
   - Write comprehensive repository tests
   - Create API endpoint tests
   - Implement MassTransit consumer tests

3. **Test Automation**
   - Configure test isolation and cleanup
   - Set up continuous integration pipeline
   - Add test coverage reporting

### Phase 7: Production Readiness (Week 4)
1. **Performance Optimization**
   - Optimize MongoDB queries and indexing
   - Configure connection pooling and timeouts
   - Add caching where appropriate

2. **Security and Validation**
   - Implement comprehensive input validation
   - Add authentication and authorization
   - Configure HTTPS and security headers

3. **Documentation and Deployment**
   - Create API documentation
   - Set up deployment scripts
   - Configure monitoring and alerting

## Success Criteria

### Functional Requirements ✅
- [ ] **CRUD Operations**: All Create, Read, Update, Delete operations work correctly via both REST API and MassTransit message bus
- [ ] **MongoDB Auto-Generated GUIDs**: All entities use MongoDB-generated GUID IDs with proper BSON configuration
- [ ] **Composite Key Validation**: Uniqueness validation works correctly for SourceEntity (address + version) and DestinationEntity (name + version)
- [ ] **Direct Entity Usage**: API controllers work directly with entities without DTOs or mapping layers
- [ ] **Repository Pattern**: Clean repository implementation using MongoDB.Driver directly without Entity Framework

### Infrastructure Requirements ✅
- [ ] **MongoDB Integration**: Proper connection, indexing, and GUID auto-generation working correctly
- [ ] **MassTransit with RabbitMQ**: Message bus fully configured with proper routing, retry policies, and error handling
- [ ] **OpenTelemetry Observability**: All logs, metrics, and traces properly sent to OpenTelemetry Collector
- [ ] **Docker Infrastructure**: Complete Docker Compose setup for both development and testing environments

### Testing Requirements ✅
- [ ] **Integration Tests**: Comprehensive test suites running against real Docker containers
- [ ] **Test Isolation**: Tests can run independently and clean up properly after execution
- [ ] **Container Management**: Automated container lifecycle management in test fixtures
- [ ] **Test Coverage**: High test coverage for repository operations, API endpoints, and message consumers

### Quality Requirements ✅
- [ ] **Error Handling**: Proper exception handling throughout the application with meaningful error messages
- [ ] **Validation**: Comprehensive input validation using FluentValidation
- [ ] **Logging**: Structured logging with proper correlation IDs and OpenTelemetry integration
- [ ] **Performance**: Optimized MongoDB queries with proper indexing and connection management

### Architecture Requirements ✅
- [ ] **Clean Architecture**: Clear separation of concerns across API, Application, Core, and Infrastructure layers
- [ ] **SOLID Principles**: Code follows SOLID principles with proper dependency injection and abstraction
- [ ] **Scalability**: Architecture supports easy addition of new entities and features
- [ ] **Maintainability**: Clean, readable code with proper documentation and consistent patterns

## Troubleshooting Guide

### Common Setup Issues

1. **MongoDB Connection Issues**
   - Verify MongoDB container is running: `docker ps`
   - Check connection string format and credentials
   - Ensure database name is correctly configured

2. **RabbitMQ Connection Issues**
   - Verify RabbitMQ management interface: http://localhost:15672
   - Check username/password configuration
   - Ensure virtual host exists and is accessible

3. **OpenTelemetry Issues**
   - Verify collector endpoint configuration
   - Check collector logs: `docker logs entitiesmanager-otel-collector`
   - Ensure Jaeger is accessible: http://localhost:16686

4. **Test Container Issues**
   - Ensure Docker is running and accessible
   - Check port conflicts with existing services
   - Verify test container cleanup in test fixtures

This comprehensive implementation plan provides a complete roadmap for building the EntitiesManager Web API with all specified requirements. Each section includes production-ready code examples and detailed configuration to ensure successful implementation.