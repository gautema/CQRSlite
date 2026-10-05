using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Snapshotting;

namespace CQRSlite.Tests.Extensions.TestHelpers;

public abstract class Specification<TAggregate, THandler, TCommand> 
    where TAggregate: AggregateRoot
    where THandler : class
    where TCommand : ICommand
{

    protected TAggregate? Aggregate { get; set; }
    protected ISession Session { get; set; }
    protected abstract IEnumerable<IEvent> Given();
    protected abstract TCommand When();
    protected abstract THandler BuildHandler();

    protected Snapshot? Snapshot { get; set; }
    protected IList<IEvent> EventDescriptors { get; set; }
    protected IList<IEvent> PublishedEvents { get; set; }

    public Specification()
    {
        var eventpublisher = new SpecEventPublisher();
        var given = Given().ToList();
        var eventstorage = new SpecEventStorage(eventpublisher, given);
        var snapshotstorage = new SpecSnapShotStorage(Snapshot);

        var snapshotStrategy = new DefaultSnapshotStrategy();
        var repository = new SnapshotRepository(snapshotstorage, snapshotStrategy, new Repository(eventstorage), eventstorage);
        Session = new Session(repository);
        Aggregate = GetAggregate(given).GetAwaiter().GetResult();

        var handler = BuildHandler();
        var handling = handler switch
        {
            ICancellableCommandHandler<TCommand> cancellableHandler => cancellableHandler.Handle(When(), CancellationToken.None),
            ICommandHandler<TCommand> commandHandler => commandHandler.Handle(When()),
            _ => throw new InvalidCastException($"{typeof(THandler).Name} is not a command handler of type {typeof(TCommand)}")
        };
        handling.GetAwaiter().GetResult();

        Snapshot = snapshotstorage.Snapshot;
        PublishedEvents = eventpublisher.PublishedEvents;
        EventDescriptors = eventstorage.Events;
    }

    // Loaded through the session, so after the command it is the same instance the handler changed
    private async Task<TAggregate?> GetAggregate(List<IEvent> given)
    {
        if (given.Count == 0)
        {
            return null;
        }
        return await Session.Get<TAggregate>(given[0].Id);
    }
}

internal class SpecSnapShotStorage : ISnapshotStore
{
    public SpecSnapShotStorage(Snapshot? snapshot)
    {
        Snapshot = snapshot;
    }

    public Snapshot? Snapshot { get; set; }

    public Task<Snapshot?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Snapshot);
    }

    public Task Save(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        Snapshot = snapshot;
        return Task.CompletedTask;
    }
}

internal class SpecEventPublisher : IEventPublisher
{
    public SpecEventPublisher()
    {
        PublishedEvents = new List<IEvent>();
    }

    public Task Publish<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        PublishedEvents.Add(@event);
        return Task.CompletedTask;
    }

    public IList<IEvent> PublishedEvents { get; set; }
}

internal class SpecEventStorage : IEventStore
{
    private readonly IEventPublisher _publisher;

    public SpecEventStorage(IEventPublisher publisher, List<IEvent> events)
    {
        _publisher = publisher;
        Events = events;
    }

    public List<IEvent> Events { get; set; }

    public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        Events.AddRange(events);
        return Task.WhenAll(events.Select(evt =>_publisher.Publish(evt, cancellationToken)));
            
    }

    public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
    {
        var events = Events.Where(x => x.Id == aggregateId && x.Version > fromVersion);
        return Task.FromResult(events);
    }
}
