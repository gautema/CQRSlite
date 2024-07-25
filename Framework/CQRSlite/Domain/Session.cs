﻿using CQRSlite.Domain.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Domain
{
    /// <summary>
    /// Implementation of unit of work for aggregates.
    /// </summary>
    public class Session : ISession
    {
        private readonly IRepository _repository;
        private readonly Dictionary<Guid, AggregateDescriptor> _trackedAggregates;

        /// <summary>
        /// Initialize Session
        /// </summary>
        /// <param name="repository"></param>
        public Session(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _trackedAggregates = new Dictionary<Guid, AggregateDescriptor>();
        }

        public Task Add<T>(T aggregate, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            if (!IsTracked(aggregate.Id))
            {
                _trackedAggregates.Add(aggregate.Id, new AggregateDescriptor { Aggregate = aggregate, Version = aggregate.Version });
            }
            else if (_trackedAggregates[aggregate.Id].Aggregate != aggregate)
            {
                throw new ConcurrencyException(aggregate.Id);
            }
            return Task.FromResult(0);
        }

        public async Task<T> Get<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
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

            var aggregate = await _repository.Get<T>(id, cancellationToken).ConfigureAwait(false);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
            {
                throw new ConcurrencyException(id);
            }
            await Add(aggregate, cancellationToken).ConfigureAwait(false);

            return aggregate;
        }

        private bool IsTracked(Guid id) => _trackedAggregates.ContainsKey(id);

        public async Task Commit(CancellationToken cancellationToken = default)
        {
            var aggregates = _trackedAggregates.Values.ToArray();
            _trackedAggregates.Clear();

            foreach (var descriptor in aggregates)
            {
                await _repository.Save(descriptor.Aggregate, descriptor.Version, cancellationToken).ConfigureAwait(false);
            }
        }

        private class AggregateDescriptor
        {
            public AggregateRoot Aggregate { get; set; }
            public int Version { get; set; }
        }
    }
}
