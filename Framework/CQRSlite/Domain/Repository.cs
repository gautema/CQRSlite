using System;
using System.Linq;
using System.Runtime.Serialization;
using CQRSlite.Domain.Exception;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;
using CQRSlite.Snapshotting;

namespace CQRSlite.Domain
{
    public class Repository : IRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEventPublisher _publisher;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotStrategy _snapshotStrategy;

        public Repository(IEventStore eventStore, IEventPublisher publisher, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy)
        {
            _eventStore = eventStore;
            _publisher = publisher;
            _snapshotStore = snapshotStore;
            _snapshotStrategy = snapshotStrategy;
        }

        public void Save<T>(T aggregate) where T : AggregateRoot
        {
            var i = 0;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                i++;
                @event.Version = aggregate.Version + i;
                @event.TimeStamp = DateTimeOffset.UtcNow;
                _eventStore.Save(aggregate.Id, @event);
                _publisher.Publish(@event);
            }
            TryMakeSnapshot(aggregate);
            aggregate.MarkChangesAsCommitted();
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            return LoadAggregate<T>(aggregateId);
        }

        public int GetVersion(Guid aggregateId)
        {
            return _eventStore.GetVersion(aggregateId);
        }

        private T LoadAggregate<T>(Guid id) where T : AggregateRoot
        {
            var aggregate = AggregateActivator.CreateAggregate<T>();
            var snapshotVersion = TryRestoreAggregateFromSnapshot(id, aggregate);

            var events = _eventStore.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            if (!events.Any() && snapshotVersion == -1)
                throw new AggregateNotFoundException();

            aggregate.LoadFromHistory(events);
            return aggregate;
        }

        private int TryRestoreAggregateFromSnapshot<T>(Guid id, T aggregate)
        {
            var version = -1;
            if (_snapshotStrategy.IsSnapshotable(typeof(T)))
            {
                var snapshot = _snapshotStore.Get(id);
                if (snapshot != null)
                {
                    aggregate.AsDynamic().Restore(snapshot);
                    version = snapshot.Version;
                }
            }
            return version;
        }

        private void TryMakeSnapshot(AggregateRoot aggregate)
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate))
                return;
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }
    }
}