using System;
using System.Collections.Generic;
using CQRSCode1_0.ReadModel.Dtos;

namespace CQRSCode1_0.ReadModel
{
    public interface IReadModelFacade
    {
        IEnumerable<InventoryItemListDto> GetInventoryItems();
        InventoryItemDetailsDto GetInventoryItemDetails(Guid id);
    }
}