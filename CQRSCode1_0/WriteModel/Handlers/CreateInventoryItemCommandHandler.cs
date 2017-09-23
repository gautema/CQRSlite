using CQRSCode1_0.WriteModel.Commands;
using CQRSCode1_0.WriteModel.Domain;
using CQRSlite.Commands;
using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQRSCode1_0.WriteModel.Handlers
{
    public class CreateInventoryItemCommandHandler : ICommandHandler<CreateInventoryItem>
    {
        private readonly ISession _session;

        public CreateInventoryItemCommandHandler(ISession session)
        {
            this._session = session;
        }

        public async Task Handle(CreateInventoryItem message)
        {
            if (string.IsNullOrWhiteSpace(_session.Name))
            {
                _session.Name = message.Name;
            }

            var item = new InventoryItem(message.Id, message.Name);
            await _session.Add(item);
            await _session.Commit();
        }
    }
}
