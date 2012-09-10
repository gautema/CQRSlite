using System;
using CQRSlite.Eventing;

namespace CQRSCode.ReadModel.Events
{
    public class InventoryItemCreated : Event 
	{
        public readonly string Name;
        public InventoryItemCreated(Guid id, string name) 
        {
            Id = id;
            Name = name;
        }
    }
}