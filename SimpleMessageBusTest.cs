using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SimpleMessageBusTest;

// Command classes matching the server
public class CreateSourceCommand
{
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

public class CreateDestinationCommand
{
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InputSchema { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Simple Message Bus Test - Commands Only");
        
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
        
        var publishEndpoint = host.Services.GetRequiredService<IPublishEndpoint>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            Console.WriteLine("\n📋 TESTING MESSAGE BUS COMMANDS");
            Console.WriteLine("=" + new string('=', 40));

            // Test 1: Create Source Entity via Message Bus
            Console.WriteLine("\n1️⃣ Publishing CREATE Source Command...");
            var createSourceCommand = new CreateSourceCommand
            {
                Address = "message-bus-test-address",
                Version = "1.0.0",
                Name = "Message Bus Test Source",
                Configuration = new Dictionary<string, object>
                {
                    ["endpoint"] = "http://message-bus-test.com",
                    ["timeout"] = 30
                },
                RequestedBy = "MessageBusTest"
            };

            await publishEndpoint.Publish(createSourceCommand);
            Console.WriteLine("✅ Source CREATE command published");

            // Test 2: Create Destination Entity via Message Bus
            Console.WriteLine("\n2️⃣ Publishing CREATE Destination Command...");
            var createDestinationCommand = new CreateDestinationCommand
            {
                Name = "Message Bus Test Destination",
                Version = "1.0.0",
                InputSchema = @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""testField"": { ""type"": ""string"" }
                    }
                }",
                RequestedBy = "MessageBusTest"
            };

            await publishEndpoint.Publish(createDestinationCommand);
            Console.WriteLine("✅ Destination CREATE command published");

            // Wait for processing
            Console.WriteLine("\n⏳ Waiting 5 seconds for message processing...");
            await Task.Delay(5000);

            // Verify via REST API
            Console.WriteLine("\n3️⃣ Verifying results via REST API...");
            await VerifyResults();

            Console.WriteLine("\n✅ Message Bus Test Completed!");
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

    private static async Task VerifyResults()
    {
        using var httpClient = new HttpClient();
        
        try
        {
            // Check Sources
            Console.WriteLine("🔍 Checking Sources...");
            var sourcesResponse = await httpClient.GetStringAsync("http://localhost:5130/api/sources");
            var sources = JsonSerializer.Deserialize<JsonElement[]>(sourcesResponse);
            
            var messageSource = sources.FirstOrDefault(s => 
                s.GetProperty("name").GetString() == "Message Bus Test Source");
            
            if (messageSource.ValueKind != JsonValueKind.Undefined)
            {
                Console.WriteLine($"✅ Source created via message bus: {messageSource.GetProperty("id")}");
            }
            else
            {
                Console.WriteLine("❌ Source NOT found - message bus command may have failed");
            }

            // Check Destinations
            Console.WriteLine("🔍 Checking Destinations...");
            var destinationsResponse = await httpClient.GetStringAsync("http://localhost:5130/api/destinations");
            var destinations = JsonSerializer.Deserialize<JsonElement[]>(destinationsResponse);
            
            var messageDestination = destinations.FirstOrDefault(d => 
                d.GetProperty("name").GetString() == "Message Bus Test Destination");
            
            if (messageDestination.ValueKind != JsonValueKind.Undefined)
            {
                Console.WriteLine($"✅ Destination created via message bus: {messageDestination.GetProperty("id")}");
            }
            else
            {
                Console.WriteLine("❌ Destination NOT found - message bus command may have failed");
            }

            Console.WriteLine($"\n📊 Total Sources: {sources.Length}");
            Console.WriteLine($"📊 Total Destinations: {destinations.Length}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Verification failed: {ex.Message}");
        }
    }
}
