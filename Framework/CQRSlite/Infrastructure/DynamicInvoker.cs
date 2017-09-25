using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            GetTypeAndHash(obj, methodname, args, exactMatch, out var type, out var hash);
            var method = cachedMembers.GetOrAdd(hash, x =>
            {
                var argtypes = GetArgTypes(args);
                var m = GetMember(type, methodname, argtypes, exactMatch);
                return m == null ? null : new CompiledMethodInfo(m, type);
            });
            return method?.Invoke(obj, args);
        }

        private static void GetTypeAndHash<T>(T obj, string methodname, IReadOnlyList<object> args, bool exactMatch,
            out Type type, out int hash)
        {
            type = obj.GetType();
            hash = 23;
            hash = hash * 31 + type.GetHashCode();
            hash = hash * 31 + methodname.GetHashCode();
            hash = hash * 31 + exactMatch.GetHashCode();
            for (var index = 0; index < args.Count; index++)
            {
                var t = args[index];
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

        private static MethodInfo GetMember(Type type, string name, Type[] argtypes, bool exactMatch)
        {
            while (true)
            {
                var methods = type.GetMethods(bindingFlags);
                var member = methods.FirstOrDefault(m =>
                                 m.Name == name && m.GetParameters().Select(p => p.ParameterType)
                                     .SequenceEqual(argtypes)) ??
                             methods.FirstOrDefault(m =>
                                 m.Name == name && m.GetParameters().Select(p => p.ParameterType).ToArray()
                                     .Matches(argtypes, exactMatch));
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

        private static bool Matches(this Type[] arr, Type[] args, bool exactMatch)
        {
            if (arr.Length != args.Length) return false;
            if (exactMatch) return false;
            for (var i = 0; i < args.Length; i++)
            {
                if (!arr[i].IsAssignableFrom(args[i]))
                    return false;
            }
            return true;
        }
    }
}
