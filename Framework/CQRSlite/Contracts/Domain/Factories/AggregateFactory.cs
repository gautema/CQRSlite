using System;
using CQRSlite.Contracts.Domain.Exception;

namespace CQRSlite.Contracts.Domain.Factories
{
    internal static class AggregateFactory
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