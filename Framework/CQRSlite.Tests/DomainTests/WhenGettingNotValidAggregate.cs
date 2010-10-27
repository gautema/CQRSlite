using System;
using CQRSlite.Domain;
using CQRSlite.Tests.TestSubstitutes;
using Xunit;

namespace CQRSlite.Tests.DomainTests
{
    public class WhenGettingNotValidAggregate
    {
        private Repository<TestAggregateNoParameterLessConstructor> _rep;

        public WhenGettingNotValidAggregate()
        {
            var eventStore = new TestEventStore();
            var eventPublisher = new TestEventPublisher();
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(eventStore, null, eventPublisher);
        }

        [Fact]
        public void ShouldThrowIfNoParameterlessConstructor()
        {

            Assert.Throws<AggreagateMissingParameterlessConstructorException>(() =>
                                                                                  {
                                                                                      _rep.Get(Guid.NewGuid());
                                                                                  });

        }
    }
}