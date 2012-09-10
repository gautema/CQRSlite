using System;
using CQRSlite.Bus;
using CQRSlite.Tests.Substitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.Bus
{
	[TestFixture]
    public class When_sending_command
    {
        private InProcessBus _bus;

		[SetUp]
        public void Setup()
        {
            _bus = new InProcessBus();
        }

        [Test]
        public void Should_run_handler()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _bus.Send(new TestAggregateDoSomething());

            Assert.AreEqual(1,handler.TimesRun);
        }

        [Test]
        public void Should_throw_if_more_handlers()
        {
            var x = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);

            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }

        [Test]
        public void Should_throw_if_no_handlers()
        {
            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }
    }
}