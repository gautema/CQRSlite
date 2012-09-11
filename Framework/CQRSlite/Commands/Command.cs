using System;
using CQRSlite.Messages;

namespace CQRSlite.Commands
{
    public class Command : Message
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }
}