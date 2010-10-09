using System;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;

namespace CQRSGui.Tools
{
    public class StructureMapServiceLocatorControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            return ServiceLocator.Current.GetInstance(controllerType) as Controller;
        }
    }
}