# Tutorial 1: Overview

This tutorial walks through the sample application in the [`Sample`](../../Sample) folder, one layer at a time. By the end you will know every piece a CQRSlite application needs, and where your own code goes. It assumes you have read [Concepts](../concepts.md), or already know the basic ideas.

## What the sample does

The sample is a small inventory tracker. You can:

- create an item with a name
- check items in to stock, and remove them from it
- rename an item
- deactivate an item, which removes it from the list

It has one business rule worth enforcing: you can't remove more items than are in stock. Everything else is there to show how a change travels from a web form to the event store and on to the screens that display it.

## Running it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). From the repository root:

```
dotnet run --project Sample/CQRSWeb
```

Then open http://localhost:2225. Create an item, check some items in, and try to remove more than you have. That last request fails with an error page showing the rule that refused it.

The sample keeps everything in memory, so the data is gone when you stop it.

## How it is laid out

The sample is split into three projects:

```
Sample/
  CQRSCode/                 the application itself
    Events/                 what can happen: shared by both sides
    WriteModel/
      Commands/             what you can ask for
      Domain/               the InventoryItem aggregate and its rules
      Handlers/             command handlers
      InMemoryEventStore.cs where events are stored
    ReadModel/
      Dtos/                 the data the screens show
      Handlers/             event handlers that keep that data up to date,
                            and query handlers that return it
      Queries/              what you can ask about
      Infrastructure/       an in-memory read database
  CQRSWeb/                  ASP.NET Core MVC app: setup and controllers
  CQRSTest/                 tests for the write model
```

The split into `WriteModel` and `ReadModel` is the CQRS split from [Concepts](../concepts.md). The two sides share nothing except the events, which is why those live in a folder of their own. In a larger system the sides are often separate projects, or even separate services.

## The life of a change

Here is what happens when you check in five items. Each step is covered by a page of this tutorial.

1. The web app's controller creates a `CheckInItemsToInventory` command and sends it through the router ([part 5](5-web-app.md)).
2. The command handler loads the `InventoryItem` aggregate through the session, calls `CheckIn(5)`, and commits ([part 2](2-write-model.md)).
3. The aggregate checks its rules and records an `ItemsCheckedInToInventory` event ([part 2](2-write-model.md)).
4. The repository appends the new event to the event store, which publishes it ([part 3](3-event-store.md)).
5. Event handlers on the read side update the item's details and the list of items ([part 4](4-read-model.md)).
6. The browser is redirected to the list, which the controller fetches with a query ([part 4](4-read-model.md) and [part 5](5-web-app.md)).

Next: [the write model](2-write-model.md).
