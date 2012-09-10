using CQRSlite.Domain.Exception;
using CQRSlite.Infrastructure.Repositories.Domain;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Domain
{
    [TestFixture]
    public class When_adding_aggregates_to_repository
    {
        private Session _session;

        [SetUp]
        public void SetUp()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _session = new Session(new Repository(eventStore, eventPublisher));
        }

        [Test]
        public void Should_throw_if_different_object_with_tracked_guid_is_added()
        {
            var aggregate = new TestSnapshotAggregate();
            var aggregate2 = new TestSnapshotAggregate();
            aggregate2.SetId(aggregate.Id);
            _session.Add(aggregate);
            Assert.Throws<ConcurrencyException>(() => _session.Add(aggregate2));
        }

        [Test]
        public void Should_not_throw_if_object_already_tracked()
        {
            var aggregate = new TestSnapshotAggregate();
            _session.Add(aggregate);
            _session.Add(aggregate);
        }
    }
}