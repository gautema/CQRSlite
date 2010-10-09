using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.EventingTests
{
    public class WhenGettingFromEventStore
    {
        private EventStore _eventstore;
        private IEnumerable<Event> _events;

        public WhenGettingFromEventStore()
        {
            var testEventRepository = new TestEventRepository();
            var testEventPublisher = new TestEventPublisher();
            _eventstore = new EventStore(testEventRepository,testEventPublisher);
            _events = _eventstore.GetEventsForAggregate(Guid.NewGuid(), 0);
        }

        [Fact]
        public void ShouldGetAllEvents()
        {
            Assert.Equal(2, _events.Count());
        }

        [Fact]
        public void ShouldFailIfAggregateNotExists()
        {
            Assert.Throws<AggregateNotFoundException>(() => { _eventstore.GetEventsForAggregate(Guid.Empty, 0); });
        }
    }
}