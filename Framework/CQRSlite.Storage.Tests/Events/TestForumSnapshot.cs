using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage.Tests.Events {
    public class TestForumSnapshot : Snapshotting.Snapshot {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ForumGroupId { get; set; }
        public string[] CanView { get; set; }
        public string[] CanPost { get; set; }
        public string[] CanModerate { get; set; }
        public IList<TestPost> Posts { get; set; }
    }
}
