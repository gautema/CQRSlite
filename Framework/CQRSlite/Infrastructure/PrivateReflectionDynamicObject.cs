using System;
using System.Dynamic;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    internal class PrivateReflectionDynamicObject : DynamicObject
    {
        public object RealObject { get; set; }
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static object WrapObjectIfNeeded(object o)
        {
            // Don't wrap primitive types, which don't have many interesting internal APIs
            if (o == null || o.GetType().IsPrimitive || o is string)
                return o;

            return new PrivateReflectionDynamicObject { RealObject = o };
        }

        // Called when a method is called
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

            // Wrap the sub object if necessary. This allows nested anonymous objects to work.
            result = WrapObjectIfNeeded(result);

            return true;
        }

        private static object InvokeMemberOnType(Type type, object target, string name, object[] args)
        {
            var argtypes = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
                argtypes[i] = args[i].GetType();
            while (true)
            {
                var member = type.GetMethod(name, bindingFlags, null, argtypes, null);
                if (member != null) return member.Invoke(target, args);
                if (type.BaseType == null) return null;
                type = type.BaseType;
            }
        }
    }
}