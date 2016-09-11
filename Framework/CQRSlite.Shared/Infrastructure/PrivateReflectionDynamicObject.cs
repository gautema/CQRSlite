using System;
using System.Dynamic;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    using System.Linq;

    internal class PrivateReflectionDynamicObject : DynamicObject
    {
        public object RealObject { get; set; }
        //private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static object WrapObjectIfNeeded(object o)
        {
            // Don't wrap primitive types, which don't have many interesting internal APIs
            if (o == null || o.GetType().GetTypeInfo().IsPrimitive || o is string)
            {
                return o;
            }

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
            {
                argtypes[i] = args[i].GetType();
            }
            while (true)
            {
                MethodInfo member = null;
                foreach (
                    var mi in
                        type.GetTypeInfo()
                            .DeclaredMethods.Where(x => x.Name == name && x.GetParameters().Length == argtypes.Length))
                {
                    member = mi;
                    var parameterInfos = member.GetParameters();

                    for (int i = 0; i < argtypes.Length; i++)
                    {
                        if (argtypes[i] != parameterInfos[i].ParameterType)
                        {
                            member = null;
                            break;
                        }
                    }
                    if (member != null) break;
                }

                if (member != null)
                {
                    return member.Invoke(target, args);
                }
                if (type.GetTypeInfo().BaseType == null)
                {
                    return null;
                }
                type = type.GetTypeInfo().BaseType;
            }
        }
    }
}