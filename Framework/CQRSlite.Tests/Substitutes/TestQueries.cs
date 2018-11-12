using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain.Exception;
using CQRSlite.Queries;

namespace CQRSlite.Tests.Substitutes
{
    public class TestGetSomething : IQuery<string>
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }

    public class TestGetSomethingHandler : ICancellableQueryHandler<TestGetSomething, string>
    {
        public Task<string> Handle(TestGetSomething message, CancellationToken token)
        {
            if (message.ExpectedVersion != TimesRun)
                throw new ConcurrencyException(message.Id);

            TimesRun++;
            Token = token;
            return Task.FromResult("test");
        }

        public int TimesRun { get; set; }
        public CancellationToken Token { get; set; }

    }

    public class TestGetSomethingHandler2 : IQueryHandler<TestGetSomething, string>
    {
        public Task<string> Handle(TestGetSomething message)
        {
            TimesRun++;
            return Task.FromResult("test2");
        }

        public int TimesRun { get; set; }
    }
}
