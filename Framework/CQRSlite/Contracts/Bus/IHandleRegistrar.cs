using System;
using CQRSlite.Contracts.Messages;

namespace CQRSlite.Contracts.Bus
{
    public interface IHandleRegistrar
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}