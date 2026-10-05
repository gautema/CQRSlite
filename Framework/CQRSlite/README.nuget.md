# CQRSlite

CQRSlite is a small framework for building applications with CQRS and event sourcing in C#. It gives you the building blocks (aggregates, a unit of work, repositories, a message router, snapshots and caching) and stays out of the way of everything else. Every part is behind an interface, so any of it can be replaced.

## Features

- Command sending, event publishing and queries, with handlers registered automatically
- A session that tracks aggregates as a unit of work
- A repository that loads aggregates from their events and saves new ones
- Optimistic concurrency checking
- Snapshots for aggregates with long histories
- Caching of aggregates between commands

You provide the event store for your database.

## Documentation

- [Concepts](https://github.com/gautema/cqrslite/blob/master/docs/concepts.md): CQRS and event sourcing, and how CQRSlite's pieces fit together
- [Tutorial](https://github.com/gautema/cqrslite/blob/master/docs/tutorial/1-overview.md): a walk through the sample application, from commands to read models
- [Guides](https://github.com/gautema/cqrslite/blob/master/docs/README.md#guides): aggregates, routing, event stores, snapshots and caching in depth
- [API reference](https://github.com/gautema/cqrslite/blob/master/docs/reference/api.md)

Source, sample and issues: https://github.com/gautema/cqrslite
