using System;
using System.Threading.Tasks;
using CQRSlite.Domain;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Handlers
{
    public class InventoryItemDetailView : IEventHandler<InventoryItemCreated>,
											IEventHandler<InventoryItemDeactivated>,
											IEventHandler<InventoryItemRenamed>,
											IEventHandler<ItemsRemovedFromInventory>,
											IEventHandler<ItemsCheckedInToInventory>
    {
        public Task Handle(InventoryItemCreated message)
        {
            InMemoryDatabase.Details.Add(message.Id, new InventoryItemDetailsDto(message.Id, message.Name, 0, message.Version));
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemRenamed message)
        {
            var d = GetDetailsItem(message.Id);
            d.Name = message.NewName;
            d.Version = message.Version;
            return Task.CompletedTask;
        }

        private InventoryItemDetailsDto GetDetailsItem(IIdentity id)
        {
            if (!InMemoryDatabase.Details.TryGetValue(id, out var dto))
            {
                throw new InvalidOperationException("did not find the original inventory this shouldnt happen");
            }
            return dto;
        }

        public Task Handle(ItemsRemovedFromInventory message)
        {
            var dto = GetDetailsItem(message.Id);
            dto.CurrentCount -= message.Count;
            dto.Version = message.Version;
            return Task.CompletedTask;
        }

        public Task Handle(ItemsCheckedInToInventory message)
        {
            var dto = GetDetailsItem(message.Id);
            dto.CurrentCount += message.Count;
            dto.Version = message.Version;
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemDeactivated message)
        {
            InMemoryDatabase.Details.Remove(message.Id);
            return Task.CompletedTask;
        }
    }
}
