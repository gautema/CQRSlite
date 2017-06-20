using CQRSlite.Messages;
using CQRSlite.Domain;
using System;

namespace CQRSlite.Events
{
    public interface IEvent : IMessage
    {
        IIdentity Id { get; set; }
        int Version { get; set; }
        DateTimeOffset TimeStamp { get; set; }
    }
}
