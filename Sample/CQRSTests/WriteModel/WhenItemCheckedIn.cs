using System;
using System.Collections.Generic;
using System.Linq;
using CQRSCode.ReadModel.Events;
using CQRSCode.WriteModel.Commands;
using CQRSCode.WriteModel.Domain;
using CQRSCode.WriteModel.Handlers;
using CQRSlite.Contracts.Messages;
using CQRSlite.Extensions.TestHelpers;
using NUnit.Framework;

namespace CQRSTests.WriteModel
{
    public class When_item_checked_in : Specification<InventoryItem, InventoryCommandHandlers, CheckInItemsToInventory>
    {
        private Guid _guid;

        protected override InventoryCommandHandlers BuildHandler()
        {
            return new InventoryCommandHandlers(Session);
        }

        protected override IEnumerable<Event> Given()
        {
            _guid = Guid.NewGuid();
            return new List<Event> { new InventoryItemCreated(_guid, "Jadda"){Version = 1}, new ItemsCheckedInToInventory(_guid, 2){Version = 2} };
        }

        protected override CheckInItemsToInventory When()
        {
            return new CheckInItemsToInventory(_guid, 2, 2);
        }

        [Then]
        public void Should_create_one_event()
        {
            Assert.AreEqual(1, PublishedEvents.Count());
        }

        [Then]
        public void Should_create_correct_event()
        {
            Assert.IsInstanceOf<ItemsCheckedInToInventory>(PublishedEvents.First());
        }

        [Then]
        public void Should_save_name()
        {
            Assert.AreEqual(2,((ItemsCheckedInToInventory)PublishedEvents.First()).Count);
        }
    }
}
