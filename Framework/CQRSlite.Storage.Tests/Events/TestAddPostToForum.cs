using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage.Tests.Events {
    public class TestAddPostToForum : ICommand {
        public TestAddPostToForum(Guid id,
            Guid forumId,
            string poster,
            string[] PosterRoles,
            string Markup,
            DateTimeOffset posted,
            string[] tags,
            int originalVersion) {
            Id = id;
            ForumId = forumId;
            Poster = poster;
            this.PosterRoles = PosterRoles;
            this.Markup = Markup;
            Posted = posted;
            Tags = tags;
            ExpectedVersion = originalVersion;
        }

        public Guid Id { get; set; }
        public Guid ForumId { get; set; }
        public string Poster { get; set; }
        public string[] PosterRoles { get; set; }
        public string Markup { get; set; }
        public DateTimeOffset Posted { get; set; }
        public string[] Tags { get; set; }
        public int ExpectedVersion { get; set; }
    }
}
