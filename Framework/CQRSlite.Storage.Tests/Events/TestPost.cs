using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage.Tests.Events {
    public class TestPost {

        public Guid Id { get; set; }
        public Guid ForumId { get; set; }
        public string Poster { get; set; }
        public string[] PosterRoles { get; set; }
        public string Markup { get; set; }
        public DateTimeOffset Posted { get; set; }
        public string[] Tags { get; set; }
        public int Number { get; set; }
    }
}
