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

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
            var shouldMakeSnapshot = ShouldMakeSnapShot(aggregate);
            _storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
            
            if (shouldMakeSnapshot)
            {
                var snapshot = aggregate.AsDynamic().GetSnapshot();
                _snapshotStore.Save(snapshot);
            }
        }

        private bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            if(_snapshotStore == null) return false;
            if(!IsSnapshotable(typeof(T))) return false;
            return aggregate.GetUncommittedChanges().Any(x => x.Version % SnapshotInterval == 0);
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

            if (IsSnapshotable(typeof(T)) && _snapshotStore != null)
            {
                var snapshot = _snapshotStore.Get(id);
                obj.AsDynamic().Restore(snapshot);
            }
            var e = _storage.GetEventsForAggregate(id, obj.Version);
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

