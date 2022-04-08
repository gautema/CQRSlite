using System;
using System.Linq;

namespace CQRSlite.Infrastructure
{
    internal static class InterfaceMap
    {
        internal static string GetImplementationNameOfInterfaceMethod(this Type implementation, Type @interface,
            string methodName, params Type[] argtypes)
        {
            var map = implementation.GetInterfaceMap(@interface);
            var method = map.InterfaceMethods.Single(x =>
                x.Name == methodName && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(argtypes));
            var index = Array.IndexOf(map.InterfaceMethods, method);
            return map.TargetMethods[index].Name;
        }
    }
}
