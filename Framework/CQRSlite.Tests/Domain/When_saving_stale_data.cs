using System;
using System.Linq;
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
        private readonly TestInMemoryEventStore _eventStore;
        private readonly Guid _id;

        public When_saving_stale_data()
        {
            _eventStore = new TestInMemoryEventStore();
            _rep = new Repository(_eventStore);
            _session = new Session(_rep);

            _id = Guid.NewGuid();
            _aggregate = new TestAggregate(_id);
            _aggregate.DoSomething();
            Task.Run(async () => await _rep.Save(_aggregate)).Wait();
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

        [Fact]
        public async Task Should_update_version_number()
        {
            var aggregate = new TestAggregate(_id);
            aggregate.DoSomething();
            await _rep.Save(aggregate);

            Assert.Equal(_eventStore.Events.Last().Version, _eventStore.Events.Count);
        }
    }
}