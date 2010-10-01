using System.Linq;
using SimpleCQRS.Domain;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.RepositoryTests
{
    public class WhenSaving
    {
        private TestEventStore _eventStore;
        private TestAggregate _aggregate;

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
