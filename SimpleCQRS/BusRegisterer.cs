using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using SimpleCQRS.Interfaces;

namespace SimpleCQRS
{
    public class BusRegisterer
    {
        public void Register(IServiceLocator serviceLocator, params Type[] typesFromAssemblyContainingMessages)
        {
            var bus = serviceLocator.GetInstance<IHandleRegister>();
            
            foreach (var typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
            {
                Assembly executorsAssembly = typesFromAssemblyContainingMessage.Assembly;
                var executorTypes = executorsAssembly
                    .GetTypes()
                    .Select(t => new { Type = t, Interfaces = ResolveMessageHandlerInterface(t) })
                    .Where(e => e.Interfaces != null && e.Interfaces.Count() > 0);
                
                foreach (var executorType in executorTypes)
                {
                    object executorInstance = serviceLocator.GetInstance(executorType.Type);
                    foreach (var @interface in executorType.Interfaces)
                    {
                        InvokeHandler(@interface, bus, executorInstance);
                    }

                }
            }

        }

        private void InvokeHandler(Type @interface, IHandleRegister bus, object executorInstance) {
            Type commandType = @interface.GetGenericArguments()[0];

            MethodInfo registerExecutorMethod = bus
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.Name == "RegisterHandler")
                .Where(mi => mi.IsGenericMethod)
                .Where(mi => mi.GetGenericArguments().Count() == 1)
                .Where(mi => mi.GetParameters().Count() == 1)
                .Single()
                .MakeGenericMethod(commandType);

            var inmethod = executorInstance
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(mi => mi.Name == "Handle")
                .Where(mi => mi.GetParameters().Count() == 1)
                .Where(mi => mi.GetParameters().First().ParameterType == commandType)
                .Single();

            var action = typeof(Action<>).MakeGenericType(commandType);
            var del = Delegate.CreateDelegate(action, executorInstance, inmethod);

            registerExecutorMethod.Invoke(bus, new[] { del });
        }

        private static IEnumerable<Type> ResolveMessageHandlerInterface(Type type)
        {
            return type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandles<>));
        }

    }
}
