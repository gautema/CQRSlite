using System;
using CQRSlite.Contracts.Bus.Messages;

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