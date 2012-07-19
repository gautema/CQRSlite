using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;

namespace CQRSlite.Caching
{
    public class CachingRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly Dictionary<Guid, AggregateRoot> _trackedAggregates;

        public CachingRepository(IRepository repository, IEventStore eventStore)
        {
            _repository = repository;
            _eventStore = eventStore;
            _trackedAggregates = new Dictionary<Guid, AggregateRoot>();
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            _repository.Save(aggregate,expectedVersion);
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            T aggregate;
            if (IsTracked(aggregateId))
            {
                aggregate = (T)_trackedAggregates[aggregateId];
                var events = _eventStore.Get(aggregateId, aggregate.Version).Where(desc => desc.Version > aggregate.Version);
                aggregate.LoadFromHistory(events);
                
                return aggregate;
            }

            aggregate = _repository.Get<T>(aggregateId);
            _trackedAggregates.Add(aggregateId,aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }
    }
}