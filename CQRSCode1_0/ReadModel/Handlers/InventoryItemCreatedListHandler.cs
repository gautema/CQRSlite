using CQRSCode1_0.ReadModel.Dtos;
using CQRSCode1_0.ReadModel.Events;
using CQRSCode1_0.ReadModel.Infrastructure;
using CQRSlite.Events;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSCode1_0.ReadModel.Handlers
{
    public class InventoryItemCreatedListHandler : ICancellableEventHandler<InventoryItemCreated>
    {
        public Task Handle(InventoryItemCreated message, CancellationToken token)
        {
            InMemoryDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
            return Task.CompletedTask;
        }
    }
}
