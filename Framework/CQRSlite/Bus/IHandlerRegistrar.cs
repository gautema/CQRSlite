﻿using CQRSlite.Messages;
using System;
using System.Threading.Tasks;

namespace CQRSlite.Bus
{
    public interface IHandlerRegistrar
    {
        void RegisterHandler<T>(Func<T,Task> handler) where T : class, IMessage;
    }
}
