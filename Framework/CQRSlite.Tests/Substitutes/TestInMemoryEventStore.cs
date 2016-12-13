﻿using System;
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

        public Task SaveAsync<T>(IEnumerable<IEvent> events)
        {
            Save<T>(events);
            return Task.FromResult(0);
        }

        public IEnumerable<IEvent> Get<T>(Guid aggregateId, int fromVersion)
        {
            lock(Events)
            {
                return Events.Where(x => x.Version > fromVersion && x.Id == aggregateId).OrderBy(x => x.Version).ToList();
            }
        }
    }
}