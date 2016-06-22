using CQRSlite.Bus;
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
        public void Should_publish_to_all_handlers()
        {
            var handler = new TestAggregateDidSomethingHandler();
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            _bus.RegisterHandler<TestAggregateDidSomething>(handler.Handle);
            _bus.Publish(new TestAggregateDidSomething());
            Assert.Equal(2, handler.TimesRun);
        }

        [Fact]
        public void Should_work_with_no_handlers()
        {
            _bus.Publish(new TestAggregateDidSomething());
        }
    }
}