using CQRSlite.Domain;

namespace CQRSlite.Infrastructure
{
    internal static class PrivateReflectionDynamicObjectExtensions
    {
        public static dynamic AsDynamic(this AggregateRoot o)
        {
            return new PrivateReflectionDynamicObject { RealObject = o };
        }
    }
}
