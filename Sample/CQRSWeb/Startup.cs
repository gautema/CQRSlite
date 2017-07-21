using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CQRSlite.Bus;
using CQRSlite.Commands;
using CQRSlite.Events;
using CQRSlite.Domain;
using CQRSCode.WriteModel;
using CQRSlite.Cache;
using CQRSCode.ReadModel;
using CQRSlite.Config;
using CQRSCode.WriteModel.Handlers;
using System.Reflection;
using System.Linq;
using CQRSlite.Messages;
using CQRSLite.AspNetCore;

namespace CQRSWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddCQRSLite()
                .AddEventStore<InMemoryEventStore>()
                .RegisterCommandHandler(typeof(InventoryCommandHandlers))
                .RegisterEventHandler(typeof(InventoryCommandHandlers));
            
            services.AddTransient<IReadModelFacade, ReadModelFacade>();
            
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseCQRSLiteBus(typeof(InventoryCommandHandlers));
        }
    }
}
