using System;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Commands
{
    public class CheckInItemsToInventory : ICommand 
	{
        public readonly int Count;

        public CheckInItemsToInventory(IIdentity id, int count, int originalVersion) 
		{
            Id = id;
            Count = count;
            ExpectedVersion = originalVersion;
        }

        public IIdentity Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}