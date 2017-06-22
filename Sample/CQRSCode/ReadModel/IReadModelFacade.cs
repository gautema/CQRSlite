using System;
using System.Collections.Generic;
using CQRSlite.Domain;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel
{
    public interface IReadModelFacade
    {
        IEnumerable<InventoryItemListDto> GetInventoryItems();
        InventoryItemDetailsDto GetInventoryItemDetails(IIdentity id);
    }
}