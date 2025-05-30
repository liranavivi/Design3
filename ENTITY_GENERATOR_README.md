# EntitiesManager - Automated Entity Generator

This repository contains automated scripts to generate new entity types for the EntitiesManager microservice, following the established architectural patterns and best practices.

## 📋 Overview

The entity generator creates all necessary files and boilerplate code for a new entity type, including:

- ✅ **Entity Class** with proper BSON attributes and validation
- ✅ **Repository Interface & Implementation** with MongoDB integration
- ✅ **API Controller** with full CRUD endpoints
- ✅ **MassTransit Commands & Events** for message bus integration
- ✅ **4 MassTransit Consumers** (Create, Update, Delete, Get)
- ✅ **Integration Test Base** with Docker container setup
- ✅ **Configuration Instructions** for manual setup steps

## 🚀 Quick Start

### PowerShell (Windows)

```powershell
# Generate a new Processor entity
.\Generate-NewEntity.ps1 -EntityName "Processor"

# Dry run to see what would be generated
.\Generate-NewEntity.ps1 -EntityName "Workflow" -DryRun

# Specify custom project root
.\Generate-NewEntity.ps1 -EntityName "Pipeline" -ProjectRoot "C:\MyProject"
```

### Bash (Linux/macOS)

```bash
# Make script executable
chmod +x generate-new-entity.sh

# Generate a new Processor entity
./generate-new-entity.sh --name "Processor"

# Dry run to see what would be generated
./generate-new-entity.sh --name "Workflow" --dry-run

# Specify custom project root
./generate-new-entity.sh --name "Pipeline" --project-root "/path/to/project"
```

## 📁 Generated Files

The script generates the following file structure:

```
src/EntitiesManager/
├── EntitiesManager.Core/
│   ├── Entities/
│   │   └── {EntityName}Entity.cs
│   └── Interfaces/Repositories/
│       └── I{EntityName}EntityRepository.cs
├── EntitiesManager.Infrastructure/
│   ├── Repositories/
│   │   └── {EntityName}EntityRepository.cs
│   └── MassTransit/
│       ├── Commands/
│       │   └── {EntityName}Commands.cs
│       ├── Events/
│       │   └── {EntityName}Events.cs
│       └── Consumers/{EntityName}/
│           ├── Create{EntityName}CommandConsumer.cs
│           ├── Update{EntityName}CommandConsumer.cs
│           ├── Delete{EntityName}CommandConsumer.cs
│           └── Get{EntityName}QueryConsumer.cs
└── EntitiesManager.Api/
    └── Controllers/
        └── {EntityName}sController.cs

tests/EntitiesManager.IntegrationTests/
└── {EntityName}Tests/
    └── {EntityName}IntegrationTestBase.cs
```

## 🔧 Manual Configuration Steps

After running the script, you must complete these manual configuration steps:

### 1. Update BSON Configuration

**File**: `src/EntitiesManager/EntitiesManager.Infrastructure/MongoDB/BsonConfiguration.cs`

Add inside the `Configure()` method:

```csharp
if (!BsonClassMap.IsClassMapRegistered(typeof({EntityName}Entity)))
{
    BsonClassMap.RegisterClassMap<{EntityName}Entity>(cm =>
    {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true);
    });
}
```

### 2. Update MongoDB Configuration

**File**: `src/EntitiesManager/EntitiesManager.Api/Configuration/MongoDbConfiguration.cs`

Add in the `AddMongoDb` method:

```csharp
services.AddScoped<I{EntityName}EntityRepository, {EntityName}EntityRepository>();
```

### 3. Update MassTransit Configuration

**File**: `src/EntitiesManager/EntitiesManager.Api/Configuration/MassTransitConfiguration.cs`

Add in the `AddMassTransitWithRabbitMq` method:

```csharp
x.AddConsumer<Create{EntityName}CommandConsumer>();
x.AddConsumer<Update{EntityName}CommandConsumer>();
x.AddConsumer<Delete{EntityName}CommandConsumer>();
x.AddConsumer<Get{EntityName}QueryConsumer>();
```

## 🧪 Testing Your New Entity

### 1. Run Integration Tests

```bash
dotnet test tests/EntitiesManager.IntegrationTests/{EntityName}Tests/
```

### 2. Test the REST API

```bash
# Start the application
cd src/EntitiesManager/EntitiesManager.Api
dotnet run

# Create a new entity
curl -X POST http://localhost:5130/api/{entityname}s \
  -H "Content-Type: application/json" \
  -d '{"name":"Test{EntityName}","version":"1.0.0","description":"Test","isActive":true}'

# Get all entities
curl http://localhost:5130/api/{entityname}s

# Get by ID
curl http://localhost:5130/api/{entityname}s/{id}
```

### 3. Test Message Bus Integration

Use the existing `SimpleMessageBusTest` pattern to test MassTransit consumers.

## 📋 Entity Naming Rules

- **Must start with uppercase letter**
- **Only letters and numbers allowed**
- **Length between 3-50 characters**
- **Cannot conflict with existing entities** (Source, Destination, Base)

### Valid Examples
- ✅ `Processor`
- ✅ `Workflow`
- ✅ `Pipeline`
- ✅ `DataTransform`

### Invalid Examples
- ❌ `processor` (doesn't start with uppercase)
- ❌ `Data-Transform` (contains hyphen)
- ❌ `Source` (conflicts with existing entity)
- ❌ `AB` (too short)

## 🏗️ Architecture Patterns

The generated code follows these established patterns:

### Entity Pattern
- Inherits from `BaseEntity`
- Uses `[BsonElement]` attributes for MongoDB mapping
- Implements `GetCompositeKey()` for uniqueness validation
- Includes proper validation attributes

### Repository Pattern
- Implements `IBaseRepository<T>` interface
- Uses MongoDB.Driver directly (no Entity Framework)
- Auto-generates GUID IDs via MongoDB
- Publishes events through `IEventPublisher`

### API Pattern
- RESTful controller with standard CRUD endpoints
- Proper error handling and HTTP status codes
- Direct entity usage (no DTOs/AutoMapper)
- Structured logging integration

### MassTransit Pattern
- 4 consumers per entity type (Create, Update, Delete, Get)
- Separated into entity-specific folders
- Automatic endpoint configuration
- Comprehensive error handling and logging

### Testing Pattern
- Real Docker containers for integration tests
- MongoDB and RabbitMQ test containers
- Comprehensive CRUD test coverage
- Mock event publisher for isolated testing

## 🔍 Troubleshooting

### Common Issues

1. **"Required project path not found"**
   - Ensure you're running the script from the correct directory
   - Verify the project structure matches the expected layout

2. **"Entity name conflicts with existing entity"**
   - Choose a different entity name
   - Check existing entities in the Core/Entities folder

3. **Build errors after generation**
   - Ensure all manual configuration steps are completed
   - Check that using statements are correct
   - Verify BSON configuration is properly added

### Getting Help

1. Review the `DEVELOPER_MANUAL_ADD_NEW_ENTITY.md` for detailed implementation guidance
2. Check the `MANUAL_VERIFICATION_REPORT.md` for pattern verification
3. Examine existing entities (Source, Destination) for reference implementations

## 📊 Script Features

- ✅ **Input Validation**: Comprehensive entity name and project structure validation
- ✅ **Dry Run Mode**: Preview generated files without creating them
- ✅ **Error Handling**: Detailed error messages and validation
- ✅ **Cross-Platform**: PowerShell (Windows) and Bash (Linux/macOS) versions
- ✅ **Production Ready**: Follows established codebase patterns exactly
- ✅ **Comprehensive**: Generates all necessary files and configurations
- ✅ **Tested**: Verified against actual EntitiesManager codebase patterns

## 🎯 Next Steps

After successfully generating and configuring your new entity:

1. **Customize the Entity**: Add entity-specific properties and validation
2. **Extend Repository**: Add custom query methods as needed
3. **Enhance Controller**: Add entity-specific endpoints
4. **Write Tests**: Create comprehensive unit and integration tests
5. **Update Documentation**: Document your new entity's purpose and usage

The generated code provides a solid foundation that follows all established patterns and can be easily extended for your specific requirements.
