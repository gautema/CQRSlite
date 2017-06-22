using System;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Commands
{
    public class DeactivateInventoryItem : ICommand 
	{
        public DeactivateInventoryItem(IIdentity id, int originalVersion)
        {
            Id = id;
            ExpectedVersion = originalVersion;
        }

        public IIdentity Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}