using System;

namespace CQRSlite.Domain.Exception
{
    public class AggregateNotFoundException : System.Exception
    {
        public AggregateNotFoundException(Type t, IIdentity id)
            : base($"Aggregate {id} of type {t.FullName} was not found")
        { }
    }
}
