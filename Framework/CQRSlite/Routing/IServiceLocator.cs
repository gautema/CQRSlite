using System;

namespace CQRSlite.Routing
{
    public interface IServiceLocator
    {
        T GetService<T>();
        object GetService(Type type);
    }
}
