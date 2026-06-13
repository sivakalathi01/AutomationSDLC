namespace Virtusa.Agentic.OrchestratorApi.Services;

using Virtusa.Agentic.Shared.Contracts;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

/// <summary>
/// Event Store implementation using Azure Cosmos DB.
/// Provides immutable append-only log for all run events.
/// </summary>
public interface IEventStore
{
    Task AppendEventAsync(DomainEvent @event);
    Task<List<DomainEvent>> QueryEventsAsync(string runId);
    Task<List<DomainEvent>> QueryEventsByTypeAsync(string eventType);
}

public class EventStore : IEventStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<EventStore> _logger;
    private Container? _container;

    public EventStore(CosmosClient cosmosClient, ILogger<EventStore> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
    }

    private Task<Container> GetContainerAsync()
    {
        if (_container != null) return Task.FromResult(_container);

        var database = _cosmosClient.GetDatabase("AgenticDb");
        _container = database.GetContainer("events");
        return Task.FromResult(_container);
    }

    public async Task AppendEventAsync(DomainEvent @event)
    {
        try
        {
            var container = await GetContainerAsync();
            await container.CreateItemAsync(@event, new PartitionKey(@event.RunId));
            _logger.LogInformation("Event {EventType} appended to run {RunId}", @event.EventType, @event.RunId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append event to event store");
            throw;
        }
    }

    public async Task<List<DomainEvent>> QueryEventsAsync(string runId)
    {
        try
        {
            var container = await GetContainerAsync();
            var query = container.GetItemLinqQueryable<DomainEvent>()
                .Where(e => e.RunId == runId)
                .OrderBy(e => e.OccurredAt);

            var iterator = query.ToFeedIterator();
            var events = new List<DomainEvent>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                events.AddRange(results);
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query events for run {RunId}", runId);
            return new List<DomainEvent>();
        }
    }

    public async Task<List<DomainEvent>> QueryEventsByTypeAsync(string eventType)
    {
        try
        {
            var container = await GetContainerAsync();
            var query = container.GetItemLinqQueryable<DomainEvent>()
                .Where(e => e.EventType == eventType)
                .OrderBy(e => e.OccurredAt);

            var iterator = query.ToFeedIterator();
            var events = new List<DomainEvent>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                events.AddRange(results);
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query events by type {EventType}", eventType);
            return new List<DomainEvent>();
        }
    }
}

public class InMemoryEventStore : IEventStore
{
    private static readonly List<DomainEvent> Events = new();

    public Task AppendEventAsync(DomainEvent @event)
    {
        if (string.IsNullOrWhiteSpace(@event.EventType))
        {
            @event.EventType = @event.GetType().Name;
        }

        Events.Add(@event);
        return Task.CompletedTask;
    }

    public Task<List<DomainEvent>> QueryEventsAsync(string runId)
    {
        var results = Events
            .Where(e => e.RunId == runId)
            .OrderBy(e => e.OccurredAt)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<List<DomainEvent>> QueryEventsByTypeAsync(string eventType)
    {
        var results = Events
            .Where(e => e.EventType == eventType)
            .OrderBy(e => e.OccurredAt)
            .ToList();
        return Task.FromResult(results);
    }
}
