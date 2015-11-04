using CQRSCode.ReadModel;
using CQRSCode.WriteModel;
using CQRSlite.Bus;
using CQRSlite.Cache;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Web;

namespace CQRSWeb 
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            var container = new Container(x =>
            {
                x.For<InProcessBus>().Singleton().Use<InProcessBus>();
                x.For<ICommandSender>().Use(y => y.GetInstance<InProcessBus>());
                x.For<IEventPublisher>().Use(y => y.GetInstance<InProcessBus>());
                x.For<IHandlerRegistrar>().Use(y => y.GetInstance<InProcessBus>());
                x.For<ISession>().HybridHttpOrThreadLocalScoped().Use<Session>();
                x.For<IEventStore>().Singleton().Use<InMemoryEventStore>();
                x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(y =>
                    new CacheRepository(new Repository(y.GetInstance<IEventStore>(), y.GetInstance<IEventPublisher>()),
                        y.GetInstance<IEventStore>()));

                x.Scan(s =>
                {
                    s.AssemblyContainingType<SmDependencyResolver>();
                    s.AssemblyContainingType<ReadModelFacade>();
                    s.Convention<FirstInterfaceConvention>();
                });
            });
            return container;
        }
    }
}