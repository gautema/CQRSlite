using System;

namespace CQRSlite.Contracts.Bus.Messages
{
	public class Event : Message
	{
	    public Guid Id;
        public int Version;
	    public DateTimeOffset TimeStamp;
	}
}

