using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching;

public class When_getting_aggregate_from_concurrent_sessions
{
    private readonly TestInMemoryEventStore _eventStore;
    private readonly ICache _cache;
    private readonly Guid _id;

    public When_getting_aggregate_from_concurrent_sessions()
    {
        _eventStore = new TestInMemoryEventStore();
        _cache = new MemoryCache();
        _id = Guid.NewGuid();

        var session = NewSession();
        session.Add(new TestAggregate(_id)).Wait();
        session.Commit().Wait();
    }

    [Fact]
    public async Task Should_not_share_aggregate_instance()
    {
        var first = await NewSession().Get<TestAggregate>(_id);
        var second = await NewSession().Get<TestAggregate>(_id);

        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task Should_not_save_other_sessions_changes()
    {
        var first = NewSession();
        var second = NewSession();
        (await first.Get<TestAggregate>(_id)).DoSomething();
        (await second.Get<TestAggregate>(_id)).DoSomethingElse();

        await first.Commit();
        await Assert.ThrowsAsync<CQRSlite.Domain.Exception.ConcurrencyException>(() => second.Commit());

        Assert.Contains(_eventStore.Events, e => e is TestAggregateDidSomething);
        Assert.DoesNotContain(_eventStore.Events, e => e is TestAggregateDidSomethingElse);
    }

    [Fact]
    public async Task Should_reuse_saved_instance_in_next_session()
    {
        var session = NewSession();
        var aggregate = await session.Get<TestAggregate>(_id);
        aggregate.DoSomething();
        await session.Commit();

        Assert.Same(aggregate, await NewSession().Get<TestAggregate>(_id));
    }

    private Session NewSession() => new(new CacheRepository(new Repository(_eventStore), _eventStore, _cache));
}
