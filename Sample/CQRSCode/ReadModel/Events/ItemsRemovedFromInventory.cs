using System;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSCode.ReadModel.Events
{
    public class ItemsRemovedFromInventory : Event
    {
        public readonly int Count;
 
        public ItemsRemovedFromInventory(Guid id, int count) 
        {
            Id = id;
            Count = count;
        }
    }
}