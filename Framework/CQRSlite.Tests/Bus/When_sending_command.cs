using System;
using CQRSlite.Bus;
using CQRSlite.Tests.Substitutes;
using Xunit;
using CQRSlite.Commands;

namespace CQRSlite.Tests.Bus
{
    public class When_sending_command
    {
        private InProcessBus _bus;

        public When_sending_command()
        {
            _bus = new InProcessBus();
        }

        [Fact]
        public void Should_run_handler()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _bus.Send(new TestAggregateDoSomething());

            Assert.Equal(1,handler.TimesRun);
        }

        [Fact]
        public void Should_throw_if_more_handlers()
        {
            var x = new TestAggregateDoSomethingHandler();
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);
            _bus.RegisterHandler<TestAggregateDoSomething>(x.Handle);

            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }

        [Fact]
        public void Should_throw_if_no_handlers()
        {
            Assert.Throws<InvalidOperationException>(() => _bus.Send(new TestAggregateDoSomething()));
        }

        [Fact]
        public void Should_handle_dynamically_generated_commands()
        {
            var handler = new TestAggregateDoSomethingHandler();
            var command = (ICommand)Activator.CreateInstance(typeof(TestAggregateDoSomething));

            _bus.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _bus.Send(command);

            Assert.Equal(1, handler.TimesRun);
        }
    }
}