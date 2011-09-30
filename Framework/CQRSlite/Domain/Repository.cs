using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IEventPublisher _publisher;
        private readonly Dictionary<Guid,AggregateRoot> _trackedAggregates;
        private const int SnapshotInterval = 15;

        public Repository(IEventStore storage, ISnapshotStore snapshotStore, IEventPublisher publisher)
        {
            _trackedAggregates = new Dictionary<Guid, AggregateRoot>();
            _storage = storage;
            _snapshotStore = snapshotStore;
            _publisher = publisher;
        }

        public void Add(T aggregate)
        {
            if(!_trackedAggregates.ContainsKey(aggregate.Id))
                _trackedAggregates.Add(aggregate.Id,aggregate);
        }

        public void Commit()
        {
            foreach (var aggregate in _trackedAggregates.Values)
            {
                if(_storage.GetVersion(aggregate.Id) != aggregate.Version)
                    throw new ConcurrencyException();

                var i = 0;
                foreach (var @event in aggregate.GetUncommittedChanges())
                {
                    i++;
                    @event.Version = aggregate.Version + i;
                    _storage.Save(aggregate.Id, @event);
                    _publisher.Publish(@event);
                }

                if (ShouldMakeSnapShot(aggregate))
                    MakeSnapshot(aggregate);
                aggregate.MarkChangesAsCommitted();
            }
            _trackedAggregates.Clear();
        }

        private void MakeSnapshot(AggregateRoot aggregate) 
        {
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }

        private bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if(!IsSnapshotable(typeof(T))) return false;
            var i = aggregate.Version;

            for (var j = 0; j < aggregate.GetUncommittedChanges().Count(); j++)
                if (++i % SnapshotInterval == 0 && i != 0) return true;
            return false;
        }

        public T Get(Guid id)
        {
            if (_trackedAggregates.ContainsKey(id))
                return (T)_trackedAggregates[id];
            var aggregate = CreateAggregate();
            var snapshotVersion = RestoreAggregateFromSnapshot(id, aggregate);

            var events = _storage.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadFromHistory(events);

            if (events.Count() == 0 && snapshotVersion == -1)
            {
                throw new AggregateNotFoundException();
            }
            _trackedAggregates.Add(aggregate.Id,aggregate);
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

