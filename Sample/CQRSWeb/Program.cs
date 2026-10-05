using CQRSCode.WriteModel;
using CQRSCode.WriteModel.Handlers;
using CQRSlite.Caching;
using CQRSlite.Commands;
using CQRSlite.Domain;
using CQRSlite.Events;
using CQRSlite.Messages;
using CQRSlite.Queries;
using CQRSlite.Routing;
using ISession = CQRSlite.Domain.ISession;

var builder = WebApplication.CreateBuilder(args);

// Add CQRSlite services
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    return new CacheRepository(new Repository(eventStore), eventStore, sp.GetRequiredService<ICache>());
});
builder.Services.AddScoped<ISession, Session>();

// Scan for command, event and query handlers
builder.Services.Scan(scan => scan
    .FromAssemblyOf<InventoryCommandHandlers>()
    .AddClasses(classes => classes.AssignableToAny(
        typeof(IHandler<>),
        typeof(ICancellableHandler<>),
        typeof(IQueryHandler<,>),
        typeof(ICancellableQueryHandler<,>)))
    .AsSelf()
    .WithTransientLifetime());

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Route messages to the handlers, resolving them from the current request scope
new RouteRegistrar(new RequestServiceProvider(app.Services))
    .RegisterInAssemblyOf(typeof(InventoryCommandHandlers));

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

/// <summary>
/// Resolves services from the current request scope when there is one, so scoped
/// services like ISession are shared between a request and the handlers it triggers.
/// </summary>
internal class RequestServiceProvider(IServiceProvider services) : IServiceProvider
{
    private readonly IHttpContextAccessor? _contextAccessor = services.GetService<IHttpContextAccessor>();

    public object? GetService(Type serviceType) =>
        _contextAccessor?.HttpContext?.RequestServices.GetService(serviceType) ??
        services.GetService(serviceType);
}
