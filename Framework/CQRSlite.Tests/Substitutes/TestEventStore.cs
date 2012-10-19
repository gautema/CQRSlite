using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestEventStore : IEventStore
    {
        public TestEventStore()
        {
            EmptyGuid = Guid.NewGuid();
            SavedEvents = new List<IEvent>();
        }

        public Guid EmptyGuid { get; private set; }

        public IEnumerable<IEvent> Get(Guid aggregateId, int version)
        {
            if (aggregateId == EmptyGuid || aggregateId == Guid.Empty)
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

        public void Save(IEvent @event)
        {
            SavedEvents.Add(@event);
        }

        private List<IEvent> SavedEvents { get; set; }
    }
}
