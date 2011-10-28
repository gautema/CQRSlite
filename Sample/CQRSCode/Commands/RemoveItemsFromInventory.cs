using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class RemoveItemsFromInventory : Command 
	{
		public Guid InventoryItemId;
		public readonly int Count;

	    public RemoveItemsFromInventory(Guid inventoryItemId, int count, int originalVersion)
        {
			InventoryItemId = inventoryItemId;
			Count = count;
            ExpectedVersion = originalVersion;
        }
	}
}
