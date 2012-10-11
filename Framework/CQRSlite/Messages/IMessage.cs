using System;

namespace CQRSlite.Messages
{
	public interface IMessage
	{
        Guid Id { get; set; }
	}
}
