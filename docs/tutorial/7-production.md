# Tutorial 7: Taking it to production

The sample shows every piece of a CQRSlite application, but it cuts corners that a real system can't. This page goes through them, and through the questions every event-sourced system runs into sooner or later. None of them need answering on day one, but it helps to know they are coming.

## A real event store

The sample's event store lives in memory and loses everything on restart. A real one needs a database, and the [event store guide](../guides/event-store.md) shows how to build one. The essentials:

- Store each event with its aggregate id, version, a type name and the serialized data.
- Make the database refuse a second event with the same aggregate id and version, with a unique key. This is what keeps concurrent writes safe; the repository's own check is not enough on its own.
- Return an aggregate's events in version order.

You can also build on an existing event store database, or on a document database such as PostgreSQL with JSON columns. CQRSlite only needs the two methods of `IEventStore`.

## Publishing events reliably

The sample's event store stores events and then publishes them. If the process dies between those two steps, the events are saved but the read side never hears about them, and nothing will notice.

The usual fix is the *outbox* pattern: in the same database transaction that stores the events, also record them as not yet published. A background process publishes recorded events and marks them done. Nothing is lost, because storing and recording happen together or not at all.

Another approach is to let read models follow the event store directly: each read model remembers the position of the last event it processed and asks the store for anything newer. Several event store databases have this built in as *subscriptions*.

Either way, events are delivered *at least once*, so an event handler can see the same event twice, after a crash or a retry. Make handlers safe to repeat. The sample's details read model already has what it needs for that: it stores the version of the last event it applied, so a handler can skip any event whose version isn't newer.

## Read models that lag behind

Once events are delivered asynchronously, a query straight after a command may not include that command's change yet. See [eventual consistency](../concepts.md#eventual-consistency). Common ways to handle it in a UI:

- Show the outcome of the user's own command without re-querying, for example "5 items checked in", since you know it succeeded.
- After a command, wait until the read model has reached the version the command produced before showing it.
- Accept a short delay where it doesn't matter, such as in reports.

## Rebuilding read models

A read model can be thrown away and rebuilt from the events at any time: when it has a bug, when its schema changes, or when you add a new one. To do that you replay every stored event, in order, through its event handlers.

`IEventStore` only reads events for one aggregate, so rebuilding needs one more thing from your store: a way to read all events, across all aggregates, in the order they were stored. Plan for it when you design the events table, for example with a global sequence number.

Keep event handlers free of side effects such as sending emails, or put those in separate handlers that you don't run during a rebuild. Otherwise a rebuild will send every email again.

## Changing events

Events are kept forever, so the code that reads them has to keep working with old ones.

- Adding a property is safe. Old events simply don't have it, so give it a sensible default.
- Renaming or removing a property, or changing what it means, is not. Add a new event type instead, and keep handling the old one.
- Event type names are stored with the events, so renaming or moving an event class breaks loading. Store a name you control, such as a type map from fixed strings to classes, rather than the .NET type name.
- If old events really must change shape, convert them while reading ("upcasting"), so the code only ever sees the current version.

## Personal data

Events can't be changed or deleted, which clashes with a user's right to have their data removed. Decide early how you will handle it. The common choices are to keep personal data out of events altogether, storing it elsewhere and referencing it by id, or to encrypt it with a key per person and delete the key when asked.

## Errors users will see

The sample turns every exception into an error page. A real application should handle the expected ones:

- `ConcurrencyException`: the item changed while the user was looking at it. Show them the current state and let them try again. For commands that don't depend on what the user saw, you can instead retry automatically.
- Rule violations thrown by aggregates: show them as validation messages. Many teams define their own exception type for these so they can be told apart from bugs.
- `AggregateNotFoundException`: the command referred to something that doesn't exist.

Cheap checks that don't depend on state, such as required fields or formats, can be done before a command is sent at all. Rules that depend on the aggregate's state belong in the aggregate.

## Running more than one instance

The router is in-process: it delivers messages to handlers in the same application. With several instances behind a load balancer, each one sends commands to its own handlers, and the event store's version check keeps them from overwriting each other. What changes is event delivery. If read models are updated by in-process event handlers, only the instance that saved the events runs them, which is fine as long as the read models live in a shared database. Once you have an outbox or subscriptions, as described above, that question goes away.

If you use [caching](../guides/caching.md), each instance has its own cache. That is safe, since a cached aggregate is brought up to date from the event store before use, but it means the cache helps less as you add instances.

## Performance

Loading an aggregate means replaying its events. For most aggregates that is a handful of events and costs next to nothing. If some aggregates build up long histories:

- [Snapshots](../guides/snapshots.md) store an aggregate's state every so many events, so loading only replays what came after.
- [Caching](../guides/caching.md) keeps recently used aggregates in memory, so loading only replays events stored since the last use.

Often the better answer is to look at the aggregate's boundaries instead: an aggregate that collects thousands of events is often several aggregates in disguise.

That's the end of the tutorial. From here, the [guides](../README.md#guides) cover each part in depth, and the [API reference](../reference/api.md) lists every type.
