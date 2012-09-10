using System;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSCode.WriteModel.Commands
{
    public class RemoveItemsFromInventory : Command 
	{
		public readonly int Count;

	    public RemoveItemsFromInventory(Guid id, int count, int originalVersion)
	    {
	        Id = id;
			Count = count;
            ExpectedVersion = originalVersion;
        }
	}
}
