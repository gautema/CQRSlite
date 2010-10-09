using System;
using SimpleCQRS;
using SimpleCQRS.Commanding;

namespace CQRSCode.Commands
{
    public class CreateInventoryItem : Command {
        public readonly Guid InventoryItemId;
        public readonly string Name;
	    
        public CreateInventoryItem(Guid inventoryItemId, string name)
        {
            InventoryItemId = inventoryItemId;
            Name = name;
        }
    }
}