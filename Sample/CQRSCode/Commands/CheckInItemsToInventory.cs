using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class CheckInItemsToInventory : Command 
	{
        public readonly int Count;

        public CheckInItemsToInventory(Guid inventoryItemId, int count, int originalVersion) 
		{
            AggregateId = inventoryItemId;
            Count = count;
            ExpectedVersion = originalVersion;
        }
    }
}