using System;
using SimpleCQRS;
using SimpleCQRS.Eventing;

namespace CQRSCode.Events
{
    public class InventoryItemDeactivated : Event {
        public readonly Guid Id;

        public InventoryItemDeactivated(Guid id)
        {
            Id = id;
        }
    }
}