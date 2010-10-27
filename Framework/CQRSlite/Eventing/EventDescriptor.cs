using System;

namespace CQRSlite.Eventing
{
    public class EventDescriptor
    {
        public readonly Event EventData;
        public readonly Guid Id;
        public readonly int Version;

        public EventDescriptor(Guid id, Event eventData, int version)
        {
            EventData = eventData;
            Version = version;
            Id = id;
        }
    }
}