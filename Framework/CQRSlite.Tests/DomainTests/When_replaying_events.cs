using System;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class When_replaying_events
    {
        private TestAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            _aggregate = new TestAggregate(Guid.NewGuid());
        }

        [Test]
        public void Should_call_apply_if_exist()
        {
            _aggregate.DoSomething();
            Assert.AreEqual(1, _aggregate.DidSomethingCount);
        }

        [Test]
        public void Should_not_fail_apply_if_dont_exist()
        {
            _aggregate.DoSomethingElse();
            Assert.AreEqual(0, _aggregate.DidSomethingCount);
        }
    }
}
