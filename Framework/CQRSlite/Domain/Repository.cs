using System;
using System.Linq;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IEventPublisher _publisher;
        private readonly ISnapshotStrategy _snapshotStrategy;

        public Repository(IEventStore storage, ISnapshotStore snapshotStore, IEventPublisher publisher, ISnapshotStrategy snapshotStrategy)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
            _publisher = publisher;
            _snapshotStrategy = snapshotStrategy;
        }

        public void Save(T aggregate, int expectedVersion)
        {
            if (_storage.GetVersion(aggregate.Id) != expectedVersion && expectedVersion != -1)
                throw new ConcurrencyException();
            SaveAndPublishUncommitedEvents(aggregate, expectedVersion);
            TryMakeSnapshot(aggregate, expectedVersion);
            aggregate.MarkChangesAsCommitted();
        }

        private void SaveAndPublishUncommitedEvents(T aggregate, int expectedVersion)
        {
            var version = expectedVersion;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                version++;
                @event.Version = version;
                @event.Timestamp = DateTimeOffset.UtcNow;
                _storage.Save(aggregate.Id, @event);
                _publisher.Publish(@event);
            }
        }

        private void TryMakeSnapshot(T aggregate, int expectedVersion) 
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate, expectedVersion))
                return;
            var snapshot = aggregate.MakeSnapshot();
            snapshot.Version = expectedVersion + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }

        public T Get(Guid id)
        {
            var aggregate = CreateAggregate();
            var snapshotVersion = TryRestoreAggregateFromSnapshot(id, aggregate);

            var events = _storage.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadsFromHistory(events);

            if (events.Count() == 0 && snapshotVersion == -1)
            {
                throw new AggregateNotFoundException();
            }
            return aggregate;
        }

        private int TryRestoreAggregateFromSnapshot(Guid id, T aggregate)
        {
            var version = -1;
            if (_snapshotStrategy.IsSnapshotable(typeof(T)) && _snapshotStore != null)
            {
                var snapshot = _snapshotStore.Get(id);
                if(snapshot != null)
                {
                    aggregate.RestoreFromSnapshot(snapshot);
                    version = snapshot.Version;
                }
            }
            return version;
        }

        private T CreateAggregate() 
        {
            T obj;
            try
            {
                obj = (T) Activator.CreateInstance(typeof (T), true);
            }
            catch(MissingMethodException)
            {
                throw new AggregateMissingParameterlessConstructorException();
            }
            return obj;
        }
    }
}

