using CQRSlite.Domain.Exception;
using System;
using System.Linq.Expressions;

namespace CQRSlite.Domain.Factories
{
    internal static class AggregateFactory<T>
    {
        private static Func<T> _constructor;

        private static volatile object _lock = new object();

        private static Func<T> Constructor
        {
            get
            {
                if (_constructor == null)
                {
                    lock (_lock)
                    {
                        if (_constructor != null) return _constructor;

                        var newExpr = Expression.New(typeof(T));
                        var func = Expression.Lambda<Func<T>>(newExpr);
                        _constructor = func.Compile();
                    }
                }

                return _constructor;
            }
        }

        public static T CreateAggregate()
        {
            try
            {
                return Constructor();
            }
            catch (ArgumentException)
            {
                throw new MissingParameterLessConstructorException(typeof(T));
            }
        }
    }
}