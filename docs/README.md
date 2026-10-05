# CQRSlite documentation

CQRSlite is a small framework for building applications with CQRS and event sourcing in C#. It gives you the plumbing (aggregates, a unit of work, a repository, a message router) and leaves the decisions that depend on your system, such as where events are stored and how read models are kept, to you.

## Where to start

If CQRS or event sourcing is new to you, read [Concepts](concepts.md) first. It explains the ideas and how CQRSlite's pieces fit together, in about ten minutes.

Then follow the [tutorial](tutorial/1-overview.md). It walks through the sample application in this repository, an inventory tracker, one layer at a time:

1. [Overview](tutorial/1-overview.md): what the sample does and how it is laid out
2. [The write model](tutorial/2-write-model.md): commands, events, aggregates and command handlers
3. [The event store](tutorial/3-event-store.md): where the events go
4. [The read model](tutorial/4-read-model.md): turning events into data you can query
5. [The web app](tutorial/5-web-app.md): wiring it all together in ASP.NET Core
6. [Testing](tutorial/6-testing.md): testing behaviour as given, when, then
7. [Taking it to production](tutorial/7-production.md): what the sample leaves out, and what to do about it

## Guides

Each guide covers one topic in depth, for when you are building your own application:

- [Aggregates](guides/aggregates.md): the rules CQRSlite expects aggregates to follow
- [Handlers and routing](guides/handlers-and-routing.md): how messages find their handlers
- [Implementing an event store](guides/event-store.md): storing events safely in a real database
- [Snapshots](guides/snapshots.md): loading aggregates with long histories quickly
- [Caching](guides/caching.md): keeping aggregates in memory between commands

## Reference

The [API reference](reference/api.md) lists every public type and member.

## Installing

```
dotnet add package CqrsLite
```

CQRSlite targets .NET 10 and .NET Standard 2.0, so it also runs on .NET Framework 4.6.2 and later.
