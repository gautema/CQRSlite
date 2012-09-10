using System;
using CQRSlite.Commanding;

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