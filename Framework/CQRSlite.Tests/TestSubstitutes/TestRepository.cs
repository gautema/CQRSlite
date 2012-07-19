using System;
using CQRSlite.Domain;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestRepository : IRepository
    {
        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
        }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var obj = (T) Activator.CreateInstance(typeof (T), true);
            obj.LoadFromHistory(new[] {new TestAggregateDidSomething {Id = aggregateId, Version = 1}});
            return obj;
        }

        public int GetVersion(Guid aggregateId)
        {
            return 0;
        }
    }
}