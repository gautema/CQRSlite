using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using CQRSlite.Contracts.Handlers;

namespace CQRSCode.ReadModel.Handlers
{
	public class InventoryListView : HandlesEvent<InventoryItemCreated>,
										HandlesEvent<InventoryItemRenamed>,
										HandlesEvent<InventoryItemDeactivated>
    {
        public void Handle(InventoryItemCreated message)
        {
            InMemoryDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
        }

        public void Handle(InventoryItemRenamed message)
        {
            var item = InMemoryDatabase.List.Find(x => x.Id == message.Id);
            item.Name = message.NewName;
        }

        public void Handle(InventoryItemDeactivated message)
        {
            InMemoryDatabase.List.RemoveAll(x => x.Id == message.Id);
        }
    }
}