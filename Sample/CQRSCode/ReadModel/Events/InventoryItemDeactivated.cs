using System;
using CQRSlite.Domain;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Events
{
    public class InventoryItemDeactivated : IEvent 
	{
        public InventoryItemDeactivated(IIdentity id)
        {
            Id = id;
        }

        public IIdentity Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
	}
}