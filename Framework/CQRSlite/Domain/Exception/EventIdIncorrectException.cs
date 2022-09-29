using System;

namespace CQRSlite.Domain.Exception
{
    public class EventIdIncorrectException : System.Exception
    {
        public EventIdIncorrectException( Guid id, Guid aggregateId, Type eventType)
            : base($"Event {id} of type {eventType.Name} has a different Id from it's aggregates Id ({aggregateId}). Have you have forgotten to set the Id property on the event class?")
        { }
    }
}
