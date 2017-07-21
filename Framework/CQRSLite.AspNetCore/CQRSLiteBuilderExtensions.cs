using CQRSlite.Config;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace CQRSLite.AspNetCore
{
    public static class CQRSLiteBuilderExtensions
    {
        public static IApplicationBuilder UseCQRSLiteBus(this IApplicationBuilder builder, params Type[] typesFromAssemblyContainingMessages)
        {

            var registrar = new BusRegistrar(builder.ApplicationServices.GetService<IServiceLocator>());
            registrar.Register(typesFromAssemblyContainingMessages);
            return builder;
        }
    }
}
