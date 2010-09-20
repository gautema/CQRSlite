using System;
using SimpleCQRS.Events;

namespace SimpleCQRS.ReadModel
{
    public class InvenotryItemDetailView : Handles<InventoryItemCreated>, Handles<InventoryItemDeactivated>, Handles<InventoryItemRenamed>, Handles<ItemsRemovedFromInventory>, Handles<ItemsCheckedInToInventory>
    {
        public void Handle(InventoryItemCreated message)
        {
            BullShitDatabase.details.Add(message.Id, new InventoryItemDetailsDto(message.Id, message.Name, 0,0));
        }

        public void Handle(InventoryItemRenamed message)
        {
            InventoryItemDetailsDto d = GetDetailsItem(message.Id);
            d.Name = message.NewName;
            d.Version = message.Version;
        }

        private InventoryItemDetailsDto GetDetailsItem(Guid id)
        {
            InventoryItemDetailsDto d;
            if(!BullShitDatabase.details.TryGetValue(id, out d))
            {
                throw new InvalidOperationException("did not find the original inventory this shouldnt happen");
            }
            return d;
        }

        public void Handle(ItemsRemovedFromInventory message)
        {
            InventoryItemDetailsDto d = GetDetailsItem(message.Id);
            d.CurrentCount -= message.Count;
            d.Version = message.Version;
        }

        public void Handle(ItemsCheckedInToInventory message)
        {
            InventoryItemDetailsDto d = GetDetailsItem(message.Id);
            d.CurrentCount += message.Count;
            d.Version = message.Version;
        }

        public void Handle(InventoryItemDeactivated message)
        {
            BullShitDatabase.details.Remove(message.Id);
        }
    }
}
