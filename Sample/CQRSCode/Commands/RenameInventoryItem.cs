using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
{
    public class RenameInventoryItem : Command 
	{
        public readonly string NewName;

        public RenameInventoryItem(Guid inventoryItemId, string newName, int originalVersion)
        {
            AggregateId = inventoryItemId;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }
    }
}