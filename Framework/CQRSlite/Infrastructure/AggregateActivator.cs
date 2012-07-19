using System;
using CQRSlite.Domain.Exception;

namespace CQRSlite.Infrastructure
{
    public static class AggregateActivator
    {
        public static T CreateAggregate<T>()
        {
            T obj;
            try
            {
                obj = (T)Activator.CreateInstance(typeof(T), true);
            }
            catch (MissingMethodException)
            {
                throw new MissingParameterLessConstructorException();
            }
            return obj;
        }
    }
}