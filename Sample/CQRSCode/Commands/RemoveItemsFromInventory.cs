using System;
using CQRSlite.Commanding;

namespace CQRSCode.Commands
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
