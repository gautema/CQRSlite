using System;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_replaying_events
    {
        private readonly TestAggregate _aggregate;

        public When_replaying_events()
        {
            _aggregate = new TestAggregate(Guid.NewGuid());
        }

        [Fact]
        public void Should_call_apply_if_exist()
        {
            _aggregate.DoSomething();
            Assert.Equal(1, _aggregate.DidSomethingCount);
        }

        [Fact]
        public void Should_not_fail_apply_if_dont_exist()
        {
            _aggregate.DoSomethingElse();
            Assert.Equal(0, _aggregate.DidSomethingCount);
        }
    }
}
