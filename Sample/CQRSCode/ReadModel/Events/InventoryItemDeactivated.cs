using System;
using CQRSlite.Eventing;

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