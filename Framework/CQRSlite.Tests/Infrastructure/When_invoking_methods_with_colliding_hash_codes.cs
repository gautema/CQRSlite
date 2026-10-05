using System.Reflection;
using System.Reflection.Emit;
using CQRSlite.Infrastructure;
using Xunit;

namespace CQRSlite.Tests.Infrastructure;

public class When_invoking_methods_with_colliding_hash_codes
{
    private readonly string _firstName;
    private readonly string _secondName;

    public When_invoking_methods_with_colliding_hash_codes()
    {
        (_firstName, _secondName) = FindNamesWithSameHashCode();
    }

    [Fact]
    public void Should_invoke_the_requested_method()
    {
        var instance = CreateInstanceWithMethods(_firstName, _secondName);

        Assert.Equal(_firstName, instance.Invoke(_firstName));
        Assert.Equal(_secondName, instance.Invoke(_secondName));
    }

    [Fact]
    public void Should_not_hide_existing_method_behind_missing_one()
    {
        var instance = CreateInstanceWithMethods(_secondName);

        Assert.Null(instance.Invoke(_firstName));
        Assert.Equal(_secondName, instance.Invoke(_secondName));
    }

    // string hash codes are randomized per process, so search for a pair at runtime.
    // A 32-bit birthday collision needs about 77 000 tries on average.
    private static (string, string) FindNamesWithSameHashCode()
    {
        var seen = new Dictionary<int, string>();
        for (var i = 0; ; i++)
        {
            var name = "Method" + i;
            if (seen.TryGetValue(name.GetHashCode(), out var existing))
                return (existing, name);
            seen.Add(name.GetHashCode(), name);
        }
    }

    // Each method returns its own name
    private static object CreateInstanceWithMethods(params string[] methodNames)
    {
        var assemblyName = new AssemblyName("CollidingMethods" + Guid.NewGuid().ToString("N"));
        var type = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
            .DefineDynamicModule(assemblyName.Name!)
            .DefineType("CollidingMethods", TypeAttributes.Public);
        foreach (var methodName in methodNames)
        {
            var il = type.DefineMethod(methodName, MethodAttributes.Public, typeof(string), Type.EmptyTypes).GetILGenerator();
            il.Emit(OpCodes.Ldstr, methodName);
            il.Emit(OpCodes.Ret);
        }
        return Activator.CreateInstance(type.CreateType())!;
    }
}
