using System;
using CQRSlite.Commands;

namespace CQRSCode.WriteModel.Commands
{
    public class CreateInventoryItem : Command 
	{
        public readonly string Name;
	    
        public CreateInventoryItem(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}