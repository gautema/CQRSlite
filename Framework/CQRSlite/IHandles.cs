namespace CQRSlite
{
    public interface IHandles<T> where T : Message
    {
        void Handle(T message);
    }
}