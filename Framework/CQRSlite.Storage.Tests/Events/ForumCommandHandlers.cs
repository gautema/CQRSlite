using CQRSlite.Commands;
using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSlite.Storage.Tests.Events {
    public class ForumCommandHandlers : ICancellableCommandHandler<TestCreateForum>,
        ICancellableCommandHandler<TestAddPostToForum> {

        private ISession _session;
        public ForumCommandHandlers(ISession session) {
            _session = session;

        }


        public async Task Handle(TestCreateForum message, CancellationToken token = default) {
            var forum = new TestForum(message.Id,
                message.Name,
                message.Description,
                message.ForumGroupId,
                message.CanView,
                message.CanPost,
                message.CanModerate
                );

            await _session.Add(forum);
            await _session.Commit();
        }

        public async Task Handle(TestAddPostToForum post, CancellationToken token = default) {
            var forum = await _session.Get<TestForum>(post.ForumId, post.ExpectedVersion, token);
            

            forum.AddPost(post.Id, post.Poster, post.PosterRoles, post.Markup, post.Posted, post.Tags);

            await _session.Commit(token);
        }
    }
}
