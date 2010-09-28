using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel
{
    public interface IReadModelFacade
    {
        IEnumerable<InventoryItemListDto> GetInventoryItems();
        InventoryItemDetailsDto GetInventoryItemDetails(Guid id);
    }
}