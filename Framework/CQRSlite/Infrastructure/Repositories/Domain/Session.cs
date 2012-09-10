using System;
using System.Collections.Generic;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;

namespace CQRSlite.Infrastructure.Repositories.Domain
{
    public class Session : ISession
    {
        private readonly IRepository _storage;
        private readonly Dictionary<Guid, AggregateRoot> _trackedAggregates;

        public Session(IRepository storage)
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

            var aggregate = _storage.Get<T>(id);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
                throw new ConcurrencyException();
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
                _storage.Save(aggregate, aggregate.Version);
            }
        }
    }
}
