using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerable<IEvent> Get<T>(Guid aggregateId, int version)
        {
            if (aggregateId == _emptyGuid || aggregateId == Guid.Empty)
            {
                return new List<IEvent>();
            }

            return new List<IEvent>
                {
                    new TestAggregateDidSomething {Id = aggregateId, Version = 1},
                    new TestAggregateDidSomeethingElse {Id = aggregateId, Version = 2},
                    new TestAggregateDidSomething {Id = aggregateId, Version = 3},
                }.Where(x => x.Version > version);
        }

        public Task<IEnumerable<IEvent>> GetAsync<T>(Guid aggregateId, int fromVersion)
        {
            return Task.FromResult(Get<T>(aggregateId, fromVersion));
        }

        public void Save<T>(IEnumerable<IEvent> events)
        {
            SavedEvents.AddRange(events);
        }

        public Task SaveAsync<T>(IEnumerable<IEvent> events)
        {
            Save<T>(events);
            return Task.FromResult(0);
        }

        private List<IEvent> SavedEvents { get; }
    }
}
