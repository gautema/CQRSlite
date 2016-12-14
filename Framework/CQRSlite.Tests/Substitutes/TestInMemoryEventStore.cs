using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Events;
using System.Threading.Tasks;

namespace CQRSlite.Tests.Substitutes
{
    public class TestInMemoryEventStore : IEventStore 
    {
        public readonly List<IEvent> Events = new List<IEvent>();

        public void Save<T>(IEnumerable<IEvent> events)
        {
            lock (Events)
            {
                Events.AddRange(events);
            }
        }

        public async Task SaveAsync<T>(IEnumerable<IEvent> events)
        {
            Save<T>(events);
            // simulate async delay
            await Task.Delay(100);
        }

        public IEnumerable<IEvent> Get<T>(Guid aggregateId, int fromVersion)
        {
            lock(Events)
            {
                return Events.Where(x => x.Version > fromVersion && x.Id == aggregateId).OrderBy(x => x.Version).ToList();
            }
        }

        public async Task<IEnumerable<IEvent>> GetAsync<T>(Guid aggregateId, int fromVersion)
        {
            var events = Get<T>(aggregateId, fromVersion);
            
            // simulate async delay
            await Task.Delay(100);

            return events;
        }
    }
}