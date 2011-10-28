namespace CQRSlite.Commanding
{
    public class Command : Message
    {
        public int ExpectedVersion { get; set; }
    }
}