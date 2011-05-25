using System;
using CQRSlite.Bus;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.BusTests
{
	[TestFixture]
    public class WhenSendingCommand
    {
        private InProcessBus _bus;

		[SetUp]
        public void Setup()
        {
            _bus = new InProcessBus();
        }

        [Test]
        public void ShouldRunHandler()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _bus.Send(new TestAggregateDoSomething());

            Assert.AreEqual(1,handler.TimesRun);
        }

        [Test]
        public void ShouldThrowIfMoreHandlers()
        {
            var x = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);

            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }

        [Test]
        public void ShouldThrowIfNoHandles()
        {
            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }
    }
}