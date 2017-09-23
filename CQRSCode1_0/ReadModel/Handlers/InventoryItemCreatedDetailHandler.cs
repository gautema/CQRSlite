using CQRSCode1_0.ReadModel.Dtos;
using CQRSCode1_0.ReadModel.Events;
using CQRSCode1_0.ReadModel.Infrastructure;
using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSCode1_0.ReadModel.Handlers
{
    public class InventoryItemCreatedDetailHandler : ICancellableEventHandler<InventoryItemCreated>
    {
        public Task Handle(InventoryItemCreated message, CancellationToken token)
        {
            InMemoryDatabase.Details.Add(message.Id,
                new InventoryItemDetailsDto(message.Id, message.Name, 0, message.Version));
            return Task.CompletedTask;
        }
    }
}
