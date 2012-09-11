using System;
using CQRSlite.Events;

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