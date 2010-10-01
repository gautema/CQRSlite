using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCQRS.Domain;

namespace SimpleCQRS.Eventing
{
    public class EventStore : IEventStore
    {
        private readonly IEventRepository _eventRepository;
        
        public EventStore(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        private readonly IEventPublisher _publisher;

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

        public EventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        
        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            List<EventDescriptor> eventDescriptors;
            if (!_eventRepository.TryGetEvents(aggregateId, out eventDescriptors))
            {
                eventDescriptors = new List<EventDescriptor>();
                _eventRepository.Add(aggregateId, eventDescriptors);
            }
            else if(eventDescriptors[eventDescriptors.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;
                eventDescriptors.Add(new EventDescriptor(aggregateId,@event,i));
                _publisher.Publish(@event);
            }
        }

        public  IEnumerable<Event> GetEventsForAggregate(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;
            if (!_eventRepository.TryGetEvents(aggregateId, out eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }
            return eventDescriptors.Select(desc => desc.EventData).ToList();
        }
    }
}