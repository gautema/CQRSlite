using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class DeactivateInventoryItem : Command 
	{
        public readonly Guid InventoryItemId;

        public DeactivateInventoryItem(Guid inventoryItemId, int originalVersion)
        {
            InventoryItemId = inventoryItemId;
            ExpectedVersion = originalVersion;
        }
    }
}