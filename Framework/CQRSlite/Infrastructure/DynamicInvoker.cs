using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    internal static class DynamicInvoker
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly ConcurrentDictionary<int, CompiledMethodInfo> cachedMembers = new ConcurrentDictionary<int, CompiledMethodInfo>();

        internal static object Invoke<T>(this T obj, string methodname, bool exactMatch, params object[] args)
        {
            var type = obj.GetType();
            var hash = Hash(type,  methodname, args);
            var method = cachedMembers.GetOrAdd(hash, x =>
            {
                var argtypes = GetArgTypes(args);
                var m = GetMember(type, methodname, argtypes, exactMatch);
                return m == null ? null : new CompiledMethodInfo(m, type);
            });
            return method?.Invoke(obj, args);
        }

        private static int Hash(Type type, string methodname, object[] args)
        {
            var hash = 23;
            hash = hash * 31 + type.GetHashCode();
            hash = hash * 31 + methodname.GetHashCode();
            for (var index = 0; index < args.Length; index++)
            {
                var argtype = args[index].GetType();
                hash = hash * 31 + argtype.GetHashCode();
            }
            return hash;
        }

        private static Type[] GetArgTypes(object[] args)
        {
            var argtypes = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var argtype = args[i].GetType();
                argtypes[i] = argtype;
            }
            return argtypes;
        }

        private static MethodInfo GetMember(Type type, string name, Type[] argtypes, bool exactMatch)
        {
            while (true)
            {
                var member = type.GetMethods(bindingFlags)
                    .FirstOrDefault(m => m.Name == name && m.GetParameters().Select(p => p.ParameterType)
                                             .SequenceEqual(argtypes));
                if (member == null && !exactMatch)
                    member = type.GetMethods(bindingFlags)
                        .FirstOrDefault(m => m.Name == name && m.GetParameters().Select(p => p.ParameterType).ToArray()
                                                 .Matches(argtypes));
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

        private static bool Matches(this Type[] arr, Type[] args)
        {
            if (arr.Length != args.Length) return false;
            for (var i = 0; i < args.Length; i++)
            {
                if (!arr[i].IsAssignableFrom(args[i]))
                    return false;
            }
            return true;
        }
    }
}
