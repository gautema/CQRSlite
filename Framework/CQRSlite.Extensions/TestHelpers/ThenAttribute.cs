using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace CQRSlite.Extensions.TestHelpers
{
    public class ThenAttribute : FactAttribute
    {
        protected override System.Collections.Generic.IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return base.EnumerateTestCommands(method).Select(command => new ThenCommand(command));
        }
    }
}