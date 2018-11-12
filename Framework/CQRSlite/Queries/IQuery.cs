using CQRSlite.Messages;

namespace CQRSlite.Queries
{
    /// <summary>
    /// Defines a query.
    /// </summary>
    public interface IQuery<TReturn> : IMessage
    {
    }
}