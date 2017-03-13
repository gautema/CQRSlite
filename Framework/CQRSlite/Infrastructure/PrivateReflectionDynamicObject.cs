using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    internal class PrivateReflectionDynamicObject : DynamicObject
    {
        public object RealObject { get; set; }
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Called when a method is called
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeMemberOnType(RealObject.GetType(), RealObject, binder.Name, args);

            // Wrap the sub object. This allows nested anonymous objects to work.
            result = new PrivateReflectionDynamicObject { RealObject = result }; 

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
                //TODO: change when .net core implements GetMethod correct to get access to private methods
                var member = type.GetMethods(bindingFlags).FirstOrDefault(m => m.Name == name && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(argtypes));
                //var member = type.GetMethod(name, bindingFlags, argtypes);

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
