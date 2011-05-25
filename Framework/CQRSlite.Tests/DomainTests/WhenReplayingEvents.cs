using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.DomainTests
{
	[TestFixture]
    public class WhenReplayingEvents
    {
        private TestAggregate _aggregate;

		[SetUp]
        public void Setup()
        {
            _aggregate = new TestAggregate();
        }

        [Test]
        public void ShouldCallApplyIfExists()
        {
            _aggregate.DoSomething();
            Assert.AreEqual(1, _aggregate.I);
        }

        [Test]
        public void ShouldNotFailApplyIfDontExists()
        {
            _aggregate.DoSomethingElse();
            Assert.AreEqual(0, _aggregate.I);
        }
    }
}
