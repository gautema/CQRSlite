using System;
using CQRSlite.Domain;

namespace CQRSlite.Infrastructure
{
	public class ReflectionCacheKey
	{

		public Type AggregateRootType { get; private set; }

		public object Data { get; private set; }

		public ReflectionCacheKey(AggregateRoot aggregateRoot, object data)
		{
			this.AggregateRootType = aggregateRoot.GetType();
			if (data != null)
			{
				this.Data = data.GetType();
			}
		}

	}
}
