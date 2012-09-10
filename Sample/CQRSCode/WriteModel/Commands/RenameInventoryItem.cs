using System;
using CQRSlite.Contracts.Messages;

namespace CQRSCode.WriteModel.Commands
{
    public class RenameInventoryItem : Command 
	{
        public readonly string NewName;

        public RenameInventoryItem(Guid id, string newName, int originalVersion)
        {
            Id = id;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }
    }
}