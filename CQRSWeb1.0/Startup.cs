using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Routing;
using CQRSCode1_0.WriteModel;
using CQRSlite.Caching;
using CQRSlite.Domain;
using CQRSCode1_0.ReadModel;
using CQRSCode1_0.WriteModel.Commands;
using CQRSCode1_0.WriteModel.Handlers;
using CQRSCode1_0.ReadModel.Handlers;

namespace CQRSWeb1_0
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            //Add Cqrs services
            services.AddSingleton<Router>(new Router());
            services.AddSingleton<ICommandSender>(y => y.GetService<Router>());
            services.AddSingleton<IEventPublisher>(y => y.GetService<Router>());
            services.AddSingleton<IHandlerRegistrar>(y => y.GetService<Router>());
            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddSingleton<ICache, MemoryCache>();
            services.AddScoped<IRepository>(y => new CacheRepository(new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>(), y.GetService<ICache>()));
            services.AddScoped<CQRSlite.Domain.ISession, Session>();
            

            services.AddTransient<IReadModelFacade, ReadModelFacade>();

            //services.AddTransient<ICommandHandler<CreateInventoryItem>, InventoryCommandHandlers>();
            //services.AddTransient<ICancellableCommandHandler<DeactivateInventoryItem>, InventoryCommandHandlers>();
            //services.AddTransient<ICancellableCommandHandler<RemoveItemsFromInventory>, InventoryCommandHandlers>();
            //services.AddTransient<ICancellableCommandHandler<CheckInItemsToInventory>, InventoryCommandHandlers>();
            //services.AddTransient<ICancellableCommandHandler<RenameInventoryItem>, InventoryCommandHandlers>();
            services.AddTransient<CreateInventoryItemCommandHandler, CreateInventoryItemCommandHandler>();
            services.AddTransient<InventoryItemCreatedDetailHandler, InventoryItemCreatedDetailHandler>();
            services.AddTransient<InventoryItemCreatedListHandler, InventoryItemCreatedListHandler>();
            

            //Scan for commandhandlers and eventhandlers
            //services.Scan(scan => scan
            //    .FromAssemblies(typeof(InventoryCommandHandlers).GetTypeInfo().Assembly)
            //        .AddClasses(classes => classes.Where(x =>
            //        {
            //            var allInterfaces = x.GetInterfaces();
            //            return
            //                allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IHandler<>)) ||
            //                allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICancellableHandler<>));
            //        }))
            //        .AsSelf()
            //        .WithTransientLifetime()
            //);
            // Add framework services.
            services.AddMvc();

            //Register routes
            var serviceProvider = services.BuildServiceProvider();
            var registrar = new RouteRegistrar(new Provider(serviceProvider));
            registrar.Register(typeof(InventoryCommandHandlers));

            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            //app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }


    //This makes scoped services work inside router.
    public class Provider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _contextAccessor;

        public Provider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _contextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
        }

        public object GetService(Type serviceType)
        {
            return _contextAccessor?.HttpContext?.RequestServices.GetService(serviceType) ??
                   _serviceProvider.GetService(serviceType);
        }
    }

    //public class Startup
    //{
    //    public Startup(IHostingEnvironment env)
    //    {
    //        var builder = new ConfigurationBuilder()
    //            .SetBasePath(env.ContentRootPath)
    //            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    //            .AddEnvironmentVariables();
    //        Configuration = builder.Build();
    //    }

    //    public IConfigurationRoot Configuration { get; }

    //    // This method gets called by the runtime. Use this method to add services to the container.
    //    public void ConfigureServices(IServiceCollection services)
    //    {
    //        // Add framework services.
    //        services.AddMvc();
    //    }

    //    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    //    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    //    {
    //        loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    //        loggerFactory.AddDebug();

    //        app.UseMvc();
    //    }
    //}
}
