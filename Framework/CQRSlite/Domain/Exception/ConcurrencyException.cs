using System;

namespace CQRSlite.Domain.Exception
{
    public class ConcurrencyException : System.Exception
    {
        public ConcurrencyException(IIdentity id)
            : base($"A different version than expected was found in aggregate {id}")
        { }
    }
}