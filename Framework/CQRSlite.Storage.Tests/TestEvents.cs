using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Storage.Tests.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CQRSlite.Storage.Tests {

    
    [TestClass]
    public class TestEvents {

        const string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        [DataTestMethod]
        [DataRow("Announcements",
            "View important announcements from the game operators",
            "87097683-97e3-449d-b82c-210331884858",
            new string[] { "All" },
            new string[] { "Owners" },
            new string[] { "Moderators" }
            )]
        [DataRow("General",
            "View general announcements from the game operators",
            "bc0fdc6c-db79-4930-9813-c8df338a5f1a",
            new string[] { "All" },
            new string[] { "Owners" },
            new string[] { "Moderators" }
            )]
        [DataRow("Bugs",
            "View general announcements from the game operators",
            "55704416-cd80-49d3-8fe9-7ed4abbe1088",
            new string[] { "All" },
            new string[] { "Owners" },
            new string[] { "Moderators" }
            )]

        public void TestForums(string name, string description, string forumGroupStringId, string[] CanView, string[] CanPost, string[] CanModerate) {

            ISession session = (ISession)TestStartup.GetServiceProvider().GetService(typeof(ISession));

            ICommandSender cmd = (ICommandSender)TestStartup.GetServiceProvider().GetService(typeof(ICommandSender));

            Guid forumGroupId = Guid.Parse(forumGroupStringId);
            Guid id = Guid.NewGuid();
            cmd.Send<TestCreateForum>(new TestCreateForum(id,
                name,
                description,
                forumGroupId,
                CanView,
                CanPost,
                CanModerate)
            ).Wait();



            TestForum f = session.Get<TestForum>(id).Result;
//Assert.AreEqual<string[]>(f.CanModerate, CanModerate);
            //Assert.AreEqual(f.CanPost, CanPost);
            //Assert.AreEqual(f.CanView, CanView);
            Assert.AreEqual(f.Description, description);
            Assert.AreEqual(f.ForumGroupId, forumGroupId);
            Assert.AreEqual(f.Id, id);
            Assert.AreEqual(f.Name, name);
            int posts = 100;
            
            for (int postnum = 0; postnum < posts; postnum++) {
                f = session.Get<TestForum>(id).Result;
                cmd.Send<TestAddPostToForum>(new TestAddPostToForum(
                    Guid.NewGuid(),
                    f.Id,
                    "pm@focopmile.com",
                    CanPost,
                    String.Join(Environment.NewLine, Enumerable.Repeat(LoremIpsum, 40)),
                    DateTime.Now,
                    new string[] { "hi", "there" },
                    f.Version

                )).Wait();

            }


            f = session.Get<TestForum>(id).Result;
            Assert.AreEqual(f.Posts.Count, posts);

        }
    }
}
