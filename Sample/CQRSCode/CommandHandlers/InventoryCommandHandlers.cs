using CQRSCode.Commands;
using CQRSCode.Domain;
using CQRSlite;
using CQRSlite.Domain;

namespace CQRSCode.CommandHandlers
{
    public class InventoryCommandHandlers : 
        IHandles<CreateInventoryItem>, 
        IHandles<DeactivateInventoryItem>, 
        IHandles<RemoveItemsFromInventory>,
        IHandles<CheckInItemsToInventory>, 
        IHandles<RenameInventoryItem>
    {
        private readonly IRepository<InventoryItem> _repository;
        public InventoryCommandHandlers(IRepository<InventoryItem> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateInventoryItem message)
        {
            var item = new InventoryItem(message.AggregateId, message.Name);
            _repository.Save(item, -1);
        }

        public void Handle(DeactivateInventoryItem message)
        {
            var item = _repository.Get(message.AggregateId);
            item.Deactivate();
            _repository.Save(item, message.ExpectedVersion);
        }

        public void Handle(RemoveItemsFromInventory message)
        {
            var item = _repository.Get(message.AggregateId);
            item.Remove(message.Count);
            _repository.Save(item, message.ExpectedVersion);
        }

        public void Handle(CheckInItemsToInventory message)
        {
            var item = _repository.Get(message.AggregateId);
            item.CheckIn(message.Count);
            _repository.Save(item, message.ExpectedVersion);
        }

        public void Handle(RenameInventoryItem message)
        {
            var item = _repository.Get(message.AggregateId);
            item.ChangeName(message.NewName);
            _repository.Save(item, message.ExpectedVersion);
        }
    }
}
