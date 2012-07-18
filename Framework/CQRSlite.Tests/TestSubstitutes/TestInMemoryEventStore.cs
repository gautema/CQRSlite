using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestInMemoryEventStore : IEventStore 
    {
        private readonly List<Event> _list = new List<Event>();

        public void Save(Guid aggregateId, Event eventDescriptor)
        {
            _list.Add(eventDescriptor);
        }

        public IEnumerable<Event> Get(Guid aggregateId, int fromVersion)
        {
            return _list;
        }

        public int GetVersion(Guid aggregateId)
        {
            if (!_list.Any()) return 0;
            return _list.Max(x => x.Version);
        }
    }
}