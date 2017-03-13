using CQRSlite.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    public class Session : ISession
    {
        private readonly IRepository _repository;
        private readonly Dictionary<Guid, AggregateDescriptor> _trackedAggregates;

        public Session(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _trackedAggregates = new Dictionary<Guid, AggregateDescriptor>();
        }

        public Task Add<T>(T aggregate) where T : AggregateRoot
        {
            if (!IsTracked(aggregate.Id))
            {
                _trackedAggregates.Add(aggregate.Id, new AggregateDescriptor { Aggregate = aggregate, Version = aggregate.Version });
            }
            else if (_trackedAggregates[aggregate.Id].Aggregate != aggregate)
            {
                throw new ConcurrencyException(aggregate.Id);
            }
            return Task.CompletedTask;
        }

        public async Task<T> Get<T>(Guid id, int? expectedVersion = null) where T : AggregateRoot
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

            var aggregate = await _repository.Get<T>(id);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
            {
                throw new ConcurrencyException(id);
            }
            await Add(aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }

        public async Task Commit()
        {
            await Task.WhenAll(_trackedAggregates.Values.Select(x => _repository.Save(x.Aggregate, x.Version)));
            _trackedAggregates.Clear();
        }
    }
}
