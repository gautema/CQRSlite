using System;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using CQRSlite;
using CQRSlite.Contracts.Bus.Handlers;

namespace CQRSCode.ReadModel.Handlers
{
    public class InventoryItemDetailView : HandlesEvent<InventoryItemCreated>,
											HandlesEvent<InventoryItemDeactivated>,
											HandlesEvent<InventoryItemRenamed>,
											HandlesEvent<ItemsRemovedFromInventory>,
											HandlesEvent<ItemsCheckedInToInventory>
    {
        public void Handle(InventoryItemCreated message)
        {
            InMemoryDatabase.Details.Add(message.Id, new InventoryItemDetailsDto(message.Id, message.Name, 0, message.Version));
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
            if(!InMemoryDatabase.Details.TryGetValue(id, out d))
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
            InMemoryDatabase.Details.Remove(message.Id);
        }
    }
}
