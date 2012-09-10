using System;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Contracts.Bus
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}