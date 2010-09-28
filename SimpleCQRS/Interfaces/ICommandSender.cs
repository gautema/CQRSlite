namespace SimpleCQRS.Interfaces
{
    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;

    }
}