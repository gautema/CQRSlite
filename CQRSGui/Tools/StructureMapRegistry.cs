using SimpleCQRS;
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
        }
    }
}