using CQRSlite.Domain.Exception;
using System;

namespace CQRSlite.Domain.Factories
{
    using System.Linq;
    using System.Reflection;

    internal static class AggregateFactory
    {
        public static T CreateAggregate<T>()
        {
            try
            {
                var ctorInfo=typeof(T).GetTypeInfo().DeclaredConstructors.FirstOrDefault(x=>x.GetParameters().Length==0);
                if (ctorInfo == null) throw new MissingMemberException();
                return (T) ctorInfo.Invoke(null);
                //return (T)Activator.CreateInstance(typeof(T));
            }
            catch (System.MissingMemberException)
            {
                throw new MissingParameterLessConstructorException(typeof(T));
            }
        }
    }
}