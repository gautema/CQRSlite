using System;

namespace CQRSlite.Domain.Exception
{
    public class EventsOutOfOrderException : System.Exception
    {
        public EventsOutOfOrderException(Guid id)
            : base($"EventStore gave events for aggregate {id} out of order")
        { }
    }
}