using System;
using System.Collections.Generic;
using CQRSlite.Bus;
using CQRSlite.Contracts.Bus;
using CQRSlite.Contracts.Bus.Messages;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestHandleRegistrar : IHandlerRegistrar
    {
        public static IList<TestHandlerListItem> HandlerList = new List<TestHandlerListItem>();

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            HandlerList.Add(new TestHandlerListItem {Type = typeof(T),Handler = handler});
        }
    }

    public class TestHandlerListItem
    {
        public Type Type;
        public dynamic Handler;
    }
}
