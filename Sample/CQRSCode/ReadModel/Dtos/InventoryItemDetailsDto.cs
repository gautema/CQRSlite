using System;

namespace CQRSCode.ReadModel.Dtos
{
    public class InventoryItemDetailsDto
    {
        public Guid Id;
        public string Name;
        public int CurrentCount;
        public int Version;

        public InventoryItemDetailsDto(Guid id, string name, int currentCount, int version)
        {
            Id = id;
            Name = name;
            CurrentCount = currentCount;
            Version = version;
        }
    }
}