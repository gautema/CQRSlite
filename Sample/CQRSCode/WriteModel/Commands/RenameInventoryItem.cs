using System;
using CQRSlite.Commands;

namespace CQRSCode.WriteModel.Commands
{
    public class RenameInventoryItem : ICommand 
	{
        public readonly string NewName;

        public RenameInventoryItem(Guid id, string newName, int originalVersion)
        {
            Id = id;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }

        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}