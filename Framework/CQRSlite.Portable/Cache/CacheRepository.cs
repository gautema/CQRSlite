using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CQRSlite.Cache
{
    using Provision.Providers.PortableMemoryCache.Mono;

    public class CacheRepository : IRepository
    {
        private readonly IRepository _repository;
        private readonly IEventStore _eventStore;
        private static readonly Provision.Providers.PortableMemoryCache.PortableMemoryCacheHandler _cache;
        private static readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        static CacheRepository()
        {
              var config=new Provision.Providers.PortableMemoryCache.PortableMemoryCacheHandlerConfiguration(TimeSpan.FromMinutes(15));
            _cache = new Provision.Providers.PortableMemoryCache.PortableMemoryCacheHandler(config);
      }

        public CacheRepository(IRepository repository, IEventStore eventStore)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
            if (eventStore == null)
            {
                throw new ArgumentNullException(nameof(eventStore));
            }

            _repository = repository;
            _eventStore = eventStore;
            //_policyFactory = () => new CacheItemPolicy
            //{
            //    SlidingExpiration = TimeSpan.FromMinutes(15),
            //    RemovedCallback = x =>
            //    {
            //        object o;
            //        _locks.TryRemove(x.CacheItem.Key, out o);
            //    }
            //};
        }

        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            var idstring = aggregate.Id.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    if (aggregate.Id != Guid.Empty && !IsTracked(aggregate.Id))
                    {
                        _cache.AddOrUpdate(idstring, aggregate);
                    }
                    _repository.Save(aggregate, expectedVersion);
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.RemoveByKey(idstring);
                    object o;
                    _locks.TryRemove(idstring, out o);
                }
                throw;
            }
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var idstring = aggregateId.ToString();
            try
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    T aggregate;
                    if (IsTracked(aggregateId))
                    {
                        aggregate = _cache.GetValue<T>(idstring).Result;
                        var events = _eventStore.Get<T>(aggregateId, aggregate.Version);
                        if (events.Any() && events.First().Version != aggregate.Version + 1)
                        {
                            _cache.RemoveByKey(idstring);
                            object o;
                            _locks.TryRemove(idstring, out o);
                        }
                        else
                        {
                            aggregate.LoadFromHistory(events);
                            return aggregate;
                        }
                    }

                    aggregate = _repository.Get<T>(aggregateId);
                    _cache.AddOrUpdate(aggregateId.ToString(), aggregate);
                    return aggregate;
                }
            }
            catch (Exception)
            {
                lock (_locks.GetOrAdd(idstring, _ => new object()))
                {
                    _cache.RemoveByKey(idstring);
                    object o;
                    _locks.TryRemove(idstring, out o);
                }
                throw;
            }
        }

        private bool IsTracked(Guid id)
        {
            return _cache.Contains(id.ToString()).Result;
        }
    }
}