# Implementing an event store

CQRSlite doesn't ship an event store. You write one against your own database before you go to production. This guide covers what the store must do, a SQL example, and the problems you will run into: concurrent writes, serialization, and publishing.

If you haven't seen the sample's in-memory store yet, start with [the event store part of the tutorial](../tutorial/3-event-store.md).

## The contract

<!-- snippet: Framework/CQRSlite/Events/IEventStore.cs -->
```csharp
public interface IEventStore
{
    // ...
    Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);
    // ...
    Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default);
}
```

**`Save`** gets the new events of one aggregate, in version order. The framework has already set `Id` (the aggregate's id), `Version` and `TimeStamp` on each event, so store them as they are. `Save` must:

- store all of the events or none of them,
- refuse to store an event whose aggregate id and version already exist (see the next section),
- publish the events once they are stored, if anything should react to them (see [Publishing events](#publishing-events)).

**`Get`** returns the events of one aggregate with a version greater than `fromVersion`, ordered by version. `-1` means all of them. When there are none, return an empty list, not `null`. The repository turns "no events at all" into `AggregateNotFoundException`.

## Rejecting concurrent writes

This is the one thing your store must get right.

When a command handler commits, `Repository.Save` first asks the store for any events after the version the aggregate was loaded at. If there are some, someone else changed the aggregate in the meantime and it throws `ConcurrencyException`. Then it saves the new events.

That check alone isn't enough, because it isn't atomic. Two requests can both load version 5, both check ("nothing after 5"), and both save an event with version 6. If the store accepts both, the aggregate now has two version 6 events. Every later load fails with `EventsOutOfOrderException`, and one of the changes was made without seeing the other.

So the store itself must refuse an event whose (aggregate id, version) is already stored, atomically with the insert. In a database, that is a primary key or unique index on `(aggregate_id, version)`. Turn the key violation into `ConcurrencyException` so callers see the same exception either way, and store nothing from that `Save`.

The sample's in-memory store does the same thing with a lock:

<!-- snippet: Sample/CQRSCode/WriteModel/InMemoryEventStore.cs -->
```csharp
public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
{
    var newEvents = events.ToArray();
    lock (_inMemoryDb)
    {
        // Check everything before storing anything, so a refused save stores nothing
        foreach (var aggregateEvents in newEvents.GroupBy(e => e.Id))
        {
            var storedCount = _inMemoryDb.TryGetValue(aggregateEvents.Key, out var stored) ? stored.Count : 0;
            if (aggregateEvents.First().Version != storedCount + 1)
            {
                throw new ConcurrencyException(aggregateEvents.Key);
            }
        }
        // ...
    }

    // Publish only once the events are stored
    foreach (var @event in newEvents)
    {
        await _publisher.Publish(@event, cancellationToken);
    }
}
```

## A SQL event store

Here is a complete store for PostgreSQL using plain ADO.NET. Other databases need different SQL and a different duplicate-key check, but the shape is the same.

The table. The primary key is what rejects concurrent writes. `sequence` gives every event a global order, which you need to rebuild read models:

```sql
CREATE TABLE events (
    sequence     bigint      GENERATED ALWAYS AS IDENTITY UNIQUE,
    aggregate_id uuid        NOT NULL,
    version      int         NOT NULL,
    event_type   text        NOT NULL,
    data         text        NOT NULL,
    time_stamp   timestamptz NOT NULL,
    PRIMARY KEY (aggregate_id, version)
);
```

The store:

```csharp
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CQRSlite.Domain.Exception;
using CQRSlite.Events;

public class SqlEventStore : IEventStore
{
    // IncludeFields because events often use readonly fields, like the sample's do.
    // Without it System.Text.Json silently leaves them out.
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private readonly DbDataSource _dataSource;
    private readonly IEventPublisher _publisher;
    private readonly Dictionary<string, Type> _eventTypes;

    // eventTypes: every event class that can be stored. The stored name is part of your schema.
    public SqlEventStore(DbDataSource dataSource, IEventPublisher publisher, IEnumerable<Type> eventTypes)
    {
        _dataSource = dataSource;
        _publisher = publisher;
        _eventTypes = eventTypes.ToDictionary(t => t.Name);
    }

    public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        var newEvents = events.ToArray();

        await using (var connection = await _dataSource.OpenConnectionAsync(cancellationToken))
        await using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
        {
            foreach (var @event in newEvents)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO events (aggregate_id, version, event_type, data, time_stamp)
                    VALUES (@AggregateId, @Version, @EventType, @Data, @TimeStamp)
                    """;
                AddParameter(command, "AggregateId", @event.Id);
                AddParameter(command, "Version", @event.Version);
                AddParameter(command, "EventType", @event.GetType().Name);
                AddParameter(command, "Data", JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions));
                AddParameter(command, "TimeStamp", @event.TimeStamp);

                try
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (DbException e) when (IsDuplicateKey(e))
                {
                    // Someone else stored this version first. The transaction is rolled back
                    // when it is disposed, so none of these events are stored.
                    throw new ConcurrencyException(@event.Id);
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }

        // Publish only after the commit. See "Publishing events" for what this does not guarantee.
        foreach (var @event in newEvents)
        {
            await _publisher.Publish(@event, cancellationToken);
        }
    }

    public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT event_type, data FROM events
            WHERE aggregate_id = @AggregateId AND version > @FromVersion
            ORDER BY version
            """;
        AddParameter(command, "AggregateId", aggregateId);
        AddParameter(command, "FromVersion", fromVersion);

        var events = new List<IEvent>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(Deserialize(reader.GetString(0), reader.GetString(1)));
        }
        return events;
    }

    // Not part of IEventStore: every event in the order it was stored, for rebuilding read models
    public async IAsyncEnumerable<IEvent> ReadAll([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT event_type, data FROM events ORDER BY sequence";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return Deserialize(reader.GetString(0), reader.GetString(1));
        }
    }

    private IEvent Deserialize(string eventType, string data) =>
        (IEvent)JsonSerializer.Deserialize(data, _eventTypes[eventType], JsonOptions)!;

    // PostgreSQL reports a unique key violation as SQLSTATE 23505.
    // On SQL Server, check for SqlException.Number 2627 or 2601 instead.
    protected virtual bool IsDuplicateKey(DbException e) => e.SqlState == "23505";

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
```

Register it as a singleton. `DbDataSource` comes from your database provider (for PostgreSQL, `NpgsqlDataSource.Create(connectionString)`):

```csharp
var eventTypes = typeof(InventoryItemCreated).Assembly.GetTypes()
    .Where(t => typeof(IEvent).IsAssignableFrom(t) && !t.IsAbstract);

builder.Services.AddSingleton<IEventStore>(sp =>
    new SqlEventStore(dataSource, sp.GetRequiredService<IEventPublisher>(), eventTypes));
```

## Storing events

Events are stored forever, so whatever you write today has to be readable for the life of the system.

**Store a stable type name.** The example stores the class name (`InventoryItemCreated`) and maps it back with a dictionary. Avoid storing `Type.AssemblyQualifiedName`: it includes the namespace, assembly name and version, so moving or renaming anything makes old events unreadable. With a name map, a renamed class only needs its old name added to the map.

**Check what your serializer actually writes.** The sample's events keep their data in public readonly fields set by a constructor. System.Text.Json ignores fields unless you set `IncludeFields = true`, and it doesn't fail: it writes `{"Id":...,"Version":...}` without the data and you only find out when you read it back. Test a round trip of every event type.

**Don't change stored events.** Never edit, reorder or delete rows. To change an event's shape over time:

- Add new properties with sensible defaults. Old events deserialize with the default.
- For a change in meaning, add a new event type (`ItemsCheckedInToInventoryV2`) and keep handling the old one.
- If old events need converting, convert them when reading ("upcasting"), in `Deserialize`, rather than rewriting the table.

## Publishing events

The read model learns about changes through published events, and the natural place to publish is the event store: it knows exactly which events were stored. Publish after the commit, never before. Otherwise a refused save would still update the read model.

With the in-process `Router`, `Publish` calls the event handlers directly, inside the same request. That keeps the read model up to date by the time the command returns, which is convenient. It also has two consequences:

- **A failing event handler fails the command.** The events are already stored, but the exception reaches the code that sent the command, which will report an error for a change that did happen. Keep event handlers simple, or catch and log in them.
- **Events can be lost.** If the process stops between the commit and the publish, the events are stored but never published, and the read model never sees them. Storing and publishing are two separate writes, and nothing makes them happen together.

For production, the usual answer to the second problem is an **outbox**: in the same transaction as the events, record that they still need publishing (or use the events table itself and remember the last published `sequence`). A background process publishes from there and marks them done. Delivery is then at least once, so event handlers must cope with seeing an event twice. For example, a read model can skip events whose version it has already applied.

If you read new events by `sequence` from a background process, keep in mind that sequence numbers are given out when rows are inserted, not when transactions commit. A reader can see 12 before 11 is committed and must not skip 11 for good.

[Taking it to production](../tutorial/7-production.md) covers this and the other gaps in the sample.

## Rebuilding read models

Because the events are the source of truth, any read model can be thrown away and rebuilt: clear its tables and replay every event, in the order they were stored, through its event handlers. That's what `ReadAll` in the example is for. It is also how you add a new read model to an existing system.

```csharp
public static async Task Rebuild(SqlEventStore eventStore, IEventPublisher readModelOnlyPublisher)
{
    // Clear the read model's tables first, then:
    await foreach (var @event in eventStore.ReadAll())
    {
        await readModelOnlyPublisher.Publish(@event);
    }
}
```

Publish only to the read model you're rebuilding (for example a separate `Router` with just its handlers registered), not to everything that reacts to events, or you will send the same emails twice.

## Performance

- Loading an aggregate reads all its events. That's fast for hundreds of events. For aggregates with long histories, add [snapshots](snapshots.md) or [caching](caching.md).
- The primary key on `(aggregate_id, version)` is also the index `Get` needs.
- The table is append-only: no updates and no deletes, which databases handle well.
