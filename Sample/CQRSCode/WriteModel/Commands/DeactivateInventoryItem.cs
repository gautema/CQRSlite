using System;
using CQRSlite.Commands;

namespace CQRSCode.WriteModel.Commands
{
    public class DeactivateInventoryItem : Command 
	{
        public DeactivateInventoryItem(Guid id, int originalVersion)
        {
            Id = id;
            ExpectedVersion = originalVersion;
        }
    }
}