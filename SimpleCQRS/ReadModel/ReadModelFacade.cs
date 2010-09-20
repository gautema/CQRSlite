using System;
using System.Collections.Generic;

namespace SimpleCQRS.ReadModel
{
    public class ReadModelFacade : IReadModelFacade
    {
        public IEnumerable<InventoryItemListDto> GetInventoryItems()
        {
            return BullShitDatabase.list;
        }

        public InventoryItemDetailsDto GetInventoryItemDetails(Guid id)
        {
            return BullShitDatabase.details[id];
        }
    }
}