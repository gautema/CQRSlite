using CQRSlite.Domain;
using CQRSlite.Storage.Tests.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage.Tests {
    [SnapshotStrategy(1)]
    public class TestForum : Snapshotting.SnapshotAggregateRoot<TestForumSnapshot> {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Guid ForumGroupId { get; private set; }
        public string[] CanView { get; private set; }
        public string[] CanPost { get; private set; }
        public string[] CanModerate { get; private set; }

        private TestForum() { }
        public TestForum(Guid id,
            string name,
            string description,
            Guid forumGroupId,
            string[] canView,
            string[] canPost,
            string[] canModerate) {

            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentException("message", nameof(name));
            }

            if (string.IsNullOrEmpty(description)) {
                throw new ArgumentException("message", nameof(description));
            }

            if (canView is null) {
                throw new ArgumentNullException(nameof(canView));
            }

            if (canPost is null) {
                throw new ArgumentNullException(nameof(canPost));
            }

            if (canModerate is null) {
                throw new ArgumentNullException(nameof(canModerate));
            }

            Id = id;

            ApplyChange(new TestForumCreated(Id, name, description, forumGroupId, canView, canPost, canModerate));
        }

        private void Apply(TestForumCreated e) {
            this.Name = e.Name;
            this.Description = e.Description;
            this.ForumGroupId = e.ForumGroupId;
            this.CanView = e.CanView;
            this.CanPost = e.CanPost;
            this.CanModerate = e.CanModerate;
        }


        public void AddPost(Guid id,
            string poster,
            string[] PosterRoles,
            string Markup,
            DateTimeOffset posted,
            string[] tags) {

            // TODO: CanPost test

            TestPostAdded post = new TestPostAdded(this.Id,
                this.Id,
                poster,
                PosterRoles,
                Markup,
                posted,
                tags);

            ApplyChange(post);

        }
        public IList<TestPost> Posts { get; set; }
        private void Apply(TestPostAdded e) {
            if(this.Posts == null) {
                this.Posts = new List<TestPost>();
            }
            Posts.Add(new TestPost() {
                ForumId = this.Id,
                Id = e.Id,
                Markup = e.Markup,
                Posted = e.Posted,
                Poster = e.Poster,
                PosterRoles = e.PosterRoles,
                Tags = e.Tags,
                Number = (this.Posts.Count + 1)
            }) ;
   
        }

        protected override TestForumSnapshot CreateSnapshot() {
            return new TestForumSnapshot() {
                CanModerate = this.CanModerate,
                CanPost = this.CanPost,
                CanView = this.CanView,
                Description = this.Description,
                ForumGroupId = this.ForumGroupId,
                Id = this.Id,
                Name = this.Name,
                Posts = this.Posts,
                Version = this.Version
        
        
            };
        }

        protected override void RestoreFromSnapshot(TestForumSnapshot snapshot) {
            this.CanModerate = snapshot.CanModerate;
            this.CanPost = snapshot.CanPost;
            this.CanView = snapshot.CanView;
            this.Description = snapshot.Description;
            this.ForumGroupId = snapshot.ForumGroupId;
            this.Id = snapshot.Id;
            this.Name = snapshot.Name;
            this.Posts = snapshot.Posts;
            this.Version = snapshot.Version;
        }
    }
}
