using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageBusTestClient;

// Command classes
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

public class CreateDestinationCommand
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InputSchema { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
}

public class UpdateDestinationCommand
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InputSchema { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
}

public class DeleteDestinationCommand
{
    public Guid Id { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
}

public class GetDestinationQuery
{
    public Guid? Id { get; set; }
    public string? CompositeKey { get; set; }
}

// Entity response classes
public class SourceEntity
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class DestinationEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InputSchema { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Starting MassTransit CRUD Test Client...");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });
                    });
                });

                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        await host.StartAsync();

        var serviceProvider = host.Services;
        var publishEndpoint = serviceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            Console.WriteLine("\n📋 COMPREHENSIVE CRUD TESTS - MESSAGE BUS ONLY");
            Console.WriteLine("=" + new string('=', 59));

            // Test 1: Source Entity CRUD
            await TestSourceEntityCrud(publishEndpoint, serviceProvider, logger);

            // Test 2: Destination Entity CRUD
            await TestDestinationEntityCrud(publishEndpoint, serviceProvider, logger);

            Console.WriteLine("\n✅ ALL CRUD TESTS COMPLETED SUCCESSFULLY!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            logger.LogError(ex, "Test execution failed");
        }
        finally
        {
            await host.StopAsync();
        }
    }

    private static async Task TestSourceEntityCrud(IPublishEndpoint publishEndpoint, IServiceProvider serviceProvider, ILogger logger)
    {
        Console.WriteLine("\n🔧 TESTING SOURCE ENTITY CRUD VIA MESSAGE BUS");
        Console.WriteLine("-" + new string('-', 49));

        // CREATE Source Entity
        Console.WriteLine("1️⃣ Testing CREATE Source Entity...");
        var createSourceCommand = new CreateSourceCommand
        {
            Address = "test-source-address",
            Version = "1.0.0",
            Name = "Test Source Entity",
            Configuration = new Dictionary<string, object>
            {
                ["endpoint"] = "http://test-source.com",
                ["timeout"] = 30,
                ["retries"] = 3
            },
            RequestedBy = "TestClient"
        };

        await publishEndpoint.Publish(createSourceCommand);
        Console.WriteLine("✅ CREATE command published");
        await Task.Delay(3000); // Wait for processing

        // QUERY Source Entity (we'll need to get the ID first via query)
        Console.WriteLine("\n2️⃣ Testing QUERY Source Entity...");
        try
        {
            var queryClient = serviceProvider.GetRequiredService<IRequestClient<GetSourceQuery>>();
            var queryResponse = await queryClient.GetResponse<SourceEntity>(new GetSourceQuery
            {
                CompositeKey = "test-source-address_1.0.0"
            }, timeout: TimeSpan.FromSeconds(10));

            var sourceEntity = queryResponse.Message;
            Console.WriteLine($"✅ QUERY successful - Found entity with ID: {sourceEntity.Id}");

            // UPDATE Source Entity
            Console.WriteLine("\n3️⃣ Testing UPDATE Source Entity...");
            var updateSourceCommand = new UpdateSourceCommand
            {
                Id = sourceEntity.Id,
                Address = "test-source-address",
                Version = "1.0.0",
                Name = "Updated Test Source Entity",
                Configuration = new Dictionary<string, object>
                {
                    ["endpoint"] = "http://updated-test-source.com",
                    ["timeout"] = 60,
                    ["retries"] = 5,
                    ["newField"] = "added"
                },
                RequestedBy = "TestClient"
            };

            await publishEndpoint.Publish(updateSourceCommand);
            Console.WriteLine("✅ UPDATE command published");
            await Task.Delay(3000); // Wait for processing

            // DELETE Source Entity
            Console.WriteLine("\n4️⃣ Testing DELETE Source Entity...");
            var deleteSourceCommand = new DeleteSourceCommand
            {
                Id = sourceEntity.Id,
                RequestedBy = "TestClient"
            };

            await publishEndpoint.Publish(deleteSourceCommand);
            Console.WriteLine("✅ DELETE command published");
            await Task.Delay(3000); // Wait for processing
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Query failed: {ex.Message}");
            logger.LogError(ex, "Source entity query failed");
        }

        Console.WriteLine("🎉 Source Entity CRUD tests completed!");
    }

    private static async Task TestDestinationEntityCrud(IPublishEndpoint publishEndpoint, IServiceProvider serviceProvider, ILogger logger)
    {
        Console.WriteLine("\n🎯 TESTING DESTINATION ENTITY CRUD VIA MESSAGE BUS");
        Console.WriteLine("-" + new string('-', 49));

        // CREATE Destination Entity
        Console.WriteLine("1️⃣ Testing CREATE Destination Entity...");
        var createDestinationCommand = new CreateDestinationCommand
        {
            Name = "Test Destination Entity",
            Version = "2.0.0",
            InputSchema = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""name"": { ""type"": ""string"" },
                    ""age"": { ""type"": ""number"" },
                    ""email"": { ""type"": ""string"", ""format"": ""email"" }
                },
                ""required"": [""name"", ""email""]
            }",
            RequestedBy = "TestClient"
        };

        await publishEndpoint.Publish(createDestinationCommand);
        Console.WriteLine("✅ CREATE command published");
        await Task.Delay(3000); // Wait for processing

        // QUERY Destination Entity
        Console.WriteLine("\n2️⃣ Testing QUERY Destination Entity...");
        try
        {
            var queryClient = serviceProvider.GetRequiredService<IRequestClient<GetDestinationQuery>>();
            var queryResponse = await queryClient.GetResponse<DestinationEntity>(new GetDestinationQuery
            {
                CompositeKey = "Test Destination Entity_2.0.0"
            }, timeout: TimeSpan.FromSeconds(10));

            var destinationEntity = queryResponse.Message;
            Console.WriteLine($"✅ QUERY successful - Found entity with ID: {destinationEntity.Id}");

            // UPDATE Destination Entity
            Console.WriteLine("\n3️⃣ Testing UPDATE Destination Entity...");
            var updateDestinationCommand = new UpdateDestinationCommand
            {
                Id = destinationEntity.Id,
                Name = "Test Destination Entity",
                Version = "2.0.0",
                InputSchema = @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""name"": { ""type"": ""string"" },
                        ""age"": { ""type"": ""number"" },
                        ""email"": { ""type"": ""string"", ""format"": ""email"" },
                        ""phone"": { ""type"": ""string"" },
                        ""address"": { ""type"": ""string"" }
                    },
                    ""required"": [""name"", ""email"", ""phone""]
                }",
                RequestedBy = "TestClient"
            };

            await publishEndpoint.Publish(updateDestinationCommand);
            Console.WriteLine("✅ UPDATE command published");
            await Task.Delay(3000); // Wait for processing

            // DELETE Destination Entity
            Console.WriteLine("\n4️⃣ Testing DELETE Destination Entity...");
            var deleteDestinationCommand = new DeleteDestinationCommand
            {
                Id = destinationEntity.Id,
                RequestedBy = "TestClient"
            };

            await publishEndpoint.Publish(deleteDestinationCommand);
            Console.WriteLine("✅ DELETE command published");
            await Task.Delay(3000); // Wait for processing
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Query failed: {ex.Message}");
            logger.LogError(ex, "Destination entity query failed");
        }

        Console.WriteLine("🎉 Destination Entity CRUD tests completed!");
    }
}
