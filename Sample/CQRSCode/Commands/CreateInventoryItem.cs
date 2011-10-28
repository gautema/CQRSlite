using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class CreateInventoryItem : Command 
	{
        public readonly string Name;
	    
        public CreateInventoryItem(Guid inventoryItemId, string name)
        {
            AggregateId = inventoryItemId;
            Name = name;
            ExpectedVersion = -1;
        }
    }
}