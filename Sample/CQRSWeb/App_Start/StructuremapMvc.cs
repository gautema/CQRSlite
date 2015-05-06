using System.Web.Mvc;
using CQRSWeb.App_Start;
using WebActivator;

[assembly: PreApplicationStartMethod(typeof(StructuremapMvc), "Start")]
namespace CQRSWeb.App_Start 
{
    public static class StructuremapMvc 
    {
        public static void Start() 
        {
            var container = IoC.Initialize();
            DependencyResolver.SetResolver(new SmDependencyResolver(container));
        }
    }
}