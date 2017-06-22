using System;
using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Events
{
    public class InventoryItemCreated : IEvent 
	{
        public readonly string Name;
        public InventoryItemCreated(IIdentity id, string name) 
        {
            Id = id;
            Name = name;
        }

        public IIdentity Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
	}
}