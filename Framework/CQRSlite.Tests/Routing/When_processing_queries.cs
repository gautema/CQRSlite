using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain.Exception;
using CQRSlite.Routing;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Routing
{
    public class When_processing_queries
    {
        private readonly Router _router;

        public When_processing_queries()
        {
            _router = new Router();
        }

        [Fact]
        public async Task Should_run_handler()
        {
            var handler = new TestGetSomethingHandler2();
            _router.RegisterHandler<TestGetSomething>((x, token) => handler.Handle(x));
            await _router.Query(new TestGetSomething());

            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_run_cancellation_handler()
        {
            var handler = new TestGetSomethingHandler();
            _router.RegisterHandler<TestGetSomething>((x, token) => handler.Handle(x, token));
            await _router.Query(new TestGetSomething());

            Assert.Equal(1, handler.TimesRun);
        }

        [Fact]
        public async Task Should_get_result()
        {
            var handler = new TestGetSomethingHandler();
            _router.RegisterHandler<TestGetSomething>((x, token) => handler.Handle(x, token));
            var res = await _router.Query(new TestGetSomething());

            Assert.Equal("test", res);
        }

        [Fact]
        public async Task Should_throw_if_more_handlers()
        {
            var handler = new TestGetSomethingHandler();
            _router.RegisterHandler<TestGetSomething>(handler.Handle);
            _router.RegisterHandler<TestGetSomething>(handler.Handle);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _router.Query(new TestGetSomething()));
        }

        [Fact]
        public async Task Should_throw_if_no_handlers()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _router.Query(new TestGetSomething()));
        }


        [Fact]
        public async Task Should_throw_if_handler_throws()
        {
            var handler = new TestGetSomethingHandler();
            _router.RegisterHandler<TestGetSomething>(handler.Handle);
            await Assert.ThrowsAsync<ConcurrencyException>(
                async () => await _router.Query(new TestGetSomething {ExpectedVersion = 30}));
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            var handler = new TestGetSomethingHandler();
            _router.RegisterHandler<TestGetSomething>(handler.Handle);
            await _router.Query(new TestGetSomething(), token);
            Assert.Equal(token, handler.Token);
        }
    }
}