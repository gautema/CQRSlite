using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Events;
using CQRSCode.ReadModel.Infrastructure;
using CQRSCode.ReadModel.Queries;
using CQRSlite.Events;
using CQRSlite.Queries;

namespace CQRSCode.ReadModel.Handlers
{
	public class InventoryListView : ICancellableEventHandler<InventoryItemCreated>,
	    ICancellableEventHandler<InventoryItemRenamed>,
	    ICancellableEventHandler<InventoryItemDeactivated>,
	    ICancellableQueryHandler<GetInventoryItems, List<InventoryItemListDto>>
    {
        public Task Handle(InventoryItemCreated message, CancellationToken token)
        {
            InMemoryDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemRenamed message, CancellationToken token)
        {
            var item = InMemoryDatabase.List.Find(x => x.Id == message.Id);
            item.Name = message.NewName;
            return Task.CompletedTask;
        }

        public Task Handle(InventoryItemDeactivated message, CancellationToken token)
        {
            InMemoryDatabase.List.RemoveAll(x => x.Id == message.Id);
            return Task.CompletedTask;
        }

        public Task<List<InventoryItemListDto>> Handle(GetInventoryItems message, CancellationToken token = default)
        {
            return Task.FromResult(InMemoryDatabase.List);
        }
    }
}