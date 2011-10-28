using System;

namespace CQRSlite.Eventing
{
	public class Event : Message
	{
	    public Guid Id;
        public int Version;
	    public DateTimeOffset TimeStamp;
	}
}

