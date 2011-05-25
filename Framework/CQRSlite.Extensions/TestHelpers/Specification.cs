using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Commanding;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using NUnit.Framework;

namespace CQRSlite.Extensions.TestHelpers
{
	
    public abstract class Specification<TAggreagate, THandler, TCommand> 
        where TAggreagate: AggregateRoot
        where THandler : class, IHandles<TCommand>
        where TCommand : Command
    {

        protected TAggreagate Aggregate { get; set; }
        protected IRepository<TAggreagate> Repository { get; set; }
        protected abstract IEnumerable<Event> Given();
        protected abstract TCommand When();
        protected abstract THandler BuildHandler();

        protected Snapshot Snapshot { get; set; }
        protected IList<EventDescriptor> EventDescriptors { get; set; }
        protected IList<Event> PublishedEvents { get; set; }
		
		[SetUp]
        public void Run()
        {
            var version = 0;
            IList<EventDescriptor> descriptors = Given().Select(@event => new EventDescriptor(@event.Id, @event, version++)).ToList();
            var eventstorage = new SpecEventStorage(descriptors);
            var snapshotstorage = new SpecSnapShotStorage(Snapshot);
            var eventpublisher = new SpecEventPublisher();

            Repository = new Repository<TAggreagate>(eventstorage, snapshotstorage, eventpublisher);

            Aggregate = Repository.Get(Guid.Empty);

            var handler = BuildHandler();
            handler.Handle(When());

            Snapshot = snapshotstorage.Snapshot;
            PublishedEvents = eventpublisher.PublishedEvents;
            EventDescriptors = eventstorage.Descriptors;
        }
    }

    internal class SpecSnapShotStorage : ISnapshotStore {
        public SpecSnapShotStorage(Snapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public Snapshot Snapshot { get; set; }

        public Snapshot Get(Guid id)
        {
            return Snapshot;
        }

        public void Save(Snapshot snapshot)
        {
            Snapshot = snapshot;
        }
    }

    internal class SpecEventPublisher : IEventPublisher {
        public SpecEventPublisher()
        {
            PublishedEvents = new List<Event>();
        }

        public void Publish<T>(T @event) where T : Event
        {
            PublishedEvents.Add(@event);
        }

        public IList<Event> PublishedEvents { get; set; }
    }

    internal class SpecEventStorage : IEventStore {
        public SpecEventStorage(IList<EventDescriptor> descriptors)
        {
            Descriptors = descriptors;
        }

        public IList<EventDescriptor> Descriptors { get; set; }

        public void Save(Guid aggregateId, EventDescriptor eventDescriptors)
        {
            Descriptors.Add(eventDescriptors);
        }

        public IEnumerable<EventDescriptor> Get(Guid aggregateId, int fromVersion)
        {
            return Descriptors;
        }

        public int GetVersion(Guid aggregateId)
        {
            return Descriptors.Max(x => x.Version);
        }
    }
}
