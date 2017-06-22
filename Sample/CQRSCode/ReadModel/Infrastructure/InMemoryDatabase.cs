using System;
using System.Collections.Generic;
using CQRSlite.Domain;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel.Infrastructure
{
    public static class InMemoryDatabase 
    {
        public static readonly Dictionary<IIdentity, InventoryItemDetailsDto> Details = new Dictionary<IIdentity, InventoryItemDetailsDto>();
        public static readonly List<InventoryItemListDto> List = new List<InventoryItemListDto>();
    }
}