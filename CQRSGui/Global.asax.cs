using System.Web.Mvc;
using System.Web.Routing;
using CQRSGui.Tools;
using Microsoft.Practices.ServiceLocation;
using SimpleCQRS;
using SimpleCQRS.CommandHandlers;
using SimpleCQRS.Commands;
using SimpleCQRS.Domain;
using SimpleCQRS.Events;
using SimpleCQRS.EventStore;
using SimpleCQRS.ReadModel;
using StructureMap;

namespace CQRSGui
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
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

            RegisterRoutes(RouteTable.Routes);

            var registry = new StructureMapRegistry();
            var container = new Container(registry);
            var structureMapServiceLocator = new StructureMapServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => structureMapServiceLocator);
            var locator = new StructureMapServiceLocatorControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(locator);

            var bus = container.GetInstance<ICommandSender>() as FakeBus;

            var storage = new EventStore(bus);
            var rep = new Repository<InventoryItem>(storage);
            var commands = new InventoryCommandHandlers(rep);
            bus.RegisterHandler<CheckInItemsToInventory>(commands.Handle);
            bus.RegisterHandler<CreateInventoryItem>(commands.Handle);
            bus.RegisterHandler<DeactivateInventoryItem>(commands.Handle);
            bus.RegisterHandler<RemoveItemsFromInventory>(commands.Handle);
            bus.RegisterHandler<RenameInventoryItem>(commands.Handle);
            var detail = new InvenotryItemDetailView();
            bus.RegisterHandler<InventoryItemCreated>(detail.Handle);
            bus.RegisterHandler<InventoryItemDeactivated>(detail.Handle);
            bus.RegisterHandler<InventoryItemRenamed>(detail.Handle);
            bus.RegisterHandler<ItemsCheckedInToInventory>(detail.Handle);
            bus.RegisterHandler<ItemsRemovedFromInventory>(detail.Handle);
            var list = new InventoryListView();
            bus.RegisterHandler<InventoryItemCreated>(list.Handle);
            bus.RegisterHandler<InventoryItemRenamed>(list.Handle);
            bus.RegisterHandler<InventoryItemDeactivated>(list.Handle);
        }
    }
}