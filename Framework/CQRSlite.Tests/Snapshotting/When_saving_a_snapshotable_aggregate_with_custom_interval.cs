using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Snapshotting;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class When_saving_a_snapshotable_aggregate_with_custom_interval
    {
        private readonly TestInMemorySnapshotStore _snapshotStore;
	    private readonly ISession _session;
	    private readonly TestSnapshotAggregate _aggregate;

        public When_saving_a_snapshotable_aggregate_with_custom_interval()
        {
            IEventStore eventStore = new TestInMemoryEventStore();
            _snapshotStore = new TestInMemorySnapshotStore();
            var snapshotStrategy = new DefaultSnapshotStrategy(23);
            var repository = new SnapshotRepository(_snapshotStore, snapshotStrategy, new Repository(eventStore), eventStore);
            _session = new Session(repository);
            _aggregate = new TestSnapshotAggregate(Guid.NewGuid());
            
            Task.Run(async () =>
            {
                for (var i = 0; i < 50; i++)
                {
                    await _session.Add(_aggregate);
                    _aggregate.DoSomething();
                    await _session.Commit();
                }
            }).Wait();
        }

        [Fact]
        public void Should_snapshot_46th_change()
        {
            Assert.Equal(46, _snapshotStore.SavedVersion);
        }

        [Fact]
        public void Should_not_snapshot_first_event()
        {
            Assert.False(_snapshotStore.FirstSaved);
        }

        [Fact]
        public async Task Should_get_aggregate_back_correct()
        {
            Assert.Equal(50, (await _session.Get<TestSnapshotAggregate>(_aggregate.Id)).Number);
        }
    }
}
