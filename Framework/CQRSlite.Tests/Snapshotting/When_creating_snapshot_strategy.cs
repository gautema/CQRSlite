using CQRSlite.Snapshotting;
using Xunit;

namespace CQRSlite.Tests.Snapshotting;

public class When_creating_snapshot_strategy
{
    [Fact]
    public void Should_not_allow_zero_interval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DefaultSnapshotStrategy(0));
    }
}
