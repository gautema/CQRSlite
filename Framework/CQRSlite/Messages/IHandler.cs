using System.Threading.Tasks;

namespace CQRSlite.Messages
{
    public interface IHandler<in T> where T : IMessage
    {
        Task Handle(T message);
    }
}