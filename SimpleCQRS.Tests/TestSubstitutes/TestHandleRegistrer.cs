using System;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestHandleRegistrer : IHandleRegister
    {
        public static int RegisteredHandlers;

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            RegisteredHandlers += 1;
        }
    }
}
