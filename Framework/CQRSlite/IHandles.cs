namespace CQRSlite
{
    public interface IHandles<T>
    {
        void Handle(T message);
    }
}