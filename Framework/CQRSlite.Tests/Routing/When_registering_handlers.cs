using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Routing;
using CQRSlite.Routing.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Routing
{
    public class When_registering_handlers
    {
        private readonly TestServiceLocator _locator;

        public When_registering_handlers()
        {
            _locator = new TestServiceLocator();
            var register = new RouteRegistrar(_locator);
            register.Register(GetType());
        }

        [Fact]
        public void Should_register_all_handlers()
        {
            // 5 public declared handlers, one internal declared
            Assert.Equal(6, TestHandleRegistrar.HandlerList.Count);
        }

        [Fact]
        public async Task Should_be_able_to_run_all_handlers()
        {
            foreach (var item in TestHandleRegistrar.HandlerList)
            {
                var eventtypes = GetEventTypesMatching(item.Type);
                foreach (var type in eventtypes)
                {
                    var @event = Activator.CreateInstance(type);
                    await item.Handler(@event, new CancellationToken());
                }
            }
            foreach (var handler in _locator.Handlers)
            {
                Assert.Equal(1, handler.TimesRun);
            }
            Assert.Equal(9, _locator.Handlers.Count);
        }

        [Fact]
        public async Task Unresolved_handlers_should_throw()
        {
            _locator.ReturnNull = true;
            foreach (var item in TestHandleRegistrar.HandlerList)
            {
                var eventtypes = GetEventTypesMatching(item.Type);
                foreach (var type in eventtypes)
                {
                    var @event = Activator.CreateInstance(type);
                    await Assert.ThrowsAsync<HandlerNotResolvedException>(async () =>
                    {
                        await item.Handler(@event, new CancellationToken());
                    });
                }
            }
        }

        private IEnumerable<Type> GetEventTypesMatching(Type type)
        {
            return GetType().GetTypeInfo().Assembly.GetTypes().Where(x => type.GetTypeInfo().IsAssignableFrom(x));
        }
    }
}
