using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain;

public class When_committing_session
{
    private readonly TypeRecordingRepository _repository;
    private readonly Session _session;

    public When_committing_session()
    {
        _repository = new TypeRecordingRepository(new Repository(new TestInMemoryEventStore()));
        _session = new Session(_repository);
    }

    [Fact]
    public async Task Should_save_added_aggregate_with_its_own_type()
    {
        await _session.Add(new TestAggregate(Guid.NewGuid()));
        await _session.Commit();

        Assert.Equal(typeof(TestAggregate), Assert.Single(_repository.SavedTypes));
    }

    [Fact]
    public async Task Should_save_loaded_aggregate_with_its_own_type()
    {
        var id = Guid.NewGuid();
        await _session.Add(new TestAggregate(id));
        await _session.Commit();
        _repository.SavedTypes.Clear();

        var aggregate = await _session.Get<TestAggregate>(id);
        aggregate.DoSomething();
        await _session.Commit();

        Assert.Equal(typeof(TestAggregate), Assert.Single(_repository.SavedTypes));
    }

    private class TypeRecordingRepository(IRepository repository) : IRepository
    {
        public List<Type> SavedTypes { get; } = new();

        public Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            SavedTypes.Add(typeof(T));
            return repository.Save(aggregate, expectedVersion, cancellationToken);
        }

        public Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            return repository.Get<T>(aggregateId, cancellationToken);
        }
    }
}
