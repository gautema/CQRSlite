using System;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;

namespace CQRSlite.Snapshotting
{
    public class SnapshotRepository : IRepository
    {
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;

        public SnapshotRepository(ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy, IRepository repository, IEventStore eventStore)
        {
            _snapshotStore = snapshotStore;
            _snapshotStrategy = snapshotStrategy;
            _repository = repository;
            _eventStore = eventStore;
        }

        public void Save<T>(T aggregate) where T : AggregateRoot
        {
            TryMakeSnapshot(aggregate);
            _repository.Save(aggregate);
        }

        public T Get<T>(Guid aggregateId, int? exectedVersion = null) where T : AggregateRoot
        {
            var aggregate = AggregateActivator.CreateAggregate<T>();
            var snapshotVersion = TryRestoreAggregateFromSnapshot(aggregateId, aggregate);

            return aggregate;
        }

        public int GetVersion(Guid aggregateId)
        {
            return _repository.GetVersion(aggregateId);
        }

        private int TryRestoreAggregateFromSnapshot<T>(Guid id, T aggregate)
        {
            var version = -1;
            if (_snapshotStrategy.IsSnapshotable(typeof(T)))
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

        private void TryMakeSnapshot(AggregateRoot aggregate)
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate))
                return;
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }
    }
}