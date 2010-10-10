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
            if(ShouldMakeSnapShot(aggregate))
            {
                // var snapshot = aggregate.GetSnapshot();
                //_snapshotStore.Save();
            }
            _storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
            aggregate.MarkChangesAsCommitted();
        }

        private bool ShouldMakeSnapShot(AggregateRoot aggregate)
        {
            return IsSNapshotable(aggregate) && aggregate.GetUncommittedChanges().Any(x => x.Version % SnapshotInterval == 0);
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

            if (IsSNapshotable(obj) && _snapshotStore != null)
            {
                //var snapshot = _snapshotStore.Get(type, id);
                // obj.RestoreFromSnapshot(snapshot);
            }
            var e = _storage.GetEventsForAggregate(id, obj.Version);
            obj.LoadsFromHistory(e);
            return obj;
        }

        private bool IsSNapshotable(AggregateRoot obj)
        {
            throw new NotImplementedException();
        }
    }
}

