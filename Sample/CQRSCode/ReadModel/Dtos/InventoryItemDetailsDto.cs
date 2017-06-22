using System;
using CQRSlite.Domain;

namespace CQRSCode.ReadModel.Dtos
{
    public class InventoryItemDetailsDto
    {
        public IIdentity Id;
        public string Name;
        public int CurrentCount;
        public int Version;

        public InventoryItemDetailsDto(IIdentity id, string name, int currentCount, int version)
        {
            Id = id;
            Name = name;
            CurrentCount = currentCount;
            Version = version;
        }
    }
}