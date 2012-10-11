using System;
using CQRSlite.Messages;

namespace CQRSlite.Events
{
    public interface IEvent : IMessage
    {
        int Version { get; set; }
        DateTimeOffset TimeStamp { get; set; }
    }
}

