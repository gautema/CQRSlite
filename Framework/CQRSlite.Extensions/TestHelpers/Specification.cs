using System;
using System.Collections.Generic;
using System.Linq;
using CQRSlite.Commanding;
using CQRSlite.Domain;
using CQRSlite.Eventing;
using NUnit.Framework;

namespace CQRSlite.Extensions.TestHelpers
{
	[TestFixture]
    public abstract class Specification<TAggregate, THandler, TCommand> 
        where TAggregate: AggregateRoot
        where THandler : class, IHandles<TCommand>
        where TCommand : Command
    {

        protected TAggregate Aggregate { get; set; }
        protected IRepository<TAggregate> Repository { get; set; }
        protected ISession Session { get; set; }
        protected abstract IEnumerable<Event> Given();
        protected abstract TCommand When();
        protected abstract THandler BuildHandler();

        protected Snapshot Snapshot { get; set; }
        protected IList<Event> EventDescriptors { get; set; }
        protected IList<Event> PublishedEvents { get; set; }
		
		[SetUp]
        public void Run()
        {
            var eventstorage = new SpecEventStorage(Given().ToList());
            var snapshotstorage = new SpecSnapShotStorage(Snapshot);
            var eventpublisher = new SpecEventPublisher();

            Repository = new Repository<TAggregate>(Session, eventstorage, snapshotstorage);

            Aggregate = Repository.Get(Guid.Empty);

            var handler = BuildHandler();
            handler.Handle(When());

            Snapshot = snapshotstorage.Snapshot;
            PublishedEvents = eventpublisher.PublishedEvents;
            EventDescriptors = eventstorage.Events;
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
        public SpecEventStorage(IList<Event> events)
        {
            Events = events;
        }

        public IList<Event> Events { get; set; }

        public void Save(Guid aggregateId, Event @event)
        {
            Events.Add(@event);
        }

        public IEnumerable<Event> Get(Guid aggregateId, int fromVersion)
        {
            return Events;
        }

        public int GetVersion(Guid aggregateId)
        {
            return Events.Max(x => x.Version);
        }
    }
}
