using System;
using CQRSCode.ReadModel.Dtos;
using CQRSlite.Queries;

namespace CQRSCode.ReadModel.Queries
{
    public class GetInventoryItemDetails : IQuery<InventoryItemDetailsDto>
    {
        public GetInventoryItemDetails(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
