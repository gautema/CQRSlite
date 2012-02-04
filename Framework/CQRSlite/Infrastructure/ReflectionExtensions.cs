using System;
using System.Collections.Generic;
using System.Reflection;
using CQRSlite.Domain;
using CQRSlite.Eventing;

namespace CQRSlite.Infrastructure
{


	public static class ReflectionExtensions
	{

		private static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		private static IDictionary<ReflectionCacheKey, LateBoundAction> _applyCache = new Dictionary<ReflectionCacheKey, LateBoundAction>();
		private static readonly object _applyCacheLock = new object();

		public static void Apply(this AggregateRoot aggregateRoot, Event @event)
		{
			var key = new ReflectionCacheKey(aggregateRoot, @event);
			LateBoundAction action;

			lock (_applyCacheLock)
			{
				if (!_applyCache.TryGetValue(key, out action))
				{
					MethodInfo method = aggregateRoot.GetType().GetMethod("Apply", bindingFlags, null, new Type[] { @event.GetType() }, null);

					// Not all AggregateRoots will apply all events.
					if (method == null)
						return;

					action = DelegateHelper.CreateAction(method);
					_applyCache.Add(key, action);
				}
			}
			action(aggregateRoot, new object[] { @event });
		}

		private static IDictionary<ReflectionCacheKey, LateBoundFunc> _makeSnapshotCache = new Dictionary<ReflectionCacheKey, LateBoundFunc>();
		private static readonly object _makeSnapshotCacheLock = new object();

		public static Snapshot MakeSnapshot(this AggregateRoot aggregateRoot)
		{
			var key = new ReflectionCacheKey(aggregateRoot, null);
			LateBoundFunc func;

			lock (_makeSnapshotCacheLock)
			{
				if (!_makeSnapshotCache.TryGetValue(key, out func))
				{
					MethodInfo method = aggregateRoot.GetType().GetMethod("CreateSnapshot", bindingFlags);
					func = DelegateHelper.CreateFunc(method);
					_makeSnapshotCache.Add(key, func);
				}
			}
			return (Snapshot)func(aggregateRoot, new object[] { });
		}

		private static IDictionary<ReflectionCacheKey, LateBoundAction> _restoreSnapshotCache = new Dictionary<ReflectionCacheKey, LateBoundAction>();

		private static readonly object _restoreSnapshotCacheLock = new object();

		public static void RestoreFromSnapshot(this AggregateRoot aggregateRoot, Snapshot snapshot)
		{
			var key = new ReflectionCacheKey(aggregateRoot, snapshot);
			LateBoundAction action;

			lock (_restoreSnapshotCacheLock)
			{
				if (!_restoreSnapshotCache.TryGetValue(key, out action))
				{
					MethodInfo method = aggregateRoot.GetType().GetMethod("RestoreFromSnapshot", bindingFlags, null, new[] { snapshot.GetType() }, null);
					action = DelegateHelper.CreateAction(method);
					_restoreSnapshotCache.Add(key, action);
				}
			}

			action(aggregateRoot, new object[] { snapshot });
		}



	}
}