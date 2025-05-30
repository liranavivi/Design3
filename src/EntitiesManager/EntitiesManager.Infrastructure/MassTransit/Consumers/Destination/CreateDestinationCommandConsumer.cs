using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MassTransit.Commands;
using EntitiesManager.Infrastructure.MassTransit.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EntitiesManager.Infrastructure.MassTransit.Consumers.Destination;

public class CreateDestinationCommandConsumer : IConsumer<CreateDestinationCommand>
{
    private readonly IDestinationEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateDestinationCommandConsumer> _logger;

    public CreateDestinationCommandConsumer(
        IDestinationEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateDestinationCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateDestinationCommand> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("CreateDestinationCommand");
        activity?.SetTag("command.type", "CreateDestination");
        activity?.SetTag("command.name", context.Message.Name);
        activity?.SetTag("command.version", context.Message.Version);

        try
        {
            var entity = new DestinationEntity
            {
                Version = context.Message.Version,
                Name = context.Message.Name,
                InputSchema = context.Message.InputSchema,
                CreatedBy = context.Message.RequestedBy
            };

            var created = await _repository.CreateAsync(entity);

            await _publishEndpoint.Publish(new DestinationCreatedEvent
            {
                Id = created.Id,
                Version = created.Version,
                Name = created.Name,
                InputSchema = created.InputSchema,
                CreatedAt = created.CreatedAt,
                CreatedBy = created.CreatedBy
            });

            await context.RespondAsync(created);

            _logger.LogInformation("Successfully processed CreateDestinationCommand for {Name}_{Version}",
                context.Message.Name, context.Message.Version);
        }
        catch (DuplicateKeyException ex)
        {
            _logger.LogWarning("Duplicate key error in CreateDestinationCommand: {Error}", ex.Message);
            await context.RespondAsync(new { Error = ex.Message, Type = "DuplicateKey" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CreateDestinationCommand");
            await context.RespondAsync(new { Error = ex.Message, Type = "InternalError" });
            throw;
        }
    }
}
