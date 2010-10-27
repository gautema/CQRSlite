using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenSaving
    {
        private readonly TestEventStore _eventStore;
        private readonly TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
        private Repository<TestAggregateNoParameterLessConstructor> _rep;

        public WhenSaving()
        {
            _eventStore = new TestEventStore();
            _eventPublisher = new TestEventPublisher();
            var snapshotstore = new NullSnapshotStore();
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(_eventStore, snapshotstore, _eventPublisher);
            _aggregate = new TestAggregateNoParameterLessConstructor(2);

        }

        [Fact]
        public void ShouldSaveUncommitedChanges()
        {
            _aggregate.DoSomething();
            _rep.Save(_aggregate, 0);
            Assert.Equal(1, _eventStore.SavedEvents);
        }

        [Fact]
        public void ShouldMarkCommitedAfterSave()
        {
            _aggregate.DoSomething();
            _rep.Save(_aggregate, 0);
            Assert.Equal(0, _aggregate.GetUncommittedChanges().Count());
        }

        [Fact]
        public void ShouldThrowConcurrencyException()
        {
            Assert.Throws<ConcurrencyException>(() =>  _rep.Save(_aggregate, 1));
        }
        
        [Fact]
        public void ShouldPublishEvents()
        {
            _aggregate.DoSomething();
            _rep.Save(_aggregate, 0);
            Assert.Equal(1, _eventPublisher.Published);
        }

        [Fact]
        public void ShouldAddNewAggregate()
        {
            var agg = new TestAggregateNoParameterLessConstructor(1,Guid.Empty);
            agg.DoSomething();
            _rep.Save(agg,0);
            Assert.Equal(1, _eventStore.SavedEvents);
        }
    }
}
