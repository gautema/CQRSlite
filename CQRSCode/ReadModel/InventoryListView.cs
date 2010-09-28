using CQRSCode.Events;
using CQRSCode.Infrastructure;
using CQRSCode.ReadModel.Dtos;
using SimpleCQRS;
using SimpleCQRS.Interfaces;

namespace CQRSCode.ReadModel
{
    public class InventoryListView : IHandles<InventoryItemCreated>, IHandles<InventoryItemRenamed>, IHandles<InventoryItemDeactivated>
    {
        public void Handle(InventoryItemCreated message)
        {
            BullShitDatabase.List.Add(new InventoryItemListDto(message.Id, message.Name));
        }

        public void Handle(InventoryItemRenamed message)
        {
            var item = BullShitDatabase.List.Find(x => x.Id == message.Id);
            item.Name = message.NewName;
        }

        public void Handle(InventoryItemDeactivated message)
        {
            BullShitDatabase.List.RemoveAll(x => x.Id == message.Id);
        }
    }
}