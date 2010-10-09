using System;
using SimpleCQRS.Config;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.ConfigTests
{
    public class WhenRegistering
    {
        private BusRegisterer _register;
        private TestServiceLocator _locator;

        public WhenRegistering()
        {
            _register = new BusRegisterer();
            _locator = new TestServiceLocator();
            if (TestHandleRegistrer.HandlerList.Count == 0)
                _register.Register(_locator, GetType());
        }

        [Fact]
        public void ShouldRegisterAllHandlers()
        {
            Assert.Equal(3, TestHandleRegistrer.HandlerList.Count);
        }

        [Fact]
        public void ShouldRegisterHandlerMethod()
        {
            foreach (var handler in TestHandleRegistrer.HandlerList)
            {
                Assert.Equal("Handle",((dynamic)handler).Method.Name);
            }
        }
    }
}
