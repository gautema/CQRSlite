using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CQRSlite.Config;
using StructureMap;

namespace CQRSWeb
{
    public class SmDependencyResolver : IDependencyResolver, IServiceLocator 
    {
        private readonly IContainer _container;

        public SmDependencyResolver(IContainer container) 
        {
            _container = container;
        }

        public T GetService<T>()
        {
            return (T) GetService(typeof (T));
        }

        public object GetService(Type serviceType) 
        {
            if (serviceType == null) return null;
            try
            {
                return serviceType.IsAbstract || serviceType.IsInterface
                    ? _container.TryGetInstance(serviceType)
                    : _container.GetInstance(serviceType);
            }
            catch 
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType) 
        {
            return _container.GetAllInstances<object>().Where(s => s.GetType() == serviceType);
        }
    }
}