using CQRSCode.ReadModel;
using CQRSlite.Bus;
using CQRSlite.Commanding;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace CQRSGui.Tools
{
    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry()
        {
            For<InProcessBus>().Singleton().Use<InProcessBus>();
            For<ICommandSender>().Use(x => x.GetInstance<InProcessBus>());
            For<IEventPublisher>().Use(x => x.GetInstance<InProcessBus>());
            For<IHandleRegister>().Use(x => x.GetInstance<InProcessBus>());
            For(typeof(IRepository<>)).Use(typeof(Repository<>));

            Scan(s =>
                     {
                         s.TheCallingAssembly();
                         s.AssemblyContainingType<InProcessBus>();
                         s.AssemblyContainingType<ReadModelFacade>();
                         s.Convention<FirstInterfaceConvention>();
                     });
        }
    }
}