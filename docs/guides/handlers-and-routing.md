# Handlers and routing

Read this when you add handlers or set up the `Router` in your own application. It explains how a message finds its handler, and the rules around that. [The web app part of the tutorial](../tutorial/5-web-app.md) shows the sample's setup step by step.

## Handler interfaces

A handler is a class that implements one of these interfaces for a message type. One class can implement many of them, as the sample's `InventoryCommandHandlers` and `InventoryListView` do.

| Message | Interface | Method | Handlers per message |
| --- | --- | --- | --- |
| Command | `ICommandHandler<T>` | `Task Handle(T message)` | exactly one |
| Command | `ICancellableCommandHandler<T>` | `Task Handle(T message, CancellationToken token)` | exactly one |
| Event | `IEventHandler<T>` | `Task Handle(T message)` | any number |
| Event | `ICancellableEventHandler<T>` | `Task Handle(T message, CancellationToken token)` | any number |
| Query | `IQueryHandler<T, TResponse>` | `Task<TResponse> Handle(T query)` | exactly one |
| Query | `ICancellableQueryHandler<T, TResponse>` | `Task<TResponse> Handle(T message, CancellationToken token)` | exactly one |

Prefer the cancellable versions. They get the token passed to `Send`, `Publish` or `Query`. In ASP.NET Core that's usually the request's cancellation token, which a controller action gets as a `CancellationToken` parameter. Pass it on to the session and your database calls.

<!-- snippet: Sample/CQRSCode/WriteModel/Handlers/InventoryCommandHandlers.cs -->
```csharp
public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem>,
    ICancellableCommandHandler<DeactivateInventoryItem>,
    ICancellableCommandHandler<RemoveItemsFromInventory>,
    ICancellableCommandHandler<CheckInItemsToInventory>,
    ICancellableCommandHandler<RenameInventoryItem>
{
    private readonly ISession _session;

    public InventoryCommandHandlers(ISession session)
    {
        _session = session;
    }
    // ...
    public async Task Handle(RenameInventoryItem message, CancellationToken token)
    {
        var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
        item.ChangeName(message.NewName);
        await _session.Commit(token);
    }
}
```

Explicit interface implementations work too, if you would rather not have several public `Handle` methods.

## The router

`Router` is the in-process message bus. It implements all four messaging interfaces:

- `ICommandSender.Send`: runs the one handler for a command
- `IEventPublisher.Publish`: runs every handler for an event
- `IQueryProcessor.Query`: runs the one handler for a query and returns its result
- `IHandlerRegistrar.RegisterHandler`: adds a handler

Create one instance and register it under all of them, so everything shares the same routes:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
var router = new Router();
builder.Services.AddSingleton(router);
builder.Services.AddSingleton<ICommandSender>(router);
builder.Services.AddSingleton<IEventPublisher>(router);
builder.Services.AddSingleton<IHandlerRegistrar>(router);
builder.Services.AddSingleton<IQueryProcessor>(router);
```

## Registering handlers

`RouteRegistrar` finds handlers by reflection and registers them with the router:

```csharp
var registrar = new RouteRegistrar(requestServiceProvider);

// Every handler in the assemblies of these types. Each assembly is scanned once.
registrar.RegisterInAssemblyOf(typeof(CQRSCode.WriteModel.Handlers.InventoryCommandHandlers));

// Or only these handler classes
registrar.RegisterHandlers(typeof(CQRSCode.ReadModel.Handlers.InventoryListView));
```

It registers every non-abstract class that implements one of the interfaces above, once for each message type it handles. Do this once, at startup.

The registrar only records *which class* handles each message. The handler object itself is created when a message arrives, by asking the `IServiceProvider` you gave the registrar for that class. So every handler class must be registered in your DI container too. The sample does it with Scrutor:

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

Register handlers as transient. Register `ISession` and `IRepository` as scoped, so the handler, the session and the repository used by one request belong together.

## Resolving handlers per request

The service provider you give `RouteRegistrar` matters. In ASP.NET Core, `app.Services` is the *root* provider. Handlers resolved from it get scoped services like `ISession` from the root too. In Development, ASP.NET Core refuses with "Cannot resolve scoped service from root provider". In Production, it gives every request the same session, which mixes up aggregates from different requests.

The sample passes a small wrapper instead, which resolves from the current request's scope when there is one:

<!-- snippet: Sample/CQRSWeb/Program.cs -->
```csharp
new RouteRegistrar(new RequestServiceProvider(app.Services))
    .RegisterInAssemblyOf(typeof(InventoryCommandHandlers));
// ...
internal class RequestServiceProvider(IServiceProvider services) : IServiceProvider
{
    private readonly IHttpContextAccessor? _contextAccessor = services.GetService<IHttpContextAccessor>();

    public object? GetService(Type serviceType) =>
        _contextAccessor?.HttpContext?.RequestServices.GetService(serviceType) ??
        services.GetService(serviceType);
}
```

It needs `builder.Services.AddHttpContextAccessor()`. Copy it into your application as it is.

Event handlers that run because a command saved events are resolved the same way, so they run in the same request scope as the command.

Outside a web request, for example in a background service, there is no `HttpContext`, so the wrapper falls back to the root provider. If you send messages from such code, create a scope for each message (`IServiceScopeFactory.CreateScope()`) and make the wrapper find it, for example through an `AsyncLocal<IServiceProvider>` that your background code sets around each message.

## How messages are matched

- **By the message's exact runtime type.** A handler for a base class or an interface is not called for derived messages. If you want one handler for a family of events, implement the interface once per concrete event type.
- **Commands and queries need exactly one handler.** `Send` and `Query` throw `InvalidOperationException` when there is none ("No handler registered for ...") or more than one. A class that implements both `ICommandHandler<T>` and `ICancellableCommandHandler<T>` for the same command counts as two.
- **Events can have any number of handlers.** Publishing an event nobody handles does nothing.

## Publishing events

`Publish` starts the handlers for an event one after the other, then waits for all of them together. So:

- Asynchronous handlers run at the same time. Don't let two handlers for the same event update the same data.
- If a handler fails, `Publish` throws once all the handlers have finished. The others still run.
- A handler that throws synchronously stops the handlers after it from being started, and `Publish` throws straight away. An `async` method never does that, but a non-async method returning `Task`, or a handler class that can't be resolved, does.

When the event store publishes, a failing event handler makes the command that saved the events fail too, although the events are already stored. [Publishing events](event-store.md#publishing-events) in the event store guide explains this and what to do about it in production.

## Registering handlers by hand

You can also register a handler yourself, with a function that takes the message and a cancellation token. That's useful for a few handlers, for tests, or for handlers that aren't classes:

```csharp
// services: the RequestServiceProvider from the sample, so the handler gets the request's ISession
router.RegisterHandler<RenameInventoryItem>((command, token) =>
    services.GetRequiredService<InventoryCommandHandlers>().Handle(command, token));

// A handler can also be a plain function
router.RegisterHandler<InventoryItemCreated>((e, token) =>
{
    Console.WriteLine($"Created {e.Name}");
    return Task.CompletedTask;
});
```

## Errors

These come from routing itself:

- `InvalidOperationException` from `Send` or `Query`: no handler, or more than one, for that message type.
- `HandlerNotResolvedException`: the service provider returned nothing for a handler class (or for `IHandlerRegistrar` when you created the `RouteRegistrar`). Register the class in DI.
- `ResolvedHandlerMethodNotFoundException`: the resolved object had no `Handle` method for the message. That happens if the provider returns a different type than the one registered.

Exceptions thrown by handlers reach whoever called `Send`, `Publish` or `Query`. In a web application, map the ones that are about the user's request to a proper response, rather than a 500:

```csharp
public class ItemsController(ICommandSender commandSender) : ControllerBase
{
    [HttpPost("items/{id}/name")]
    public async Task<IActionResult> Rename(Guid id, string name, int version, CancellationToken cancellationToken)
    {
        try
        {
            await commandSender.Send(new RenameInventoryItem(id, name, version), cancellationToken);
            return NoContent();
        }
        catch (ConcurrencyException)
        {
            return Conflict("The item was changed by someone else. Reload it and try again.");
        }
        catch (AggregateNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}
```

`ConcurrencyException` means the aggregate changed since the version the user was looking at. Retrying the same command with the same expected version will fail again. Show the user the new state instead. Retry automatically only for commands that don't depend on what the user saw: load the latest version, apply the command again, and give up after a few attempts.
