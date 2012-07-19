using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
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