using System.Collections;
using System.Reflection;
using CQRSlite.Caching;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Caching;

public class When_evicting_cache_entry
{
    private readonly CacheRepository _rep;
    private readonly TestAggregate _aggregate;
    private readonly ICache _cache;
    private readonly IDictionary _locks;

    public When_evicting_cache_entry()
    {
        _cache = new TestMemoryCache();
        _rep = new CacheRepository(new TestRepository(), new TestEventStore(), _cache);
        _aggregate = _rep.Get<TestAggregate>(Guid.NewGuid()).Result;
        var field = _rep.GetType().GetField("_locks", BindingFlags.Static | BindingFlags.NonPublic);
        _locks = (IDictionary)field!.GetValue(_rep)!;
        _cache.Remove(_aggregate.Id);
    }

    [Fact]
    public void Should_remove_lock()
    {
        Assert.False(_locks.Contains(_aggregate.Id));
    }

    [Fact]
    public void Should_not_throw_if_no_lock()
    {
        _cache.Remove(_aggregate.Id);
        _cache.Remove(_aggregate.Id);
    }

    [Fact]
    public async Task Should_get_new_aggregate_next_get()
    {
        await _cache.Remove(_aggregate.Id);

        var aggregate = await _rep.Get<TestAggregate>(_aggregate.Id);
        Assert.NotEqual(_aggregate, aggregate);
    }
}
