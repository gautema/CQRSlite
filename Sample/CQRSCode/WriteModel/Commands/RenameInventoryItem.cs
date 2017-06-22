using System;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Commands
{
    public class RenameInventoryItem : ICommand 
	{
        public readonly string NewName;

        public RenameInventoryItem(IIdentity id, string newName, int originalVersion)
        {
            Id = id;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }

        public IIdentity Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}