using System.Threading.Tasks;
using CQRSCode.WriteModel.Commands;
using CQRSCode.WriteModel.Domain;
using CQRSlite.Commands;
using CQRSlite.Domain;

namespace CQRSCode.WriteModel.Handlers
{
    public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem>,
											ICommandHandler<DeactivateInventoryItem>,
											ICommandHandler<RemoveItemsFromInventory>,
											ICommandHandler<CheckInItemsToInventory>,
											ICommandHandler<RenameInventoryItem>
    {
        private readonly ISession _session;

        public InventoryCommandHandlers(ISession session)
        {
            _session = session;
        }

        public async Task Handle(CreateInventoryItem message)
        {
            var item = new InventoryItem(message.Id, message.Name);
            await _session.Add(item);
            await _session.Commit();
        }

        public async Task Handle(DeactivateInventoryItem message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Deactivate();
            await _session.Commit();
        }

        public async Task Handle(RemoveItemsFromInventory message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Remove(message.Count);
            await _session.Commit();
        }

        public async Task Handle(CheckInItemsToInventory message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.CheckIn(message.Count);
            await _session.Commit();
        }

        public async Task Handle(RenameInventoryItem message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.ChangeName(message.NewName);
            await _session.Commit();
        }
    }
}
