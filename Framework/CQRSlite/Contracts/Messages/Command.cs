using System;

namespace CQRSlite.Contracts.Messages
{
    public class Command : Message
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }
}