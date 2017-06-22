using System;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Commands
{
    public class CreateInventoryItem : ICommand 
	{
        public readonly string Name;
	    
        public CreateInventoryItem(IIdentity id, string name)
        {
            Id = id;
            Name = name;
        }

        public IIdentity Id { get; set; }
        public int ExpectedVersion { get; set; }
	}
}