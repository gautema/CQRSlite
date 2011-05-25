using System;
using System.Collections.Generic;
using System.Linq;
using CQRSCode.CommandHandlers;
using CQRSCode.Commands;
using CQRSCode.Domain;
using CQRSCode.Events;
using CQRSlite.Eventing;
using CQRSlite.Extensions.TestHelpers;
using NUnit.Framework;

namespace CQRSTests
{
	[TestFixture]
    public class WhenItemCheckedIn : Specification<InventoryItem, InventoryCommandHandlers, CheckInItemsToInventory>
    {
        protected override InventoryCommandHandlers BuildHandler()
        {
            return new InventoryCommandHandlers(Repository);
        }

        protected override IEnumerable<Event> Given()
        {
            return new List<Event> { new InventoryItemCreated(Guid.Empty, "Jadda"), new ItemsCheckedInToInventory(Guid.Empty, 2) };
        }

        protected override CheckInItemsToInventory When()
        {
            return new CheckInItemsToInventory(Guid.Empty, 2, 1);
        }

        [Then]
        public void ShouldCreateOneEvent()
        {
            Assert.AreEqual(1, PublishedEvents.Count());
        }

        [Then]
        public void ShouldCreateCorrectEvent()
        {
            Assert.IsInstanceOf<ItemsCheckedInToInventory>(PublishedEvents.First());
        }

        [Then]
        public void ShouldSaveName()
        {
            Assert.AreEqual(2,((ItemsCheckedInToInventory)PublishedEvents.First()).Count);
        }
    }
}
