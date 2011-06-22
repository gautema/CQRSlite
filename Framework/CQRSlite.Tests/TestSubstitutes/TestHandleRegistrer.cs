using System;
using System.Collections.Generic;
using CQRSlite.Bus;

namespace CQRSlite.Tests.TestSubstitutes
{
    public class TestHandleRegistrer : IHandleRegister
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
