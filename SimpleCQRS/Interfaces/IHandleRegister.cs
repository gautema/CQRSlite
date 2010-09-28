using System;

namespace SimpleCQRS.Interfaces
{
    public interface IHandleRegister
    {
        void RegisterHandler<T>(Action<T> handler) where T : Message;
    }
}