﻿using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using System.Threading.Tasks;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_saving_using_repository_with_event_publisher_async
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
        private ISession _session;
        private Repository _rep;

        public When_saving_using_repository_with_event_publisher_async()
        {
            _eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
#pragma warning disable 618
            _rep = new Repository(_eventStore, _eventPublisher);
#pragma warning restore 618
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Fact]
        public async Task Should_publish_events()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            await _session.CommitAsync();
            Assert.Equal(1, _eventPublisher.Published);
        }
    }
}
