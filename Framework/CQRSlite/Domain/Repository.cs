using System;
using System.Linq;
using CQRSlite.Eventing;

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
                _storage.Save(aggregate.Id, new EventDescriptor(aggregate.Id, @event, version));
                _publisher.Publish(@event);
            }

            var shouldMakeSnapshot = ShouldMakeSnapShot(aggregate, expectedVersion);
            aggregate.MarkChangesAsCommitted();

            if (shouldMakeSnapshot) MakeSnapshot(aggregate, version);
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

            var eventDescriptors = _storage.Get(id, version);
            var events = eventDescriptors.Where(desc => desc.Version > version).Select(desc => desc.EventData);
            aggregate.LoadsFromHistory(events);

            if (eventDescriptors.Count() == 0 && version == -1)
            {
                throw new AggregateNotFoundException();
            }
            return aggregate;
        }

        private T CreateAggregate() {
            T obj;
            try
            {
                obj = (T) Activator.CreateInstance(typeof (T), true);
            }
            catch(MissingMethodException)
            {
                throw new AggreagateMissingParameterlessConstructorException();
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

