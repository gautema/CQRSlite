using CQRSlite.Domain;
using CQRSlite.Snapshotting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CQRSlite.Storage {
    public class AttributeSnapshotStrategy : ISnapshotStrategy {

        public const string DEFAULT_SNAPSHOT_INTERVAL_KEY = "CQRSlite.Storage.DefaultSnapshotInterval";
        private int defaultSnapshotInterval = 100;
        public AttributeSnapshotStrategy(IConfiguration config) {
            if(config != null && !string.IsNullOrEmpty(config[DEFAULT_SNAPSHOT_INTERVAL_KEY])) {
                int.TryParse(config[DEFAULT_SNAPSHOT_INTERVAL_KEY], out defaultSnapshotInterval);
            }
        }

        public bool IsSnapshotable(Type aggregateType) {
            if (aggregateType.GetTypeInfo().BaseType == null)
                return false;
            if (aggregateType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                aggregateType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == typeof(SnapshotAggregateRoot<>))
                return true;
            return IsSnapshotable(aggregateType.GetTypeInfo().BaseType);
        }

        public bool ShouldMakeSnapShot(AggregateRoot aggregate) {
            int snapshotInterval = defaultSnapshotInterval;

            // Get instance of the attribute.
            SnapshotStrategyAttribute snapshotStrategy =
                (SnapshotStrategyAttribute)Attribute.GetCustomAttribute(aggregate.GetType(), typeof(SnapshotStrategyAttribute));

            if(snapshotStrategy != null) {
                snapshotInterval = snapshotStrategy.Interval;
            }

            if (!IsSnapshotable(aggregate.GetType()))
                return false;

            var i = aggregate.Version;
            for (var j = 0; j < aggregate.GetUncommittedChanges().Length; j++)
                if (++i % snapshotInterval == 0 && i != 0)
                    return true;
            return false;
        }
    }
}
