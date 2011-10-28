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
        private const int SnapshotInterval = 15;

        public Repository(IEventStore storage, ISnapshotStore snapshotStore, IEventPublisher publisher)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
            _publisher = publisher;
        }

        public void Save(T aggregate, int expectedVersion)
        {
            if (_storage.GetVersion(aggregate.Id) != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }

            var version = expectedVersion;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                version++;
                @event.Version = version;
                @event.Timestamp = DateTimeOffset.UtcNow;
                _storage.Save(aggregate.Id, @event);
                _publisher.Publish(@event);
            }

            if (ShouldMakeSnapShot(aggregate, expectedVersion)) 
                MakeSnapshot(aggregate, version);
            aggregate.MarkChangesAsCommitted();
        }

        private void MakeSnapshot(T aggregate, int version) 
        {
            var snapshot = aggregate.AsDynamic().GetSnapshot();
            snapshot.RealObject.Version = version;
            _snapshotStore.Save(snapshot.RealObject);
        }

        private bool ShouldMakeSnapShot(AggregateRoot aggregate, int expectedVersion)
        {
            if(!IsSnapshotable(typeof(T))) return false;
            var i = expectedVersion;

            for (var j = 0; j < aggregate.GetUncommittedChanges().Count(); j++)
                if (++i % SnapshotInterval == 0 && i != 0) return true;
            return false;
        }

        public T Get(Guid id)
        {
            var aggregate = CreateAggregate();
            var snapshotVersion = RestoreAggregateFromSnapshot(id, aggregate);

            var events = _storage.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadsFromHistory(events);

            if (events.Count() == 0 && snapshotVersion == -1)
            {
                throw new AggregateNotFoundException();
            }
            return aggregate;
        }

        private int RestoreAggregateFromSnapshot(Guid id, T aggregate)
        {
            var version = -1;
            if (IsSnapshotable(typeof(T)) && _snapshotStore != null)
            {
                var snapshot = _snapshotStore.Get(id);
                if(snapshot != null)
                {
                    aggregate.AsDynamic().Restore(snapshot);
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

        private bool IsSnapshotable(Type aggregateType)
        {
            if(aggregateType.BaseType == null)
                return false;
            if (aggregateType.BaseType.IsGenericType &&
                aggregateType.BaseType.GetGenericTypeDefinition() == typeof (SnapshotAggregateRoot<>))
                return true;
            return IsSnapshotable(aggregateType.BaseType);
        }
    }
}

