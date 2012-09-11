using System;
using CQRSlite.Messages;

namespace CQRSlite.Events
{
	public class Event : Message
	{
	    public Guid Id;
        public int Version;
	    public DateTimeOffset TimeStamp;
	}
}

