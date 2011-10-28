using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        public void Add<T>(T aggregate) where T : AggregateRoot
        {
            if (!IsTracked(aggregate.Id))
                _trackedAggregates.Add(aggregate.Id, aggregate);
            else if (_trackedAggregates[aggregate.Id] != aggregate)
                throw new ConcurrencyException();
        }

        public T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot
        {
            if(IsTracked(id))
            {
                var trackedAggregate = (T)_trackedAggregates[id];
                if (trackedAggregate.Version != expectedVersion && expectedVersion != null)
                    throw new ConcurrencyException();
                return trackedAggregate;
            }
            if (expectedVersion != null && _storage.GetVersion(id) != expectedVersion && expectedVersion != -1)
                throw new ConcurrencyException();

            var aggregate = LoadAggregate<T>(id);
            Add(aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }

        public void Commit()
        {
            foreach (var aggregate in _trackedAggregates.Values)
            {
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
        
        private void TryMakeSnapshot(AggregateRoot aggregate)
        {
            if (!_snapshotStrategy.ShouldMakeSnapShot(aggregate))
                return;
            var snapshot = aggregate.AsDynamic().GetSnapshot().RealObject;
            snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count();
            _snapshotStore.Save(snapshot);
        }

        private T LoadAggregate<T>(Guid id) where T : AggregateRoot
        {
            var aggregate = CreateAggregate<T>();
            var snapshotVersion = TryRestoreAggregateFromSnapshot(id, aggregate);

            var events = _storage.Get(id, snapshotVersion).Where(desc => desc.Version > snapshotVersion);
            if (events.Count() == 0 && snapshotVersion == -1)
                throw new AggregateNotFoundException();

            aggregate.LoadFromHistory(events);
            return aggregate;
        }

        private T CreateAggregate<T>()
        {
            T obj;
            try
            {
                obj = (T)Activator.CreateInstance(typeof(T), true);
            }
            catch (MissingMethodException)
            {
                obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
            }
            return obj;
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
    }
}
