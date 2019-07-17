using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStore : IEventStore
    {
        private List<IEvent> SavedEvents { get; }
        public CancellationToken Token { get; private set; }

        public TestEventStore()
        {
            SavedEvents = new List<IEvent>();
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int version, CancellationToken cancellationToken = default)
        {
            Token = cancellationToken;
            if (aggregateId == default)
            {
                return Task.FromResult((IEnumerable<IEvent>)new List<IEvent>());
            }

            return Task.FromResult(new List<IEvent>
            {
                new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                new TestAggregateDidSomethingElse {Id = aggregateId, Version = 2},
                new TestAggregateDidSomething {Id = aggregateId, Version = 3},
            }.Where(x => x.Version > version));
        }

        public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            Token = cancellationToken;
            SavedEvents.AddRange(events);
            return Task.CompletedTask;
        }

    }
}
