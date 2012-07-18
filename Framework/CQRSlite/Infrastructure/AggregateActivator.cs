using System;
using System.Runtime.Serialization;

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
                obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
            }
            return obj;
        }
    }
}