using EntitiesManager.Core.Interfaces.Repositories;
using EntitiesManager.Infrastructure.MassTransit.Commands;
using EntitiesManager.Infrastructure.MassTransit.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EntitiesManager.Infrastructure.MassTransit.Consumers.Destination;

public class DeleteDestinationCommandConsumer : IConsumer<DeleteDestinationCommand>
{
    private readonly IDestinationEntityRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DeleteDestinationCommandConsumer> _logger;

    public DeleteDestinationCommandConsumer(
        IDestinationEntityRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<DeleteDestinationCommandConsumer> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteDestinationCommand> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("DeleteDestinationCommand");
        activity?.SetTag("command.type", "DeleteDestination");
        activity?.SetTag("command.id", context.Message.Id.ToString());

        try
        {
            var deleted = await _repository.DeleteAsync(context.Message.Id);

            if (!deleted)
            {
                await context.RespondAsync(new { Error = "Destination not found", Type = "NotFound" });
                return;
            }

            await _publishEndpoint.Publish(new DestinationDeletedEvent
            {
                Id = context.Message.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = context.Message.RequestedBy
            });

            await context.RespondAsync(new { Success = true, Message = "Destination deleted successfully" });

            _logger.LogInformation("Successfully processed DeleteDestinationCommand for ID {Id}", context.Message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DeleteDestinationCommand");
            await context.RespondAsync(new { Error = ex.Message, Type = "InternalError" });
            throw;
        }
    }
}
