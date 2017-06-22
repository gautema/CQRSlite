using System;
using System.Collections.Generic;
using CQRSlite.Domain;
using CQRSCode.ReadModel.Dtos;
using CQRSCode.ReadModel.Infrastructure;

namespace CQRSCode.ReadModel
{
    public class ReadModelFacade : IReadModelFacade
    {
        public IEnumerable<InventoryItemListDto> GetInventoryItems()
        {
            return InMemoryDatabase.List;
        }

        public InventoryItemDetailsDto GetInventoryItemDetails(IIdentity id)
        {
            return InMemoryDatabase.Details[id];
        }
    }
}