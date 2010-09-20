using System;
using System.Collections.Generic;
using SimpleCQRS.ReadModel.Dtos;

namespace SimpleCQRS.ReadModel
{
    public interface IReadModelFacade
    {
        IEnumerable<InventoryItemListDto> GetInventoryItems();
        InventoryItemDetailsDto GetInventoryItemDetails(Guid id);
    }
}