namespace Virtusa.Agentic.OrchestratorApi.Services;

using Virtusa.Agentic.Shared.Contracts;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

/// <summary>
/// Service Bus publisher for async inter-service communication.
/// </summary>
public interface IServiceBusPublisher
{
    Task PublishCommandAsync<T>(string topicName, T command) where T : ServiceBusCommand;
    Task PublishEventAsync<T>(string topicName, T @event) where T : ServiceBusEvent;
}

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(ServiceBusClient serviceBusClient, ILogger<ServiceBusPublisher> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task PublishCommandAsync<T>(string topicName, T command) where T : ServiceBusCommand
    {
        try
        {
            var sender = _serviceBusClient.CreateSender(topicName);
            var json = JsonSerializer.Serialize(command);
            var message = new ServiceBusMessage(json)
            {
                CorrelationId = command.CorrelationId,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Command {CommandType} published to topic {TopicName}", typeof(T).Name, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish command to topic {TopicName}", topicName);
            throw;
        }
    }

    public async Task PublishEventAsync<T>(string topicName, T @event) where T : ServiceBusEvent
    {
        try
        {
            var sender = _serviceBusClient.CreateSender(topicName);
            var json = JsonSerializer.Serialize(@event);
            var message = new ServiceBusMessage(json)
            {
                CorrelationId = @event.CorrelationId,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Event {EventType} published to topic {TopicName}", typeof(T).Name, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event to topic {TopicName}", topicName);
            throw;
        }
    }
}

public class NoopServiceBusPublisher : IServiceBusPublisher
{
    public Task PublishCommandAsync<T>(string topicName, T command) where T : ServiceBusCommand
    {
        return Task.CompletedTask;
    }

    public Task PublishEventAsync<T>(string topicName, T @event) where T : ServiceBusEvent
    {
        return Task.CompletedTask;
    }
}
