using System;
using System.Collections.Generic;
using CQRSlite.Routing;

namespace CQRSlite.Tests.Substitutes
{
    public class TestServiceLocator : IServiceProvider
    {
        private readonly TestHandleRegistrar _registrar;

        public TestServiceLocator(TestHandleRegistrar registrar)
        {
            _registrar = registrar;
        }
        public readonly List<dynamic> Handlers = new List<dynamic>();
        public bool ReturnNull { get; set; }

        public object GetService(Type type)
        {
            if (ReturnNull)
                return null;

            if(type == typeof(IHandlerRegistrar))
                return _registrar;

            if (type == typeof(TestAggregateDidSomethingHandler))
            {
                var handler = new TestAggregateDidSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestAggregateDidSomethingInternalHandler))
            {
                var handler = new TestAggregateDidSomethingInternalHandler();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestAggregateDoSomethingElseHandler))
            {
                var handler = new TestAggregateDoSomethingElseHandler();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestAggregateDoSomethingHandler))
            {
                var handler = new TestAggregateDoSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestAggregateDoSomethingHandlerExplicit))
            {
                var handler = new TestAggregateDoSomethingHandlerExplicit();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestAggregateDoSomethingHandlerExplicit2))
            {
                var handler = new TestAggregateDoSomethingHandlerExplicit2();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestGetSomethingHandler))
            {
                var handler = new TestGetSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(TestGetSomethingHandler2))
            {
                var handler = new TestGetSomethingHandler2();
                Handlers.Add(handler);
                return handler;
            }
            if (type == typeof(AllHandler))
            {
                var handler = new AllHandler();
                Handlers.Add(handler);
                return handler;
            }
            throw new ArgumentException($"Type {type.Name} not registered");
        }
    }
}