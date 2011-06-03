using System;
using CQRSlite.Bus;
using CQRSlite.Config;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestServiceLocator : IServiceLocator {
        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type type)
        {
            if(type == typeof(IHandleRegister))
                return new TestHandleRegistrer();
            if (type == typeof(TestAggregateDidSomethingHandler))
                return new TestAggregateDidSomethingHandler();
            return new TestAggregateDoSomethingHandler();
        }
    }
}