using System;
using SimpleCQRS.Bus;
using System.Collections;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestHandleRegistrer : IHandleRegister
    {
        public static IList HandlerList = new ArrayList();

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            HandlerList.Add(handler);
        }
    }
}
