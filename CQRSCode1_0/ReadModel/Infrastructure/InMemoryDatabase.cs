using System;
using System.Collections.Generic;
using CQRSCode1_0.ReadModel.Dtos;

namespace CQRSCode1_0.ReadModel.Infrastructure
{
    public static class InMemoryDatabase 
    {
        public static readonly Dictionary<Guid, InventoryItemDetailsDto> Details = new Dictionary<Guid,InventoryItemDetailsDto>();
        public static readonly List<InventoryItemListDto> List = new List<InventoryItemListDto>();
    }
}