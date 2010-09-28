using System;
using SimpleCQRS;

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