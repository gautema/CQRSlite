using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestInMemoryEventStore : IEventStore 
    {
        private readonly List<EventDescriptor> _list = new List<EventDescriptor>();

        public void Save(Guid aggregateId, EventDescriptor eventDescriptors)
        {
            _list.Add(eventDescriptors);
        }

        public IEnumerable<EventDescriptor> Get(Guid aggregateId, int fromVersion)
        {
            return _list;
        }

        public int GetVersion(Guid aggregateId)
        {
            if (_list.Count() == 0) return 0;
            return _list.Max(x => x.Version);
        }
    }
}