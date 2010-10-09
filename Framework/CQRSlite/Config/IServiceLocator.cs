using System;

namespace CQRSlite.Config
{
    public interface IServiceLocator {
        T GetInstance<T>();
        object GetInstance(Type type);
    }
}