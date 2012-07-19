using System;
using CQRSlite.Domain.Exception;

namespace CQRSlite.Infrastructure
{
    internal static class AggregateActivator
    {
        public static T CreateAggregate<T>()
        {
            try
            {
               return (T)Activator.CreateInstance(typeof(T), true);
            }
            catch (MissingMethodException)
            {
                throw new MissingParameterLessConstructorException();
            }
        }
    }
}