using System;

namespace CQRSlite.Domain.Exception
{
    public class EventsOutOfOrderException : System.Exception
    {
        public EventsOutOfOrderException(Guid id)
            : base(string.Format("Eventstore gave event for aggregate {0} out of order", id))
        {
        }
    }
}