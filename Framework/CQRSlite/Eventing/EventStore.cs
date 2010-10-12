using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Domain;

namespace CQRSlite.Eventing
{
    public class EventStore : IEventStore
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventPublisher _publisher;

        public EventStore(IEventRepository eventRepository, IEventPublisher publisher)
        {
            _eventRepository = eventRepository;
            _publisher = publisher;
        }
        
        public struct EventDescriptor
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

        public int SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            if (_eventRepository.GetVersion(aggregateId) != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;
                _eventRepository.Save(aggregateId, new EventDescriptor(aggregateId,@event,i));
                _publisher.Publish(@event);
            }
            return i;
        }

        public  IEnumerable<Event> GetEventsForAggregate(Guid aggregateId, int fromVersion)
        {
            List<EventDescriptor> eventDescriptors;
            if (!_eventRepository.TryGetEvents(aggregateId, fromVersion, out eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }
            return eventDescriptors.Where(desc => desc.Version > fromVersion).Select(desc => desc.EventData).ToList();
        }
    }
}