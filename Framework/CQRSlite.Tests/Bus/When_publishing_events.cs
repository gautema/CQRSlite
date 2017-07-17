using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Bus;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Bus
{
    public class When_publishing_events
    {
        private InProcessBus _bus;

        public When_publishing_events()
        {
            _bus = new InProcessBus();
        }
		
        [Fact]
        public async Task Should_publish_to_all_handlers()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomethingElse>((x, token) => handler.Handle(x));
            _bus.RegisterHandler<TestAggregateDidSomethingElse>((x, token) => handler.Handle(x));
            await _bus.Publish(new TestAggregateDidSomethingElse());
            Assert.Equal(2, handler.TimesRun);
        }

        [Fact]
        public async Task Should_publish_to_all_cancellation_handlers()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            await _bus.Publish(new TestAggregateDidSomething());
            Assert.Equal(2, handler.TimesRun);
        }

        [Fact]
        public async Task Should_work_with_no_handlers()
        {
            await _bus.Publish(new TestAggregateDidSomething());
        }

        [Fact]
        public async Task Should_throw_if_handler_throws()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            await Assert.ThrowsAsync<ConcurrencyException>(
                async () => await _bus.Publish(new TestAggregateDidSomething {Version = -10}));
        }

        [Fact]
        public async Task Should_wait_for_published_to_finish()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            await _bus.Publish(new TestAggregateDidSomething {LongRunning = true});
            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            await _bus.Publish(new TestAggregateDidSomething {LongRunning = true}, token);
            Assert.Equal(token, handler.Token);
        }
    }
}