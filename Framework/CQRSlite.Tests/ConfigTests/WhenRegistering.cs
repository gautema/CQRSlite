using CQRSlite.Config;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.ConfigTests
{
	[TestFixture]
    public class When_registering
    {
        private BusRegisterer _register;
        private TestServiceLocator _locator;

		[SetUp]
        public void Setup()
        {
            _register = new BusRegisterer();
            _locator = new TestServiceLocator();
            if (TestHandleRegistrer.HandlerList.Count == 0)
                _register.Register(_locator, GetType());
        }

        [Test]
        public void Should_register_all_handlers()
        {
            Assert.AreEqual(3, TestHandleRegistrer.HandlerList.Count);
        }

        [Test]
        public void Should_register_handler_method()
        {
            foreach (var handler in TestHandleRegistrer.HandlerList)
            {
                Assert.AreEqual("Handle",((dynamic)handler).Method.Name);
            }
        }
    }
}
