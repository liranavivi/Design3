using EntitiesManager.Core.Exceptions;
using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MassTransit.Commands;
using EntitiesManager.Infrastructure.MassTransit.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EntitiesManager.Infrastructure.MassTransit.Consumers.Destination;

public class UpdateDestinationCommandConsumer : IConsumer<UpdateDestinationCommand>
{
    private readonly IDestinationEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateDestinationCommandConsumer> _logger;

    public UpdateDestinationCommandConsumer(
        IDestinationEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<UpdateDestinationCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateDestinationCommand> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("UpdateDestinationCommand");
        activity?.SetTag("command.type", "UpdateDestination");
        activity?.SetTag("command.id", context.Message.Id.ToString());

        try
        {
            var existing = await _repository.GetByIdAsync(context.Message.Id);
            if (existing == null)
            {
                await context.RespondAsync(new { Error = "Destination not found", Type = "NotFound" });
                return;
            }

            existing.Name = context.Message.Name;
            existing.Version = context.Message.Version;
            existing.InputSchema = context.Message.InputSchema;
            existing.UpdatedBy = context.Message.RequestedBy;

            var updated = await _repository.UpdateAsync(existing);

            await _publishEndpoint.Publish(new DestinationUpdatedEvent
            {
                Id = updated.Id,
                Version = updated.Version,
                Name = updated.Name,
                InputSchema = updated.InputSchema,
                UpdatedAt = updated.UpdatedAt,
                UpdatedBy = updated.UpdatedBy
            });

            await context.RespondAsync(updated);

            _logger.LogInformation("Successfully processed UpdateDestinationCommand for ID {Id}", context.Message.Id);
        }
        catch (DuplicateKeyException ex)
        {
            _logger.LogWarning("Duplicate key error in UpdateDestinationCommand: {Error}", ex.Message);
            await context.RespondAsync(new { Error = ex.Message, Type = "DuplicateKey" });
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("Entity not found in UpdateDestinationCommand: {Error}", ex.Message);
            await context.RespondAsync(new { Error = ex.Message, Type = "NotFound" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UpdateDestinationCommand");
            await context.RespondAsync(new { Error = ex.Message, Type = "InternalError" });
            throw;
        }
    }
}
