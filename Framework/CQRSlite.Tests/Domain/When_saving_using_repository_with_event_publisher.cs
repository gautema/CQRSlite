using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Domain
{
    [TestFixture]
    public class When_saving_using_repository_with_event_publisher
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregateNoParameterLessConstructor _aggregate;
        private TestEventPublisher _eventPublisher;
        private ISession _session;
        private Repository _rep;

        [SetUp]
        public void Setup()
        {
            _eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
#pragma warning disable 618
            _rep = new Repository(_eventStore, _eventPublisher);
#pragma warning restore 618
            _session = new Session(_rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Test]
        public void Should_publish_events()
        {
            _aggregate.DoSomething();
            _session.Add(_aggregate);
            _session.Commit();
            Assert.AreEqual(1, _eventPublisher.Published);
        }
    }
}