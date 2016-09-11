using System;

namespace CQRSlite.Domain.Exception
{
    public class MissingParameterLessConstructorException : System.Exception
    {
        public MissingParameterLessConstructorException(Type type)
            : base($"{type.FullName} has no constructor without paramerters. This can be either public or private")
        { }
    }
}