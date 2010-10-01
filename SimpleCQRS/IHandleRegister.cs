using System;

namespace SimpleCQRS
{
    public interface IHandleRegister
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}