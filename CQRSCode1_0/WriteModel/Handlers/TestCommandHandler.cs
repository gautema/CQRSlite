using CQRSCode1_0.WriteModel.Commands;
using CQRSlite.Commands;
using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQRSCode1_0.WriteModel.Handlers
{
    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly ITest test;

        public TestCommandHandler(ITest test)
        {
            this.test = test;
        }

        public async Task Handle(TestCommand message)
        {
            Console.WriteLine(message.Value);
        }
    }
}
