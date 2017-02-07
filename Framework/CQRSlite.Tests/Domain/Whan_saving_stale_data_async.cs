using CQRSlite.Domain;
using CQRSlite.Domain.Exception;
using CQRSlite.Tests.Substitutes;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class Whan_saving_stale_data_async
    {
        private TestInMemoryEventStore _eventStore;
        private TestAggregate _aggregate;
        private Repository _rep;
        private Session _session;


        public Whan_saving_stale_data_async()
        {
            _eventStore = new TestInMemoryEventStore();
            _rep = new Repository(_eventStore);
            _session = new Session(_rep);

            _aggregate = new TestAggregate(Guid.NewGuid());
            _aggregate.DoSomething();
            _rep.Save(_aggregate);
        }

        [Fact]
        public async Task Should_throw_concurrency_exception_from_repository()
        {
            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _rep.SaveAsync(_aggregate, 0));
        }

        [Fact]
        public async Task Should_throw_concurrency_exception_from_session()
        {
            _session.Add(_aggregate);
            _aggregate.DoSomething();
            await _rep.SaveAsync(_aggregate);
            await Assert.ThrowsAsync<ConcurrencyException>(async () => await _session.CommitAsync());
        }
    }
}
