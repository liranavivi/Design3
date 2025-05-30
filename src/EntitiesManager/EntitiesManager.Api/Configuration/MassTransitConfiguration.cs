using EntitiesManager.Infrastructure.MassTransit.Consumers.Source;
using EntitiesManager.Infrastructure.MassTransit.Consumers.Destination;
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
                // cfg.UseInMemoryOutbox(); // Commented out due to obsolete warning

                // Configure endpoints to use message type routing
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
