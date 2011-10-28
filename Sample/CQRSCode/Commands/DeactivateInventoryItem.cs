using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class DeactivateInventoryItem : Command 
	{
        public DeactivateInventoryItem(Guid inventoryItemId, int originalVersion)
        {
            AggregateId = inventoryItemId;
            ExpectedVersion = originalVersion;
        }
    }
}