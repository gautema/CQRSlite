using CQRSlite.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQRSlite.Storage.Tests.Events {
    public class TestCreateForum : ICommand {


        public TestCreateForum(Guid id, string name, string description, Guid forumGroupId, string[] CanView, string[] CanPost, string[] CanModerate) {

            this.Id = id;
            this.Name = name;
            this.Description = description;
            ForumGroupId = forumGroupId;
            this.CanView = CanView;
            this.CanPost = CanPost;
            this.CanModerate = CanModerate;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Guid ForumGroupId { get; set; }
        public string[] CanView { get; set; }
        public string[] CanPost { get; set; }
        public string[] CanModerate { get; set; }
        public Guid Id { get; set; }
    }
}