using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using StructureMap;
using IServiceLocator = CQRSlite.Config.IServiceLocator;

namespace CQRSGui.Tools
{
    public class StructureMapServiceLocator : ServiceLocatorImplBase, IServiceLocator
    {
        private readonly IContainer _container;

        public StructureMapServiceLocator(IContainer container)
        {
            _container = container;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null) return null;

            return string.IsNullOrEmpty(key) 
                ? _container.GetInstance(serviceType) 
                : _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType).Cast<object>();
        }
    }
}
