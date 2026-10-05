using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching;

public class When_getting_aggregate_with_abandoned_changes
{
    private readonly TestInMemoryEventStore _eventStore;
    private readonly ICache _cache;
    private readonly Guid _id;

    public When_getting_aggregate_with_abandoned_changes()
    {
        _eventStore = new TestInMemoryEventStore();
        _cache = new MemoryCache();
        _id = Guid.NewGuid();

        var session = NewSession();
        session.Add(new TestAggregate(_id)).Wait();
        session.Commit().Wait();

        // A command changes the aggregate, then fails before committing
        NewSession().Get<TestAggregate>(_id).Result.DoSomething();
    }

    [Fact]
    public async Task Should_not_save_abandoned_changes_with_next_commit()
    {
        var session = NewSession();
        (await session.Get<TestAggregate>(_id)).DoSomethingElse();
        await session.Commit();

        Assert.DoesNotContain(_eventStore.Events, e => e is TestAggregateDidSomething);
        Assert.Contains(_eventStore.Events, e => e is TestAggregateDidSomethingElse);
    }

    [Fact]
    public async Task Should_get_aggregate_without_abandoned_changes()
    {
        var aggregate = await NewSession().Get<TestAggregate>(_id);

        Assert.Empty(aggregate.GetUncommittedChanges());
        Assert.Equal(0, aggregate.DidSomethingCount);
    }

    private Session NewSession() => new(new CacheRepository(new Repository(_eventStore), _eventStore, _cache));
}
