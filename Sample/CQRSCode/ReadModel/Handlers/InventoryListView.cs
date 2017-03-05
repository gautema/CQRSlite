using System.Threading.Tasks;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using CQRSlite.Events;

namespace CQRSCode.ReadModel.Handlers
{
	public class InventoryListView : IEventHandler<InventoryItemCreated>,
										IEventHandler<InventoryItemRenamed>,
										IEventHandler<InventoryItemDeactivated>
    {
        public Task Handle(InventoryItemCreated message)
        {
            InMemoryDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemRenamed message)
        {
            var item = InMemoryDatabase.List.Find(x => x.Id == message.Id);
            item.Name = message.NewName;
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemDeactivated message)
        {
            InMemoryDatabase.List.RemoveAll(x => x.Id == message.Id);
            return Task.CompletedTask;
        }
    }
}