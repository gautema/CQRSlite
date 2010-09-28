using CQRSCode.Events;
using CQRSCode.Infrastructure;
using CQRSCode.ReadModel.Dtos;
using SimpleCQRS;

namespace CQRSCode.ReadModel
{
    public class InventoryListView : Handles<InventoryItemCreated>, Handles<InventoryItemRenamed>, Handles<InventoryItemDeactivated>
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