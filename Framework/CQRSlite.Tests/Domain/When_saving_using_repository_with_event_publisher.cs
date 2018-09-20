using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Tests.Substitutes;
using Xunit;

namespace CQRSlite.Tests.Domain
{
    public class When_saving_using_repository_with_event_publisher
    {
        private readonly TestAggregateNoParameterLessConstructor _aggregate;
        private readonly TestEventPublisher _eventPublisher;
        private readonly ISession _session;

        public When_saving_using_repository_with_event_publisher()
        {
            var eventStore = new TestInMemoryEventStore();
            _eventPublisher = new TestEventPublisher();
#pragma warning disable 618
            var rep = new Repository(eventStore, _eventPublisher);
#pragma warning restore 618
            _session = new Session(rep);

            _aggregate = new TestAggregateNoParameterLessConstructor(2);
        }

        [Fact]
        public async Task Should_publish_events()
        {
            _aggregate.DoSomething();
            await _session.Add(_aggregate);
            await _session.Commit();
            Assert.Equal(1, _eventPublisher.Published);
        }

        [Fact]
        public async Task Should_forward_cancellation_token()
        {
            var token = new CancellationToken();
            _aggregate.DoSomething();
            await _session.Add(_aggregate, token);
            await _session.Commit(token);
            Assert.Equal(token, _eventPublisher.Token);
        }
    }
}