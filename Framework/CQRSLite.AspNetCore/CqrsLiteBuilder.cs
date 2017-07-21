using CQRSlite.Bus;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using CQRSlite.Messages;
using CQRSlite.Config;

namespace CQRSLite.AspNetCore
{
    public class CqrsLiteBuilder
    {

        public CqrsLiteBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> services are attached to.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> services are attached to.
        /// </value>
        public IServiceCollection Services { get; private set; }

        private CqrsLiteBuilder AddScoped(Type serviceType, Type concreteType)
        {
            Services.AddScoped(serviceType, concreteType);

            return this;
        }

        private CqrsLiteBuilder AddSingleton(Type serviceType, Type concreteType)
        {
            Services.AddSingleton(serviceType, concreteType);

            return this;
        }

        public CqrsLiteBuilder AddEventStore(Type eventStoreType)
            => AddSingleton(typeof(IEventStore), eventStoreType);

        public CqrsLiteBuilder AddEventStore<T>() where T : IEventStore
            => AddEventStore(typeof(T));

        public CqrsLiteBuilder AddServiceLocator(Type serviceLocatorType)
     => AddSingleton(typeof(IServiceLocator), serviceLocatorType);

        public CqrsLiteBuilder AddServiceLocator<T>() where T : IServiceLocator
            => AddServiceLocator(typeof(T));

        public CqrsLiteBuilder AddCommandSender(Type commandSenderType)
             => AddScoped(typeof(ICommandSender), commandSenderType);

        public CqrsLiteBuilder AddCommandSender<T>() where T : ICommandSender
            => AddCommandSender(typeof(T));

        public CqrsLiteBuilder AddEventPublisher(Type eventPublisherType)
            => AddScoped(typeof(IEventPublisher), eventPublisherType);

        public CqrsLiteBuilder AddEventPublisher<T>() where T : IEventPublisher
            => AddEventPublisher(typeof(T));

        public CqrsLiteBuilder AddHandlerRegistrar(Type handlerRegistrarType)
            => AddScoped(typeof(IHandlerRegistrar), handlerRegistrarType);

        public CqrsLiteBuilder AddHandlerRegistrar<T>() where T : IHandlerRegistrar
            => AddHandlerRegistrar(typeof(T));

        public CqrsLiteBuilder AddSession(Type sessionType)
            => AddScoped(typeof(ISession), sessionType);

        public CqrsLiteBuilder AddSession<T>() where T : ISession
            => AddSession(typeof(T));

        public CqrsLiteBuilder RegisterCommandHandler(params Type [] typesFromAssembly)
        {
            ScanAssemblies(typesFromAssembly, typeof(ICancellableHandler<>));
            return this;
        }

        public CqrsLiteBuilder RegisterEventHandler(params Type[] typesFromAssembly)
        {
            ScanAssemblies(typesFromAssembly, typeof(IHandler<>));
            return this;
        }
        
        public CqrsLiteBuilder RegisterBus(params Type[] typesFromAssemblyContainingMessages)
        {
            return this;
        }

        private void ScanAssemblies(Type[] typesFromAssembly, Type typeToScan)
        {
            Services.Scan(scan => scan
               .FromAssembliesOf(typesFromAssembly)
                   .AddClasses(classes => classes.Where(x => {
                       var allInterfaces = x.GetInterfaces();
                       return
                           allInterfaces.Any(y => y.GetTypeInfo().IsGenericType && y.GetTypeInfo().GetGenericTypeDefinition() == typeToScan);
                   }))
                   .AsSelf()
                   .WithTransientLifetime()
           );
        }
    }
}
