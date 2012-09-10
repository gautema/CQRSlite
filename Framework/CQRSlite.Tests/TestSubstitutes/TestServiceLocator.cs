using System;
using System.Collections.Generic;
using CQRSlite.Bus;
using CQRSlite.Contracts.Bus;
using CQRSlite.Contracts.Config;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestServiceLocator : IServiceLocator
    {
        public List<dynamic> Handlers = new List<dynamic>();
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type type)
        {
            if(type == typeof(IHandleRegistrar))
                return new TestHandleRegistrar();
            if (type == typeof(TestAggregateDidSomethingHandler))
            {
                var handler = new TestAggregateDidSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
            else
            {
                var handler = new TestAggregateDoSomethingHandler();
                Handlers.Add(handler);
                return handler;
            }
        }
    }
}