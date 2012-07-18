using System;
using System.Collections.Generic;

namespace CQRSlite.Domain
{
    public class Session : ISession
    {
        private readonly IAggregateStore _storage;
        private readonly Dictionary<Guid, AggregateRoot> _trackedAggregates;

        public Session(IAggregateStore storage)
        {
            _storage = storage;
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
                if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
                    throw new ConcurrencyException();
                return trackedAggregate;
            }
            if (expectedVersion != null && _storage.GetVersion(id) != expectedVersion && expectedVersion != -1)
                throw new ConcurrencyException();

            var aggregate = _storage.Get<T>(id);
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
                _storage.Save(aggregate);
            }
        }

    }
}
