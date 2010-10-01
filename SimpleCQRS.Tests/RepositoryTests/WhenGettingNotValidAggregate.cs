using System;
using SimpleCQRS.Domain;
using SimpleCQRS.Tests.TestSubstitutes;
using Xunit;

namespace SimpleCQRS.Tests.RepositoryTests
{
    public class WhenGettingNotValidAggregate
    {
        private Repository<TestAggregateNoParameterLessConstructor> _rep;

        public WhenGettingNotValidAggregate()
        {
            var eventStore = new TestEventStore();
            _rep = new Repository<TestAggregateNoParameterLessConstructor>(eventStore);
        }

        [Fact]
        public void ShouldThrowIfNoParameterlessConstructor()
        {

            Assert.Throws<AggreagateMissingParameterlessConstructorException>(() =>
                                                                                  {
                                                                                      _rep.GetById(Guid.NewGuid());
                                                                                  });

        }
    }
}