using System;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;
        private const int SnapshotInterval = 15;

        public Repository(IEventStore storage, ISnapshotStore snapshotStore)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
        }

        public void Save(T aggregate, int expectedVersion)
        {
            var shouldMakeSnapshot = ShouldMakeSnapShot(aggregate,expectedVersion);
            var version = _storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
            
            if (shouldMakeSnapshot)
            {
                var snapshot = aggregate.AsDynamic().GetSnapshot();
                snapshot.RealObject.Version = version;
                _snapshotStore.Save(snapshot.RealObject);
            }
        }

        private bool ShouldMakeSnapShot(AggregateRoot aggregate, int expectedVersion)
        {
            if(!IsSnapshotable(typeof(T))) return false;
            var i = expectedVersion;

            for (var j = 0; j < aggregate.GetUncommittedChanges().Count(); j++)
                if (++i%SnapshotInterval == 0 && i != 0) return true;
            return false;
        }

        public T GetById(Guid id)
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

            var snapshotversion = -1;
            if (IsSnapshotable(typeof(T)) && _snapshotStore != null)
            {
                var snapshot = _snapshotStore.Get(id);
                if(snapshot != null)
                {
                    obj.AsDynamic().Restore(snapshot);
                    snapshotversion = snapshot.Version;
                }
            }
            var e = _storage.GetEventsForAggregate(id, snapshotversion);
            obj.LoadsFromHistory(e);
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

