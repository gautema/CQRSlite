using System;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSCode.ReadModel.Events
{
    public class InventoryItemDeactivated : Event 
	{
        public InventoryItemDeactivated(Guid id)
        {
            Id = id;
        }
    }
}