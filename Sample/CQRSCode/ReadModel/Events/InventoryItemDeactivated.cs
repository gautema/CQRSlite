using System;
using CQRSlite.Contracts.Messages;

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