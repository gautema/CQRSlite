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
        private readonly ISnapshotStrategy _snapshotStrategy;
        private readonly Dictionary<Guid, AggregateRoot> _trackedAggregates;

        public Session(IEventStore storage, ISnapshotStore snapshotStore, IEventPublisher publisher, ISnapshotStrategy snapshotStrategy)
        {
            _storage = storage;
            _snapshotStore = snapshotStore;
            _publisher = publisher;
            _snapshotStrategy = snapshotStrategy;
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

                SaveAndPublishUncommitedEvents(aggregate);
                TryMakeSnapshot(aggregate);
                aggregate.MarkChangesAsCommitted();
            }
        }

        private void SaveAndPublishUncommitedEvents(AggregateRoot aggregate)
        {
            var i = 0;
            foreach (var @event in aggregate.GetUncommittedChanges())
            {
                i++;
                @event.Version = aggregate.Version + i;
                @event.TimeStamp = DateTimeOffset.UtcNow;
                _storage.Save(aggregate.Id, @event);
                _publisher.Publish(@event);
            }
        }

        public bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
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
