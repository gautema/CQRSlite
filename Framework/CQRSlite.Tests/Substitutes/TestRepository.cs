using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Domain;

namespace CQRSlite.Tests.Substitutes
{
    public class TestRepository : IRepository
    {
        public bool Throw { set; private get; }
        public Task Save<T>(T aggregate, int? expectedVersion = null, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            Saved = aggregate;
            if (Throw)
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }

        public AggregateRoot Saved { get; private set; }

        public Task<T> Get<T>(Guid aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            if (Throw)
            {
                throw new Exception();
            }
            var obj = (T) Activator.CreateInstance(typeof (T), true);
            obj.LoadFromHistory(new[] {new TestAggregateDidSomething {Id = aggregateId, Version = 1}});
            return Task.FromResult(obj);
        }
    }
}