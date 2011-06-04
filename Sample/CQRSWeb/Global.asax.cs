using System.Web.Mvc;
using System.Web.Routing;
using CQRSCode.ReadModel;
using CQRSlite;
using CQRSlite.Config;

namespace CQRSWeb
{	
	public class MonoWebFormViewEngine : WebFormViewEngine
	{
    	protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
    	{
        	return base.FileExists(controllerContext, virtualPath.Replace("~", ""));
    	}
	}

	public class MonoRazorViewEngine : RazorViewEngine
	{
    	protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
    	{
        	return base.FileExists(controllerContext, virtualPath.Replace("~", ""));
    	}
	}
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
			ViewEngines.Engines.Clear();
    		ViewEngines.Engines.Add(new MonoWebFormViewEngine());
    		ViewEngines.Engines.Add(new MonoRazorViewEngine());
			AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            RegisterHandlers((IServiceLocator)DependencyResolver.Current);
        }

        private void RegisterHandlers(IServiceLocator serviceLocator)
        {
            var registerer = new BusRegisterer();
            registerer.Register(serviceLocator, typeof(IHandles<>), typeof(ReadModelFacade));
        }
    }
}