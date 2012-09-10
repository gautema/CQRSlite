using System;

namespace CQRSlite.Contracts.Messages
{
	public class Event : Message
	{
	    public Guid Id;
        public int Version;
	    public DateTimeOffset TimeStamp;
	}
}

