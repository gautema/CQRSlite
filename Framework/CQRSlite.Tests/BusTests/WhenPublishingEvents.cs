using CQRSlite.Bus;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.BusTests
{
	[TestFixture]
    public class WhenPublishingEvents
    {
        private InProcessBus _bus;

		[SetUp]
        public void Setup()
        {
            _bus = new InProcessBus();
        }

        [Test]
        public void ShouldPublishToAllHandlers()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            _bus.Publish(new TestAggregateDidSomething());
            Assert.AreEqual(2, handler.TimesRun);
        }

        [Test]
        public void ShouldWorkWithNoHandlers()
        {
            _bus.Publish(new TestAggregateDidSomething());
        }
    }
}