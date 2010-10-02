using System;
using System.Collections.Generic;
using SimpleCQRS.Domain;
using SimpleCQRS.Eventing;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.EventingTests
{
    public class WhenSavingToEventStore
    {
        private readonly EventStore _eventstore;
        private readonly TestEventRepository _testEventRepository;
        private readonly TestEventPublisher _testEventPublisher;

        public WhenSavingToEventStore()
        {
            _testEventRepository = new TestEventRepository();
            _testEventPublisher = new TestEventPublisher();
            _eventstore = new EventStore(_testEventRepository, _testEventPublisher);
        }

        [Fact]
        public void ShouldThrowConcurrencyException()
        {
            Assert.Throws<ConcurrencyException>(()=> _eventstore.SaveEvents(Guid.NewGuid(), new List<Event>(), 0));
        }

        [Fact]
        public void ShouldSaveToEventStore()
        {
            _eventstore.SaveEvents(Guid.NewGuid(), new List<Event> { new TestAggregateDidSomething() }, 2);
            Assert.Equal(1,_testEventRepository.SavedEvents);
        }

        [Fact]
        public void ShouldPublishEvents()
        {
            _eventstore.SaveEvents(Guid.NewGuid(), new List<Event> { new TestAggregateDidSomething() }, 2);
            Assert.Equal(1, _testEventPublisher.Published);
        }

        [Fact]
        public void ShouldAddNewAggregate()
        {
            _eventstore.SaveEvents(Guid.Empty,new List<Event>{new TestAggregateDidSomething()},0);
            Assert.Equal(1,_testEventRepository.AddedEvents);
        }
        [Fact]
        public void ShouldSaveNewAggregate()
        {
            _eventstore.SaveEvents(Guid.Empty, new List<Event> { new TestAggregateDidSomething() }, 0);
            Assert.Equal(1, _testEventRepository.SavedEvents);
        }
    }
}