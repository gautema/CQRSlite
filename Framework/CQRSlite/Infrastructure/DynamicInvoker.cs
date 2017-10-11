using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQRSlite.Infrastructure
{
    internal static class DynamicInvoker
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static volatile Dictionary<int, CompiledMethodInfo> _cachedMembers = new Dictionary<int, CompiledMethodInfo>();
        private static readonly object _lockObj = new object();

        internal static object Invoke<T>(this T obj, string methodname, params object[] args)
        {
            var type = obj.GetType();
            var hash = Hash(type,  methodname, args);
            var exists = _cachedMembers.TryGetValue(hash, out var method);
            if (exists) return method?.Invoke(obj, args);
            lock (_lockObj)
            {
                //Recheck if exist inside lock in case another thread has added it.
                exists = _cachedMembers.TryGetValue(hash, out method);
                var dict = new Dictionary<int, CompiledMethodInfo>(_cachedMembers);
                if (exists) return method?.Invoke(obj, args);

                var argtypes = GetArgTypes(args);
                var m = GetMember(type, methodname, argtypes);
                method = m == null ? null : new CompiledMethodInfo(m, type);

                dict.Add(hash, method);
                _cachedMembers = dict;
                return method?.Invoke(obj, args);
            }
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

        private static MethodInfo GetMember(Type type, string name, Type[] argtypes)
        {
            while (true)
            {
                var methods = type.GetMethods(bindingFlags).Where(m => m.Name == name).ToArray();
                var member = methods.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(argtypes)) ??
                             methods.FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).ToArray().Matches(argtypes));

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
