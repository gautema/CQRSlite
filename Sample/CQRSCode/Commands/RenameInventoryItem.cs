using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class RenameInventoryItem : Command 
	{
        public readonly Guid InventoryItemId;
        public readonly string NewName;

        public RenameInventoryItem(Guid inventoryItemId, string newName, int originalVersion)
        {
            InventoryItemId = inventoryItemId;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }
    }
}