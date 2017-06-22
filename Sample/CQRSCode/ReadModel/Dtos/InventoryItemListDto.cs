using System;
using CQRSlite.Domain;

namespace CQRSCode.ReadModel.Dtos
{
    public class InventoryItemListDto
    {
        public IIdentity Id;
        public string Name;

        public InventoryItemListDto(IIdentity id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}