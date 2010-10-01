namespace SimpleCQRS
{
    public interface IHandles<T>
    {
        void Handle(T message);
    }
}