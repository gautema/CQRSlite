using System;
using CQRSlite.Events;

namespace CQRSlite.Tests.Substitutes
{
    public class TestAggregateCreated : IEvent
    {
        private readonly Guid _id;

        public TestAggregateCreated(Guid id)
        {
            _id = id;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}