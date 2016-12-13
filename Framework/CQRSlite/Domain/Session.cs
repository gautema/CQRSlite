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
                return HandleTracked<T>(id, expectedVersion);
            }

            var aggregate = _repository.Get<T>(id);
            AddAggregate(id, expectedVersion, aggregate);

            return aggregate;
        }

        public async Task<T> GetAsync<T>(Guid id, int? expectedVersion = default(int?)) where T : AggregateRoot
        {
            if (IsTracked(id))
            {
                return HandleTracked<T>(id, expectedVersion);
            }

            var aggregate = await _repository.GetAsync<T>(id);
            AddAggregate(id, expectedVersion, aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return _trackedAggregates.ContainsKey(id);
        }

        private T HandleTracked<T>(Guid id, int? expectedVersion) where T : AggregateRoot
        {
            var trackedAggregate = (T)_trackedAggregates[id].Aggregate;
            if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
            {
                throw new ConcurrencyException(trackedAggregate.Id);
            }
            return trackedAggregate;
        }

        private void AddAggregate<T>(Guid id, int? expectedVersion, T aggregate) where T : AggregateRoot
        {
            if (expectedVersion != null && aggregate.Version != expectedVersion)
            {
                throw new ConcurrencyException(id);
            }
            Add(aggregate);
        }

        public void Commit()
        {
            foreach (var descriptor in _trackedAggregates.Values)
            {
                _repository.Save(descriptor.Aggregate, descriptor.Version);
            }
            _trackedAggregates.Clear();
        }

        public async Task CommitAsync()
        {
            var saveTasks = _trackedAggregates.Values.Select(obj => _repository.SaveAsync(obj.Aggregate, obj.Version));
            await Task.WhenAll(saveTasks);
            _trackedAggregates.Clear();
        }
    }
}
