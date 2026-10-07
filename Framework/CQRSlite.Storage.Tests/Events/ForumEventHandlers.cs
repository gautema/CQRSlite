using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQRSlite.Storage.Tests.Events {
    public class ForumEventHandlers : IEventHandler<TestForumCreated> {
        public Task Handle(TestForumCreated message) {
            return Task.CompletedTask;
        }
    }
}
