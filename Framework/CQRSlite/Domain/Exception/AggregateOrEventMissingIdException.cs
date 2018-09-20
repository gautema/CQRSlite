using System;

namespace CQRSlite.Domain.Exception
{
    public class AggregateOrEventMissingIdException : System.Exception
    {
        public AggregateOrEventMissingIdException(Type aggregateType, Type eventType)
            : base($"An event of type {eventType.FullName} was tried saved from {aggregateType.FullName} but no id where set on either")
        { }
    }
}