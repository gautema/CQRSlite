namespace SimpleCQRS.Interfaces
{
    public interface IHandles<T>
    {
        void Handle(T message);
    }
}