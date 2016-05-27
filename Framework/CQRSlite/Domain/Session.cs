using CQRSlite.Domain.Exception;
using System;
using System.Collections.Generic;

namespace CQRSlite.Domain
{
    public class Session : ISession
    {
        private readonly IRepository _repository;
        private readonly Dictionary<Guid, AggregateDescriptor> _trackedAggregates;

        public Session(IRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
            _trackedAggregates = new Dictionary<Guid, AggregateDescriptor>();
        }

        public void Add<T>(T aggregate) where T : AggregateRoot
        {
            if (!IsTracked(aggregate.Id))
            {
                _trackedAggregates.Add(aggregate.Id, new AggregateDescriptor { Aggregate = aggregate, Version = aggregate.Version });
            }
            else if (_trackedAggregates[aggregate.Id].Aggregate != aggregate)
            {
                throw new ConcurrencyException(aggregate.Id);
            }
        }

        public T Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot
        {
            if (IsTracked(id))
            {
                var trackedAggregate = (T)_trackedAggregates[id].Aggregate;
                if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
                {
                    throw new ConcurrencyException(trackedAggregate.Id);
                }
                return trackedAggregate;
            }

            var aggregate = _repository.Get<T>(id);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
            {
                throw new ConcurrencyException(id);
            }
            Add(aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }

        public void Commit()
        {
            foreach (var descriptor in _trackedAggregates.Values)
            {
                _repository.Save(descriptor.Aggregate, descriptor.Version);
            }
            _trackedAggregates.Clear();
        }
    }
}
