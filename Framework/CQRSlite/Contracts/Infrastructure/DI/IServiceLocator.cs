using System;

namespace CQRSlite.Contracts.Infrastructure.DI
{
    public interface IServiceLocator 
	{
        T GetService<T>();
        object GetService(Type type);
    }
}