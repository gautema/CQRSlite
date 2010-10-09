using System;

namespace CQRSlite.Bus
{
    public interface IHandleRegister
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}