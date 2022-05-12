using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SnapshotStrategyAttribute : Attribute {
        public int Interval {get;private set;}
        public SnapshotStrategyAttribute(int interval) {
            Interval = interval;
        }
    }
}
