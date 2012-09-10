using CQRSlite.Commanding;

namespace CQRSlite.Handlers
{
	public interface HandlesCommand<T> : Handles<T> where T : Command
	{
	}
}