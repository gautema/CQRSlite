using System;

namespace CQRSlite.Contracts.Config
{
    public interface IServiceLocator 
	{
        T GetService<T>();
        object GetService(Type type);
    }
}