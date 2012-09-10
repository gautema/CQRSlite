using System;
using CQRSlite.Bus;
using CQRSlite.Tests.TestSubstitutes;
using NUnit.Framework;

namespace CQRSlite.Tests.BusTests
{
	[TestFixture]
    public class When_registering_handlers
    {
        private BusRegistrar _register;
        private TestServiceLocator _locator;

		[SetUp]
        public void Setup()
        {
            _locator = new TestServiceLocator();
            _register = new BusRegistrar(_locator);
            if (TestHandleRegistrar.HandlerList.Count == 0)
                _register.Register(GetType());
        }

        [Test]
        public void Should_register_all_handlers()
        {
            Assert.AreEqual(3, TestHandleRegistrar.HandlerList.Count);
        }

        [Test]
        public void Should_be_able_to_run_all_handlers()
        {
            foreach (var item in TestHandleRegistrar.HandlerList)
            {
                var @event = Activator.CreateInstance(item.Type);
                item.Handler(@event);
            }
            foreach (var handler in _locator.Handlers)
            {
                Assert.That(handler.TimesRun, Is.EqualTo(1));
            }
        }
    }
}
