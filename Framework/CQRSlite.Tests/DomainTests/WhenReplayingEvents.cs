using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenReplayingEvents
    {
        private readonly TestAggregate _aggregate;

        public WhenReplayingEvents()
        {
            _aggregate = new TestAggregate();
        }

        [Fact]
        public void ShouldCallApplyIfExists()
        {
            _aggregate.DoSomething();
            Assert.Equal(1, _aggregate.I);
        }

        [Fact]
        public void ShouldNotFailApplyIfDontExists()
        {
            _aggregate.DoSomethingElse();
            Assert.Equal(0, _aggregate.I);
        }
    }
}
