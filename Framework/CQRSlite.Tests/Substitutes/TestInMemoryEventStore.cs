using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestInMemoryEventStore : IEventStore 
    {
        public readonly List<IEvent> Events = new List<IEvent>();

        public void Save(IEvent @event)
        {
            Events.Add(@event);
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            return Events.Where(x => x.Version > fromVersion).OrderBy(x => x.Version);
        }

        public int GetVersion(Guid aggregateId, int? expectedVersion)
        {
            if (!Events.Any()) return 0;
            return Events.Max(x => x.Version);
        }
    }
}