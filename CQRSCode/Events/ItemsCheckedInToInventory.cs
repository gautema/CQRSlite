using System;
using SimpleCQRS;
using SimpleCQRS.Eventing;

namespace CQRSCode.Events
{
    public class ItemsCheckedInToInventory : Event
    {
        public Guid Id;
        public readonly int Count;
 
        public ItemsCheckedInToInventory(Guid id, int count) {
            Id = id;
            Count = count;
        }
    }
}