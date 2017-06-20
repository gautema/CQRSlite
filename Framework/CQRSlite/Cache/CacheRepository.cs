﻿using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Cache
{
    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private readonly ICache _cache;
        private static readonly ConcurrentDictionary<IIdentity, SemaphoreSlim> _locks =
            new ConcurrentDictionary<IIdentity, SemaphoreSlim>();

        private static SemaphoreSlim CreateLock(IIdentity _) => new SemaphoreSlim(1, 1);

        public CacheRepository(IRepository repository, IEventStore eventStore, ICache cache)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _cache.RegisterEvictionCallback(key => _locks.TryRemove(key, out var o));
        }

        public async Task Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var @lock = _locks.GetOrAdd(aggregate.Id, CreateLock);
            await @lock.WaitAsync();
            try
            {
                if (aggregate.Id.IsValid && !_cache.IsTracked(aggregate.Id))
                {
                    _cache.Set(aggregate.Id, aggregate);
                }
                await _repository.Save(aggregate, expectedVersion);
            }
            catch (Exception)
            {
                _cache.Remove(aggregate.Id);
                throw;
            }
            finally
            {
                @lock.Release();
            }
        }

        public async Task<T> Get<T>(IIdentity aggregateId) where T : AggregateRoot
        {
            var @lock = _locks.GetOrAdd(aggregateId, CreateLock);
            await @lock.WaitAsync();
            try
            {
                T aggregate;
                if (_cache.IsTracked(aggregateId))
                {
                    aggregate = (T)_cache.Get(aggregateId);
                    var events = await _eventStore.Get(aggregateId, aggregate.Version);
                    if (events.Any() && events.First().Version != aggregate.Version + 1)
                    {
                        _cache.Remove(aggregateId);
                    }
                    else
                    {
                        aggregate.LoadFromHistory(events);
                        return aggregate;
                    }
                }

                aggregate = await _repository.Get<T>(aggregateId);
                _cache.Set(aggregateId, aggregate);
                return aggregate;
            }
            catch (Exception)
            {
                _cache.Remove(aggregateId);
                throw;
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}