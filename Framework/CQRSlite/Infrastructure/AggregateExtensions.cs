using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    internal static class AggregateExtensions
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly ConcurrentDictionary<int, CompiledMethodInfo> cachedMembers = new ConcurrentDictionary<int, CompiledMethodInfo>();

        internal static object Invoke<T>(this T root, string methodname, params object[] args)
        {
            GetTypeAndHash(root, methodname, args, out var type, out var hash);
            var method = cachedMembers.GetOrAdd(hash, x =>
            {
                var argtypes = GetArgTypes(args);
                var m = GetMember(type, methodname, argtypes);
                return m == null ? null : new CompiledMethodInfo(m, type);
            });
            return method?.Invoke(root, args);
        }

        private static void GetTypeAndHash<T>(T root, string methodname, object[] args, 
            out Type type, out int hash)
        {
            type = root.GetType();
            hash = 23;
            hash = hash * 31 + type.GetHashCode();
            hash = hash * 31 + methodname.GetHashCode();
            foreach (var t in args)
            {
                var argtype = t.GetType();
                hash = hash * 31 + argtype.GetHashCode();
            }
        }

        private static Type[] GetArgTypes(IReadOnlyList<object> args)
        {
            var argtypes = new Type[args.Count];
            for (var i = 0; i < args.Count; i++)
            {
                var argtype = args[i].GetType();
                argtypes[i] = argtype;
            }
            return argtypes;
        }

        private static MethodInfo GetMember(Type type, string name, Type[] argtypes)
        {
            while (true)
            {
                var member = type.GetMethods(bindingFlags)
                    .FirstOrDefault(m => m.Name == name && m.GetParameters().Select(p => p.ParameterType)
                                             .SequenceEqual(argtypes));

                if (member != null)
                {
                    return member;
                }
                var t = type.GetTypeInfo().BaseType;
                if (t == null)
                {
                    return null;
                }
                type = t;
            }
        }
    }
}
