using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;
using CQRSlite.Infrastructure;

namespace CQRSlite.Domain
{
    public class Session : ISession
    {
        private readonly IEventStore _storage;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IEventPublisher _publisher;
        private readonly Dictionary<Guid, AggregateRoot> _trackedAggregates;

        public Session(IEventStore storage, ISnapshotStore snapshotStore, IEventPublisher publisher)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
            _publisher = publisher;
            _trackedAggregates = new Dictionary<Guid, AggregateRoot>();
        }

        public void Track(AggregateRoot aggregate)
        {
            if (!IsTracked(aggregate.Id))
                _trackedAggregates.Add(aggregate.Id, aggregate);
            else if (_trackedAggregates[aggregate.Id] != aggregate)
                throw new TrackedAggregateAddedException();
        }

        public T Get<T>(Guid id) where T : AggregateRoot
        {
            return (T)_trackedAggregates[id];
        }

        public void Commit()
        {
            foreach (var aggregate in _trackedAggregates.Values)
            {
                if (_storage.GetVersion(aggregate.Id) != aggregate.Version)
                    throw new ConcurrencyException();

                var i = 0;
                foreach (var @event in aggregate.GetUncommittedChanges())
                {
                    i++;
                    @event.Version = aggregate.Version + i;
                    _storage.Save(aggregate.Id, @event);
                    _publisher.Publish(@event);
                }

                if (SnapshotHelper.ShouldMakeSnapShot(aggregate))
                    MakeSnapshot(aggregate);
                aggregate.MarkChangesAsCommitted();
            }
            _trackedAggregates.Clear();
        }

        public bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }
        
        private void MakeSnapshot(AggregateRoot aggregate)
        {
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }
    }
}
