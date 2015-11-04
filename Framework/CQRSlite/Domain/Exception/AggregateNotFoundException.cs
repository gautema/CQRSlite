using System;

namespace CQRSlite.Domain.Exception
{
    public class AggregateNotFoundException : System.Exception
    {
        public AggregateNotFoundException(Guid id)
            : base($"Aggregate {id} was not found")
        {
        }
    }
}