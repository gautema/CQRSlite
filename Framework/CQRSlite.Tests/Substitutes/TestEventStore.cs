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
        private readonly Guid _emptyGuid;
        private List<IEvent> SavedEvents { get; }
        public CancellationToken Token { get; private set; }

        public TestEventStore()
        {
            _emptyGuid = Guid.NewGuid();
            SavedEvents = new List<IEvent>();
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            Token = cancellationToken;
            if (aggregateId == _emptyGuid || aggregateId == Guid.Empty)
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

        public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            Token = cancellationToken;
            SavedEvents.AddRange(events);
            return Task.CompletedTask;
        }

    }
}
