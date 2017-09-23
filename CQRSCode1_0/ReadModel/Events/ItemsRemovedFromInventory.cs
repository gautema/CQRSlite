using System;
using CQRSlite.Events;

namespace CQRSCode1_0.ReadModel.Events
{
    public class ItemsRemovedFromInventory : IEvent
    {
        public readonly int Count;
 
        public ItemsRemovedFromInventory(Guid id, int count) 
        {
            Id = id;
            Count = count;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}