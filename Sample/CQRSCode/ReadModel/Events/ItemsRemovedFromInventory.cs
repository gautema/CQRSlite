using System;
using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Events
{
    public class ItemsRemovedFromInventory : IEvent
    {
        public readonly int Count;
 
        public ItemsRemovedFromInventory(IIdentity id, int count) 
        {
            Id = id;
            Count = count;
        }

        public IIdentity Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}