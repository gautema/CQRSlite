using System;

namespace CQRSlite.Routing.Exception
{
    public class ResolvedHandlerMethodNotFoundException : ArgumentNullException
    {
        public ResolvedHandlerMethodNotFoundException(string paramName)
            : base($"Could not execute Handle method on type {paramName}")
        {
        }
    }
}