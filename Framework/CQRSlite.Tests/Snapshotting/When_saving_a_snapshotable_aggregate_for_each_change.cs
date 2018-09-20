using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class When_saving_a_snapshotable_aggregate_for_each_change
    {
        private readonly TestInMemorySnapshotStore _snapshotStore;
	    private readonly ISession _session;
	    private readonly TestSnapshotAggregate _aggregate;

        public When_saving_a_snapshotable_aggregate_for_each_change()
        {
            IEventStore eventStore = new TestInMemoryEventStore();
            _snapshotStore = new TestInMemorySnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy();
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            _session = new Session(repository);
            _aggregate = new TestSnapshotAggregate();

            for (var i = 0; i < 150; i++)
            {
                _session.Add(_aggregate);
                _aggregate.DoSomething();
                _session.Commit();
            }
        }

        [Fact]
        public void Should_snapshot_100th_change()
        {
            Assert.Equal(100, _snapshotStore.SavedVersion);
        }

        [Fact]
        public void Should_not_snapshot_first_event()
        {
            Assert.False(_snapshotStore.FirstSaved);
        }

        [Fact]
        public async Task Should_get_aggregate_back_correct()
        {
            Assert.Equal(150, (await _session.Get<TestSnapshotAggregate>(_aggregate.Id)).Number);
        }
    }
}