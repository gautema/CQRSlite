using System;
using System.Collections.Generic;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.Infrastructure
{
    public static class BullShitDatabase 
    {
        public static Dictionary<Guid, InventoryItemDetailsDto> Details = new Dictionary<Guid,InventoryItemDetailsDto>();
        public static List<InventoryItemListDto> List = new List<InventoryItemListDto>();
    }
}