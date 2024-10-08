using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace WebApi;

public class EventStoreRepository(EventStoreClientSettings settings)
{
    private readonly EventStoreClient _client = new(settings);
    public async Task AppendEvents(string streamName, object data, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        var eventData = new EventData(
            Uuid.NewUuid(), 
            data.GetType().AssemblyQualifiedName!, 
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)), 
            metadata: null);

        await _client.AppendToStreamAsync(
            streamName, 
            StreamState.Any,
            [eventData], 
            cancellationToken: cancellationToken);
    }

    public async Task<T> LoadAsync<T>(string streamName, CancellationToken cancellationToken) where T : IAggregate, new()
    {
        var aggregate = new T();
        var events = _client.ReadStreamAsync(
            Direction.Forwards, 
            streamName, 
            StreamPosition.Start, 
            cancellationToken: cancellationToken);

        await foreach (var resolvedEvent in events)
        {
            var eventType = Type.GetType(resolvedEvent.Event.EventType);
            var eventData = JsonSerializer.Deserialize(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), 
                eventType!);

            if (eventData != null) aggregate.ApplyEvent(eventData);
        }

        return aggregate;
    }

    public async Task<List<T>> LoadListAsync<T>(CancellationToken cancellationToken) where T : IAggregate, new()
    {
        var aggregates = new Dictionary<string, T>();
        
        var events = _client.ReadAllAsync(
            Direction.Forwards, 
            Position.Start, 
            cancellationToken: cancellationToken);

        await foreach (var resolvedEvent in events)
        {
            var streamId = resolvedEvent.Event.EventStreamId;
            var eventType = Type.GetType(resolvedEvent.Event.EventType);
            if (!IsRelevantEventForAggregate<T>(eventType!)) continue;
            
            if (!aggregates.TryGetValue(streamId, out var value))
            {
                value = new();
                aggregates[streamId] = value;
            }
                
            var eventData = JsonSerializer.Deserialize(
                Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), 
                eventType!);
                
            if (eventData != null) value.ApplyEvent(eventData);
        }

        return aggregates.Values.ToList();
    }

    public async Task DeleteStreamAsync(string streamName)
    {
        await _client.TombstoneAsync(streamName, StreamState.Any);
    }

    private static bool IsRelevantEventForAggregate<T>(Type eventType)
    {
        return AggregateEventMapping.TryGetValue(typeof(T), out var relevantEvents) && 
               relevantEvents.Contains(eventType);
    }

    private readonly static Dictionary<Type, List<Type>> AggregateEventMapping = new()
    {
        { typeof(Account), [typeof(AccountOpened), typeof(MoneyDeposited), typeof(MoneyWithdrawn)]},
    };
}