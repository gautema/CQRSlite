using System;
using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Events
{
    public class InventoryItemRenamed : IEvent
    {
        public readonly string NewName;
 
        public InventoryItemRenamed(IIdentity id, string newName)
        {
            Id = id;
            NewName = newName;
        }

        public IIdentity Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}