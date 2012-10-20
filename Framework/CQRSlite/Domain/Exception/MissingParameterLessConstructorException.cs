using System;

namespace CQRSlite.Domain.Exception
{
    public class MissingParameterLessConstructorException : System.Exception
    {
        public MissingParameterLessConstructorException(Type type)
            : base(string.Format("{0} has no constructor without paramerters. This can be either public or private", type.FullName))
        {
        }
    }
}