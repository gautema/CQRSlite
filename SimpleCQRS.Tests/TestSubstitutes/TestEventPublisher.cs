using SimpleCQRS.Eventing;

namespace SimpleCQRS.Tests.TestSubstitutes
{
    public class TestEventPublisher: IEventPublisher {
        public void Publish<T>(T @event) where T : Event
        {
            
        }
    }
}