using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Eventing;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestInMemoryEventRepository : IEventRepository 
    {
        private readonly List<EventStore.EventDescriptor> _list = new List<EventStore.EventDescriptor>();

        public void Save(Guid aggregateId, EventStore.EventDescriptor eventDescriptors)
        {
            _list.Add(eventDescriptors);
        }

        public bool TryGetEvents(Guid aggregateId, int eventDescriptors, out List<EventStore.EventDescriptor> list)
        {
            list = _list;
            return true;
        }

        public int GetVersion(Guid aggregateId)
        {
            if (_list.Count() == 0) return 0;
            return _list.Max(x => x.Version);
        }
    }
}