using System;

namespace CQRSlite.Contracts.Bus.Messages
{
    public class Command : Message
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }
}