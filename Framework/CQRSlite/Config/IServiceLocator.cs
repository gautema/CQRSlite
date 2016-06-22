using System;

namespace CQRSlite.Config
{
    public interface IServiceLocator
    {
        T GetService<T>();
        object GetService(Type type);
    }
}
