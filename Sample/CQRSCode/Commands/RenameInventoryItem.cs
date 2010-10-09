using System;
using SimpleCQRS;
using SimpleCQRS.Commanding;

namespace CQRSCode.Commands
{
    public class RenameInventoryItem : Command {
        public readonly Guid InventoryItemId;
        public readonly string NewName;
        public readonly int OriginalVersion;

        public RenameInventoryItem(Guid inventoryItemId, string newName, int originalVersion)
        {
            InventoryItemId = inventoryItemId;
            NewName = newName;
            OriginalVersion = originalVersion;
        }
    }
}