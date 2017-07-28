using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Tests.Substitutes;
using Xunit;
using CQRSlite.Commands;
using CQRSlite.Routing;
using CQRSlite.Domain.Exception;

namespace CQRSlite.Tests.Routing
{
    public class When_sending_command
    {
        private Router _router;

        public When_sending_command()
        {
            _router = new Router();
        }

        [Fact]
        public async Task Should_run_handler()
        {
            var handler = new TestAggregateDoSomethingElseHandler();
            _router.RegisterHandler<TestAggregateDoSomethingElse>((x, token) => handler.Handle(x));
            await _router.Send(new TestAggregateDoSomethingElse());

            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_run_cancellation_handler()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            await _router.Send(new TestAggregateDoSomething());

            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_throw_if_more_handlers()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _router.Send(new TestAggregateDoSomething()));
        }

        [Fact]
        public async Task Should_throw_if_no_handlers()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _router.Send(new TestAggregateDoSomething()));
        }

        [Fact]
        public async Task Should_handle_dynamically_generated_commands()
        {
            var handler = new TestAggregateDoSomethingHandler();
            var command = (ICommand)Activator.CreateInstance(typeof(TestAggregateDoSomething));

            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            await _router.Send(command);

            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_throw_if_handler_throws()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            await Assert.ThrowsAsync<ConcurrencyException>(
                async () => await _router.Send(new TestAggregateDoSomething {ExpectedVersion = 30}));
        }

        [Fact]
        public async Task Should_wait_for_send_to_finish()
        {
            var handler = new TestAggregateDoSomethingHandler();
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            await _router.Send(new TestAggregateDoSomething {LongRunning = true});
            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            var handler = new TestAggregateDoSomethingHandler();
            _router.RegisterHandler<TestAggregateDoSomething>(handler.Handle);
            await _router.Send(new TestAggregateDoSomething(), token);
            Assert.Equal(token, handler.Token);
        }
    }
}