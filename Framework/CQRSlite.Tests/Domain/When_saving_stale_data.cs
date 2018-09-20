using System;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_saving_stale_data
    {
        private readonly TestAggregate _aggregate;
        private readonly Repository _rep;
        private readonly Session _session;


        public When_saving_stale_data()
        {
            var eventStore = new TestInMemoryEventStore();
            _rep = new Repository(eventStore);
            _session = new Session(_rep);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _aggregate.DoSomething();
            Task.Run(() => _rep.Save(_aggregate)).Wait();
        }

        [Fact]
        public async Task Should_throw_concurrency_exception_from_repository()
        {
            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _rep.Save(_aggregate, 0));
        }

        [Fact]
        public async Task Should_throw_concurrency_exception_from_session()
        {
            await _session.Add(_aggregate);
            _aggregate.DoSomething();
            await _rep.Save(_aggregate);
            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _session.Commit());
        }
    }
}