using System.Web.Mvc;
using System.Web.Routing;
using CQRSCode.ReadModel;
using CQRSlite;
using CQRSlite.Bus;
using CQRSlite.Contracts.Bus.Handlers;
using CQRSlite.Contracts.Infrastructure.DI;

namespace CQRSWeb
{	
    public class MvcApplication : System.Web.HttpApplication
    {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
			AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RegisterHandlers((IServiceLocator)DependencyResolver.Current);
        }

        private void RegisterHandlers(IServiceLocator serviceLocator)
        {
            var registerer = new BusRegistrar(serviceLocator);
			registerer.Register(typeof(HandlesCommand<>), typeof(HandlesEvent<>), typeof(ReadModelFacade));
        }
    }
}