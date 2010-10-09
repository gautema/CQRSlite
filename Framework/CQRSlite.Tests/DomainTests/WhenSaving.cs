using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.DomainTests
{
    public class WhenSaving
    {
        private readonly TestEventStore _eventStore;
        private readonly TestAggregate _aggregate;

        public WhenSaving()
        {
            _eventStore = new TestEventStore();
            var rep = new Repository<TestAggregateNoParameterLessConstructor>(_eventStore);
            _aggregate = new TestAggregate();
            _aggregate.DoSomething();
            rep.Save(_aggregate, 1);
        }

        [Fact]
        public void ShouldSaveUncommitedChanges()
        {

            Assert.Equal(1, _eventStore.SavedEvents);
        }

        [Fact]
        public void ShouldMarkCommitedAfterSave()
        {
            Assert.Equal(0, _aggregate.GetUncommittedChanges().Count());
        }
    }
}
