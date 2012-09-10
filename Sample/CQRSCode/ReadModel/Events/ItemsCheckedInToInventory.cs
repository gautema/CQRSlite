using System;
using CQRSlite.Eventing;

namespace CQRSCode.ReadModel.Events
{
    public class ItemsCheckedInToInventory : Event
    {
        public readonly int Count;
 
        public ItemsCheckedInToInventory(Guid id, int count) 
        {
            Id = id;
            Count = count;
        }
    }
}