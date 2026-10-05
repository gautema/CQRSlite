using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching;

public class When_evicting_cache_entry_during_operation
{
    [Fact]
    public async Task Should_not_run_operations_on_same_aggregate_concurrently()
    {
        var cache = new TestMemoryCache();
        var repository = new BlockingRepository();
        var cacheRepository = new CacheRepository(repository, new TestInMemoryEventStore(), cache);
        var id = Guid.NewGuid();

        var first = cacheRepository.Get<TestAggregate>(id);
        await repository.Entered.Task;
        await cache.Remove(id);
        var second = cacheRepository.Get<TestAggregate>(id);
        await Task.Delay(100);
        var maxConcurrent = repository.MaxConcurrent;
        repository.Release.SetResult();
        await Task.WhenAll(first, second);

        Assert.Equal(1, maxConcurrent);
    }

    private class BlockingRepository : IRepository
    {
        private int _current;
        public int MaxConcurrent { get; private set; }
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            return Task.CompletedTask;
        }

        public async Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            MaxConcurrent = Math.Max(MaxConcurrent, Interlocked.Increment(ref _current));
            Entered.TrySetResult();
            await Release.Task;
            Interlocked.Decrement(ref _current);

            var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;
            aggregate.LoadFromHistory(new IEvent[] { new TestAggregateDidSomething { Id = aggregateId, Version = 1 } });
            return aggregate;
        }
    }
}
