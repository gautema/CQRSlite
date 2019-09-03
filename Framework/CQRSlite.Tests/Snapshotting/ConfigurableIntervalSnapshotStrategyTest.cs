using System;
using System.Collections.Generic;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Snapshotting;
using Xunit;

namespace CQRSlite.Tests.Snapshotting
{
    public class ConfigurableIntervalSnapshotStrategyTest
    {
        private readonly Guid guid = Guid.NewGuid();

        [Fact]
        public void IsSnapshotable_ShouldReturnFalse_WhenTypeIsNotSnapshotable()
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(5);

            // act
            var result = sut.IsSnapshotable(typeof(NotSnapshotableOnlyAggregateRootClass));

            // assert
            Assert.False(result);
        }

        [Fact]
        public void IsSnapshotable_ShouldReturnTrue_WhenTypeIsSnapshotable()
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(5);

            // act
            var result = sut.IsSnapshotable(typeof(SnapshotableClass));

            // assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldMakeSnapShot_ShouldReturnFalse_WhenAggregateHasNoUnsavedEvents()
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(1);
            var aggregate = new SnapshotableClass(Guid.NewGuid(), 23);

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldMakeSnapShot_ShouldReturnTrue_WhenAggregateHasSingleUnsavedEventsAndNewVersionIsMultipleOfInterval()
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(5);
            var aggregate = new SnapshotableClass(guid, 4, CreateDummyEvent(guid, 5));

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void ShouldMakeSnapShot_ShouldReturnFalse_WhenAggregateHasSingleUnsavedEventsAndNewVersionIsNotMultipleOfInterval(int currentAggregateVersion)
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(5);
            var aggregate = new SnapshotableClass(guid, currentAggregateVersion, CreateDummyEvent(guid, currentAggregateVersion + 1));

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(1, 1, false)]
        [InlineData(2, 1, false)]
        [InlineData(3, 1, false)]
        [InlineData(4, 1, true)]
        [InlineData(5, 1, false)]
        [InlineData(6, 1, false)]
        [InlineData(1, 3, false)]
        [InlineData(1, 4, true)]
        [InlineData(1, 5, true)]
        [InlineData(1, 6, true)]
        [InlineData(1, 7, true)]
        [InlineData(3, 2, true)]
        [InlineData(3, 4, true)]
        public void ShouldMakeSnapShot_ShouldReturnExpectedValue(int currentAggregateVersion, int numberOfEvents, bool expectedShouldMakeSnapshot)
        {
            // arrange
            var sut = new ConfigurableIntervalSnapshotStrategy(5);
            var events = new List<IEvent>();
            for (var i = 0; i < numberOfEvents; i++)
            {
                events.Add(CreateDummyEvent(guid, currentAggregateVersion + i));
            }

            var aggregate = new SnapshotableClass(guid, currentAggregateVersion, events.ToArray());

            // act
            var result = sut.ShouldMakeSnapShot(aggregate);

            // assert
            Assert.Equal(expectedShouldMakeSnapshot, result);
        }

        private DummyEvent CreateDummyEvent(Guid id, int version)
        {
            return new DummyEvent
            {
                Id = id,
                Version = version
            };
        }

        private class DummyEvent : IEvent
        {
            public DummyEvent()
            {
            }

            public Guid Id { get; set; }

            public int Version { get; set; }

            public DateTimeOffset TimeStamp { get; set; }
        }

        private class NotSnapshotableOnlyAggregateRootClass : AggregateRoot
        {
        }

        private class SnapshotableClass : SnapshotAggregateRoot<SnapshotableClassSnapshot>
        {
            public SnapshotableClass(Guid newGuid, int version, params IEvent[] events)
            {
                Id = newGuid;
                Version = version;
                foreach (var e in events)
                    ApplyChange(e);
            }

            protected override SnapshotableClassSnapshot CreateSnapshot()
            {
                return new SnapshotableClassSnapshot();
            }

            protected override void RestoreFromSnapshot(SnapshotableClassSnapshot snapshot)
            {
            }
        }

        private class SnapshotableClassSnapshot : Snapshot
        {
        }
    }
}