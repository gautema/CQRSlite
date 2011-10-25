using System;
using System.Linq;
using System.Runtime.Serialization;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;

namespace CQRSlite.Domain
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISession _session;

        public Repository(ISession session, IEventStore storage, ISnapshotStore snapshotStore)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
            _session = session;
        }

        public void Add(T aggregate)
        {
            _session.Track(aggregate);
        }

        public T Get(Guid id)
        {
            if (_session.IsTracked(id))
                return _session.Get<T>(id);

            var aggregate = CreateAggregate();
            var snapshotVersion = RestoreAggregateFromSnapshot(id, aggregate);

            var events = _storage.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            aggregate.LoadFromHistory(events);

            if (events.Count() == 0 && snapshotVersion == -1)
            {
                throw new AggregateNotFoundException();
            }
            _session.Track(aggregate);
            return aggregate;
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
               obj = (T) FormatterServices.GetUninitializedObject(typeof (T));
            }
            return obj;
        }

        private int RestoreAggregateFromSnapshot(Guid id, T aggregate)
        {
            var version = -1;
            if (SnapshotHelper.IsSnapshotable(typeof(T)) && _snapshotStore != null)
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
    }
}

