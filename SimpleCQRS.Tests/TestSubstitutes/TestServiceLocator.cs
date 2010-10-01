using System;
using SimpleCQRS.Bus;
using SimpleCQRS.Config;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestServiceLocator : IServiceLocator {
        public T GetInstance<T>()
        {
            return (T)GetInstance(typeof(T));
        }

        public object GetInstance(Type type)
        {
            if(type == typeof(IHandleRegister))
                return new TestHandleRegistrer();
            return new TestAggregateDoSomethingHandler();
        }

        public int HandleCount { get; set; }
    }
}