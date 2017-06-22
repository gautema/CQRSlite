using System;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Commands
{
    public class RemoveItemsFromInventory : ICommand 
	{
		public readonly int Count;

	    public RemoveItemsFromInventory(IIdentity id, int count, int originalVersion)
	    {
	        Id = id;
			Count = count;
            ExpectedVersion = originalVersion;
        }

        public IIdentity Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}
