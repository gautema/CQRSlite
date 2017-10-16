using CQRSlite.Domain.Exception;
using CQRSlite.Events;
using CQRSlite.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CQRSlite.Domain
{
    /// <summary>
    /// Class to inherit all aggregates from
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public Guid Id { get; protected set; }
        public int Version { get; protected set; }

        public IEvent[] GetUncommittedChanges()
        {
            lock (_changes)
            {
                return _changes.ToArray();
            }
        }

        /// <summary>
        /// Returns all uncommited changes and clears aggregate of them.
        /// </summary>
        /// <returns>Array of new uncommited events</returns>
        public IEvent[] FlushUncommitedChanges()
        {
            lock (_changes)
            {
                var changes = _changes.ToArray();
                var i = 0;
                foreach (var @event in changes)
                {
                    if (@event.Id == Guid.Empty && Id == Guid.Empty)
                    {
                        throw new AggregateOrEventMissingIdException(GetType(), @event.GetType());
                    }
                    if (@event.Id == Guid.Empty)
                    {
                        @event.Id = Id;
                    }
                    i++;
                    @event.Version = Version + i;
                    @event.TimeStamp = DateTimeOffset.UtcNow;
                }
                Version = Version + changes.Length;
                _changes.Clear();
                return changes;
            }
        }

        /// <summary>
        /// Load an aggregate from an enumerable of events.
        /// </summary>
        /// <param name="history">All events to be loaded.</param>
        public void LoadFromHistory(IEnumerable<IEvent> history)
        {
            lock (_changes)
            {
                foreach (var e in history.ToArray())
                {
                    if (e.Version != Version + 1)
                    {
                        throw new EventsOutOfOrderException(e.Id);
                    }
                    ApplyEvent(e);
                    Id = e.Id;
                    Version++;
                }
            }
        }

        protected void ApplyChange(IEvent @event)
        {
            lock (_changes)
            {
                ApplyEvent(@event);
                _changes.Add(@event);
            }
        }

        /// <summary>
        /// Overrideable method for applying events on aggregate
        /// This is called interally when rehydrating aggregates.
        /// Can be overridden if you want custom handling.
        /// </summary>
        /// <param name="event">Event to apply</param>
        protected virtual void ApplyEvent(IEvent @event)
        {
            this.Invoke("Apply", @event);
        }
    }
}
