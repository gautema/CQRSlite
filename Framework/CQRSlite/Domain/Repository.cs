using System;
using CQRSlite.Eventing;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore<T> _snapshotStore;

        public Repository(IEventStore storage, ISnapshotStore<T> snapshotStore)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
        }

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
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
            if (_snapshotStore != null)
                _snapshotStore.Get(id);
            var e = _storage.GetEventsForAggregate(id);
            obj.LoadsFromHistory(e);
            return obj;
        }
    }
}

