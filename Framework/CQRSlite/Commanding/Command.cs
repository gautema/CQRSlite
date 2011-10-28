using System;

namespace CQRSlite.Commanding
{
    public class Command : Message
    {
        public Guid Id { get; set; }
        public int ExpectedVersion { get; set; }
    }
}