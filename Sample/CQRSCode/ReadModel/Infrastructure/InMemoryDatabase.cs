using System.Collections.Concurrent;
using CQRSCode.ReadModel.Dtos;

namespace CQRSCode.ReadModel.Infrastructure;

// Stands in for a real read database. Concurrent collections because requests run in parallel.
public static class InMemoryDatabase
{
    public static readonly ConcurrentDictionary<Guid, InventoryItemDetailsDto> Details = new();
    public static readonly ConcurrentDictionary<Guid, InventoryItemListDto> List = new();
}
