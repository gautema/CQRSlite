using System;
using SimpleCQRS;
using SimpleCQRS.Eventing;

namespace CQRSCode.Events
{
    public class InventoryItemCreated : Event {
        public readonly Guid Id;
        public readonly string Name;
        public InventoryItemCreated(Guid id, string name) {
            Id = id;
            Name = name;
        }
    }
}