using System;

namespace CQRSlite.Commanding
{
    public class Command : Message
    {
        public int ExpectedVersion { get; protected set; }
        public Guid AggregateId { get; protected set; }
    }
}