using System;
using CQRSlite.Eventing;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;

        public Repository(IEventStore storage, ISnapshotStore snapshotStore)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
        }

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
            //TODO: If version modulus snapshotInterval save snapshot.
            _storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
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
            // TODO: Check if aggregate is snapshotable
            // get snapshot for aggragate
            // aggregate needs to get state from snapshot.
            // get events after snapshot
            // apply the rest of the events
            // return obj
            if (obj is ISnapshotable && _snapshotStore != null)
            {
                //var snapshot = _snapshotStore.Get<T>(id);
                
            }
            var e = _storage.GetEventsForAggregate(id, obj.Version);
            obj.LoadsFromHistory(e);
            return obj;
        }
    }
}

