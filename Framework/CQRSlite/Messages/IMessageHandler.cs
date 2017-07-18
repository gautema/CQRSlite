using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    public interface IMessageHandler<in T> where T : IMessage
    {
    }
}