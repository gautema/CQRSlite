using System.Collections.Concurrent;
using System.Reflection;

namespace CQRSlite.Infrastructure;

internal static class DynamicInvoker
{
    private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly ConcurrentDictionary<MethodKey, CompiledMethodInfo?> _cachedMembers = new();

    internal static object? Invoke(this object obj, string methodName, params object[] args)
    {
        var key = new MethodKey(obj.GetType(), methodName, GetArgTypes(args));
        var method = _cachedMembers.GetOrAdd(key, CreateMethod);
        return method?.Invoke(obj, args);
    }

    private static CompiledMethodInfo? CreateMethod(MethodKey key)
    {
        var m = GetMember(key.Type, key.Name, key.ArgTypes);
        return m == null ? null : new CompiledMethodInfo(m, key.Type);
    }

    private static Type[] GetArgTypes(object[] args)
    {
        var argTypes = new Type[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var argType = args[i].GetType();
            argTypes[i] = argType;
        }
        return argTypes;
    }

    private static MethodInfo? GetMember(Type type, string name, Type[] argtypes)
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
            var t = type.BaseType;
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

    /// <summary>
    /// Cache key for a method lookup. Compares the full identity, so methods whose
    /// hash codes collide still get separate cache entries.
    /// </summary>
    private readonly struct MethodKey(Type type, string name, Type[] argTypes) : IEquatable<MethodKey>
    {
        public Type Type { get; } = type;
        public string Name { get; } = name;
        public Type[] ArgTypes { get; } = argTypes;

        public bool Equals(MethodKey other) =>
            Type == other.Type && Name == other.Name && ArgTypes.SequenceEqual(other.ArgTypes);

        public override bool Equals(object? obj) => obj is MethodKey other && Equals(other);

        public override int GetHashCode()
        {
            var hash = 23;
            hash = hash * 31 + Type.GetHashCode();
            hash = hash * 31 + Name.GetHashCode();
            foreach (var argType in ArgTypes)
            {
                hash = hash * 31 + argType.GetHashCode();
            }
            return hash;
        }
    }
}
