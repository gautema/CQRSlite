using System;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_adding_aggregates_to_repository
    {
        private Session _session;

        public When_adding_aggregates_to_repository()
        {
            var eventStore = new TestInMemoryEventStore();
            _session = new Session(new Repository(eventStore));
        }

        [Fact]
        public void Should_throw_if_different_object_with_tracked_guid_is_added()
        {
            var aggregate = new TestAggregate(Guid.NewGuid());
            var aggregate2 = new TestAggregate(aggregate.Id);
            _session.Add(aggregate);
            Assert.Throws<ConcurrencyException>(() => _session.Add(aggregate2));
        }

        [Fact]
        public void Should_not_throw_if_object_already_tracked()
        {
            var aggregate = new TestAggregate(Guid.NewGuid());
            _session.Add(aggregate);
            _session.Add(aggregate);
        }
    }
}