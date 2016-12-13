using System;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestRepository : IRepository
    {
        public void Save<T>(T aggregate, int? expectedVersion = null) where T : AggregateRoot
        {
            Saved = aggregate;
            if (expectedVersion == 100)
            {
                throw new Exception();
            }
        }

        public Task SaveAsync<T>(T aggregate, int? expectedVersion = default(int?)) where T : AggregateRoot
        {
            Save<T>(aggregate, expectedVersion);
            return Task.FromResult(0);
        }

        public AggregateRoot Saved { get; private set; }

        public T Get<T>(Guid aggregateId) where T : AggregateRoot
        {
            var obj = (T) Activator.CreateInstance(typeof (T), true);
            obj.LoadFromHistory(new[] {new TestAggregateDidSomething {Id = aggregateId, Version = 1}});
            return obj;
        }
    }
}