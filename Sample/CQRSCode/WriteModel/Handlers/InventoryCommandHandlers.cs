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

        public void Handle(DeactivateInventoryItem message)
        {
            var item = _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Deactivate();
            _session.Commit();
        }

        public void Handle(RemoveItemsFromInventory message)
        {
            var item = _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.Remove(message.Count);
            _session.Commit();
        }

        public void Handle(CheckInItemsToInventory message)
        {
            var item = _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.CheckIn(message.Count);
            _session.Commit();
        }

        public void Handle(RenameInventoryItem message)
        {
            var item = _session.Get<InventoryItem>(message.Id, message.ExpectedVersion);
            item.ChangeName(message.NewName);
            _session.Commit();
        }
    }
}
