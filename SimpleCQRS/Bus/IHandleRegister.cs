using System;

namespace SimpleCQRS.Bus
{
    public interface IHandleRegister
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}