using System;
using System.Collections.Generic;
using SimpleCQRS.Infrastructure;
using SimpleCQRS.ReadModel.Dtos;

namespace SimpleCQRS.ReadModel
{
    public class ReadModelFacade : IReadModelFacade
    {
        public IEnumerable<InventoryItemListDto> GetInventoryItems()
        {
            return BullShitDatabase.List;
        }

        public InventoryItemDetailsDto GetInventoryItemDetails(Guid id)
        {
            return BullShitDatabase.Details[id];
        }
    }
}