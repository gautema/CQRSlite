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
            For<IReadModelFacade>().Use<ReadModelFacade>();
            For<ICommandSender>().Singleton().Use<FakeBus>();
            For<IEventPublisher>().Use(x => x.GetInstance<ICommandSender>() as FakeBus);
            For<IEventStore>().Use<EventStore>();
            For(typeof(IRepository<>)).Use(typeof(Repository<>));

        }
    }
}