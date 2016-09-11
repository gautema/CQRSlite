namespace CQRSlite.Infrastructure
{
    internal static class PrivateReflectionDynamicObjectExtensions
    {
        public static dynamic AsDynamic(this object o)
        {
            return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o);

            //dynamic d = o;

            //return d;
        }
    }
}