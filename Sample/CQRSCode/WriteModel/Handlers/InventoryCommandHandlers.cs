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

        public void Handle(CreateInventoryItem message)
        {
            var item = new InventoryItem(message.Id, message.Name);
            _session.Add(item);
            _session.Commit();
        }

        public async void Handle(DeactivateInventoryItem message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Deactivate();
            await _session.Commit();
        }

        public async void Handle(RemoveItemsFromInventory message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Remove(message.Count);
            await _session.Commit();
        }

        public async void Handle(CheckInItemsToInventory message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.CheckIn(message.Count);
            await _session.Commit();
        }

        public async void Handle(RenameInventoryItem message)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.ChangeName(message.NewName);
            await _session.Commit();
        }
    }
}
