using System;
using SimpleCQRS.Config;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.ConfigTests
{
    public class WhenRegestering
    {
        private BusRegisterer _register;
        private TestServiceLocator _locator;

        public WhenRegestering()
        {
            _register = new BusRegisterer();
            _locator = new TestServiceLocator();
            _register.Register(_locator, GetType());
        }

        [Fact]
        public void ShouldRegisterAllHandlers()
        {
            Assert.Equal(2, TestHandleRegistrer.HandlerList.Count);
        }

        [Fact]
        public void ShouldRegisterHandlerMethod()
        {
            foreach (var handler in TestHandleRegistrer.HandlerList)
            {
                Assert.Equal("Handle",((Action<TestAggregateDoSomething>)handler).Method.Name);
            }
        }
    }
}
