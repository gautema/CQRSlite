using SimpleCQRS;
using SimpleCQRS.Domain;
using SimpleCQRS.EventStore;
using SimpleCQRS.ReadModel;
using StructureMap.Configuration.DSL;

namespace CQRSGui.Tools
{
    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry()
        {
            For<FakeBus>().Singleton().Use<FakeBus>();
            For<ICommandSender>().Use(x => x.GetInstance<FakeBus>());
            For<IEventPublisher>().Use(x => x.GetInstance<FakeBus>());
            For<IHandleRegister>().Use(x => x.GetInstance<FakeBus>());
            For<IEventStore>().Use<EventStore>();
            For<IReadModelFacade>().Use<ReadModelFacade>();
            For(typeof(IRepository<>)).Use(typeof(Repository<>));
        }
    }
}