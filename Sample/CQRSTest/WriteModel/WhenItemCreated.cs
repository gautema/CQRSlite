using System;
using System.Collections.Generic;
using System.Linq;
using CQRSCode.ReadModel.Events;
using CQRSCode.WriteModel.Commands;
using CQRSCode.WriteModel.Domain;
using CQRSCode.WriteModel.Handlers;
using CQRSlite.Events;
using CQRSlite.Tests.Extensions.TestHelpers;
using Xunit;

namespace CQRSTests.WriteModel
{
    public class WhenItemCreated : Specification<InventoryItem, InventoryCommandHandlers, CreateInventoryItem>
    {
        private Guid _id;
        protected override InventoryCommandHandlers BuildHandler()
        {
            return new InventoryCommandHandlers(Session);
        }

        protected override IEnumerable<IEvent> Given()
        {
            _id = Guid.NewGuid();
            return new List<IEvent>();
        }

        protected override CreateInventoryItem When()
        {
            return new CreateInventoryItem(_id, "myname");
        }

        [Then]
        public void Should_create_one_event()
        {
            Assert.Equal(1, PublishedEvents.Count);
        }

        [Then]
        public void Should_create_correct_event()
        {
            Assert.IsType<InventoryItemCreated>(PublishedEvents.First());
        }

        [Then]
        public void Should_save_name()
        {
            Assert.Equal("myname", ((InventoryItemCreated)PublishedEvents.First()).Name);
        }
    }
}