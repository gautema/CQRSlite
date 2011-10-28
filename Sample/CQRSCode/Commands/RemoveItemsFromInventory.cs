using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class RemoveItemsFromInventory : Command 
	{
		public readonly int Count;

	    public RemoveItemsFromInventory(Guid inventoryItemId, int count, int originalVersion)
        {
			AggregateId = inventoryItemId;
			Count = count;
            ExpectedVersion = originalVersion;
        }
	}
}
