using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStore : IEventStore
    {
        private readonly Guid _emptyGuid;

        public TestEventStore()
        {
            _emptyGuid = Guid.NewGuid();
            SavedEvents = new List<IEvent>();
        }

        public Task<IEnumerable<IEvent>> Get(IIdentity aggregateId, int version)
        {
            if (!aggregateId.IsValid)
            {
                return Task.FromResult((IEnumerable<IEvent>)new List<IEvent>());
            }

            return Task.FromResult(new List<IEvent>
            {
                new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 2},
                new TestAggregateDidSomething {Id = aggregateId, Version = 3},
            }.Where(x => x.Version > version));
        }

        public Task Save(IEnumerable<IEvent> events)
        {
            SavedEvents.AddRange(events);
            return Task.CompletedTask;
        }

        private List<IEvent> SavedEvents { get; }
    }
}
