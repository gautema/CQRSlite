# Tutorial 5: The web app

The last piece is the application that receives requests and turns them into commands and queries. In the sample it is an ASP.NET Core MVC app, but nothing in CQRSlite depends on that: the same setup works for a web API, a worker service or a console app.

## Setting up the services

All the wiring is in [`Sample/CQRSWeb/Program.cs`](../../Sample/CQRSWeb/Program.cs). It starts with the router:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
```

`Router` plays four roles, so it is registered under four interfaces. Your code sends commands through `ICommandSender`, runs queries through `IQueryProcessor`, and the event store publishes through `IEventPublisher`. `IHandlerRegistrar` is what handlers are registered with, below. One router instance serves the whole application.

Next come the event store, the repository and the session:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddScoped<IRepository>(sp =>
{
    var eventStore = sp.GetRequiredService<IEventStore>();
    return new CacheRepository(new Repository(eventStore), eventStore, sp.GetRequiredService<ICache>());
});
builder.Services.AddScoped<ISession, Session>();
```

- The event store is a singleton here because the in-memory one holds the data. A database-backed store can be scoped or singleton depending on how it manages connections.
- The repository is a `Repository` wrapped in a `CacheRepository`. Repositories are decorators: each takes another repository and adds something to it. The [caching](../guides/caching.md) and [snapshot](../guides/snapshots.md) guides show the options.
- The session is scoped, so each web request gets its own unit of work.

Then the handlers themselves are registered, so that they can be created with their dependencies (such as `ISession`) injected. The sample uses [Scrutor](https://github.com/khellang/Scrutor) to find every handler class instead of listing them:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<InventoryCommandHandlers>()
    .AddClasses(classes => classes.AssignableToAny(
        typeof(IHandler<>),
        typeof(ICancellableHandler<>),
        typeof(IQueryHandler<,>),
        typeof(ICancellableQueryHandler<,>)))
    .AsSelf()
    .WithTransientLifetime());
```

`IHandler<>` and `ICancellableHandler<>` are the base interfaces of all command and event handlers. Without Scrutor, you would register each handler class with `builder.Services.AddTransient<InventoryCommandHandlers>()` and so on.

## Connecting messages to handlers

The router doesn't know about your handlers until you tell it. `RouteRegistrar` does that by scanning an assembly for every class that implements a handler interface, and registering each `Handle` method with the router:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
var app = builder.Build();

// Route messages to the handlers, resolving them from the current request scope
new RouteRegistrar(new RequestServiceProvider(app.Services))
    .RegisterInAssemblyOf(typeof(InventoryCommandHandlers));
```

The registrar doesn't create handlers up front. Instead, each time a message arrives, it asks the service provider you gave it for a fresh handler. That is why the provider matters. A command handler depends on `ISession`, which is scoped to the web request, and the handler must get the *same* session as the rest of that request. The root `app.Services` provider can't give it that; it would refuse to create a scoped service at all in development, and share one session between every request in production.

So the sample passes a small wrapper that uses the current request's services when there is a request:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
internal class RequestServiceProvider(IServiceProvider services) : IServiceProvider
{
    private readonly IHttpContextAccessor? _contextAccessor = services.GetService<IHttpContextAccessor>();

    public object? GetService(Type serviceType) =>
        _contextAccessor?.HttpContext?.RequestServices.GetService(serviceType) ??
        services.GetService(serviceType);
}
```

It relies on `IHttpContextAccessor`, which is why `Program.cs` also calls `builder.Services.AddHttpContextAccessor()`. Copy this class into your own application as it is. The [handlers and routing guide](../guides/handlers-and-routing.md) has more on registration.

## Sending commands and queries

With that in place, the rest of the application only needs `ICommandSender` and `IQueryProcessor`. Here are the controller actions for checking items in:

<!-- snippet: Sample/CQRSWeb/Controllers/HomeController.cs -->
```csharp
public async Task<ActionResult> CheckIn(Guid id)
{
    ViewData.Model = await _queryProcessor.Query(new GetInventoryItemDetails(id));
    return View();
}

[HttpPost]
public async Task<ActionResult> CheckIn(Guid id, int number, int version, CancellationToken cancellationToken)
{
    await _commandSender.Send(new CheckInItemsToInventory(id, number, version), cancellationToken);
    return RedirectToAction("Index");
}
```

The first action shows the form. It queries the details read model and passes it to the view, which keeps the item's id and version in hidden fields:

<!-- snippet: Sample/CQRSWeb/Views/Home/CheckIn.cshtml -->
```cshtml
@Html.Hidden("Id",Model.Id)
@Html.Hidden("Version",Model.Version)
Number: @Html.TextBox("Number")<br />
```

The second action receives the form and sends a command. The version from the hidden field becomes the command's `ExpectedVersion`. This is how the [concurrency check](../concepts.md#versions-and-concurrency) works end to end: the version comes from the read model the user was looking at, and if anyone has changed the item since then, the command fails instead of overwriting their change.

`Send` returns when the command has been handled, and throws if the handler throws. The sample lets those exceptions become error pages. A real application would catch `ConcurrencyException` and tell the user that the item changed while they were looking at it, and turn rule violations into validation messages.

## That's the whole loop

You have now seen every piece of the sample: a form, a command, a handler, an aggregate, an event, the event store, an event handler, a read model, and a query that reads it back. Every CQRSlite application has the same pieces; yours will just have more of them.

Next: [testing](6-testing.md).
