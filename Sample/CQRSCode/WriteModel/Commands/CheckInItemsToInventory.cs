using System;
using CQRSlite.Commanding;

namespace CQRSCode.WriteModel.Commands
{
    public class CheckInItemsToInventory : Command 
	{
        public readonly int Count;

        public CheckInItemsToInventory(Guid id, int count, int originalVersion) 
		{
            Id = id;
            Count = count;
            ExpectedVersion = originalVersion;
        }
    }
}