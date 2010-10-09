using System;
using CQRSlite.Bus;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.BusTests
{
    public class WhenSendingCommand
    {
        private readonly InProcessBus _bus;

        public WhenSendingCommand()
        {
            _bus = new InProcessBus();
        }

        [Fact]
        public void ShouldRunHandler()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _bus.Send(new TestAggregateDoSomething());

            Assert.Equal(1,handler.TimesRun);
        }

        [Fact]
        public void ShouldThrowIfMoreHandlers()
        {
            var x = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);

            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }

        [Fact]
        public void ShouldThrowIfNoHandles()
        {
            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }
    }
}