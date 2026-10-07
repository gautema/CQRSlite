using System;
using System.Collections.Generic;
using System.Text;
using CQRSlite.Caching;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Queries;
using CQRSlite.Routing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using CQRSlite.Messages;
using CQRSlite.Storage.Tests.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using CQRSlite.Snapshotting;

namespace CQRSlite.Storage.Tests {
    
    [TestClass]
    public class TestStartup {
        private static object _servicesLock = new object();
        private static IServiceProvider Services { get; set; }

        public static IServiceProvider GetServiceProvider() {
            if (Services == null) {
                lock (_servicesLock) {
                    if (Services == null) {
                        var builder = new ConfigurationBuilder();
                        builder.AddInMemoryCollection();
                        var config = builder.Build();
                        config[BlobEventStore.CONNECTIONSTRING_KEY] = LOCALCONNECTIONSTRING;
                        config[BlobSnapshotStore.CONNECTIONSTRING_KEY] = LOCALCONNECTIONSTRING;
                        IConfigurationRoot configuration = builder.Build();

                        IServiceCollection services = new ServiceCollection();
                        //Add Cqrs services

                        // ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy
                        services.AddSingleton<Router>(new Router());
                        services.AddSingleton<ICommandSender>(y => y.GetService<Router>());
                        services.AddSingleton<IEventPublisher>(y => y.GetService<Router>());
                        services.AddSingleton<IHandlerRegistrar>(y => y.GetService<Router>());
                        services.AddSingleton<IQueryProcessor>(y => y.GetService<Router>());
                        services.AddSingleton<IConfiguration>(config);
                        services.AddSingleton<IEventStore, BlobEventStore>();

                        services.AddSingleton<ISnapshotStore, BlobSnapshotStore>();
                        services.AddSingleton<ISnapshotStrategy, AttributeSnapshotStrategy>();
                        services.AddSingleton<ICache, MemoryCache>();

                        services.AddScoped<IBlobEventStorePathProvider>(y => new BlobPathProvider("events", "snapshots"));
                        services.AddScoped<IBlobSnapshotStorePathProvider>(y => new BlobPathProvider("events", "snapshots"));

                        services.AddScoped<IRepository>(y => new SnapshotRepository(y.GetService<ISnapshotStore>(), y.GetService<ISnapshotStrategy>(), new Repository(y.GetService<IEventStore>()), y.GetService<IEventStore>()));
                        services.AddScoped<ISession, Session>();
                        //Scan for commandhandlers and eventhandlers
                        services.Scan(scan => scan
                            .FromAssemblies(typeof(ForumCommandHandlers).GetTypeInfo().Assembly)
                                .AddClasses(classes => classes.Where(x => {
                                    var allInterfaces = x.GetInterfaces();
                                    return
                                        allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IHandler<>)) ||
                                        allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICancellableHandler<>)) ||
                                        allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(IQueryHandler<,>)) ||
                                        allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeof(ICancellableQueryHandler<,>));
                                }))
                                .AsSelf()
                                .WithTransientLifetime()
                        );

                        Services = services.BuildServiceProvider();

                        RouteRegistrar r = new RouteRegistrar(Services);
                        r.RegisterInAssemblyOf(typeof(ForumCommandHandlers));
                    }
                }
            }
            return Services;
        }

        [TestInitialize]
        public static void Startup() {

            Console.WriteLine("TestInitialize");
        }


        private static string  _LOCALCONNECTIONSTRING = null;
        public static string LOCALCONNECTIONSTRING {
 
            get
            {
                //return "azure.blob://development=true";
                if (true) {
                    if (string.IsNullOrEmpty(_LOCALCONNECTIONSTRING)) {
                        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CQRSlist.Storage.Tests");
                        if (System.IO.Directory.Exists(path)) {
                            try {
                                System.IO.Directory.Delete(path, true);
                            }
                            finally { }
                        }
                        System.IO.Directory.CreateDirectory(path);
                        Console.WriteLine(path);
                        _LOCALCONNECTIONSTRING = "disk://path=" + path;
                    }
                }
                // use if you have the Azure emulator
                //return "azure.blob://development=true";
                return _LOCALCONNECTIONSTRING;
            }
        }



    }
}
