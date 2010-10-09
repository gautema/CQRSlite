using System;

namespace SimpleCQRS.Config
{
    public interface IServiceLocator {
        T GetInstance<T>();
        object GetInstance(Type type);
    }
}