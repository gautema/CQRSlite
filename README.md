# CQRSlite

[![Build Status](https://ci.appveyor.com/api/projects/status/github/gautema/CQRSLite?branch=master&svg=true)](https://ci.appveyor.com/project/gautema/CQRSLite)
[![NuGet](https://img.shields.io/nuget/vpre/cqrslite.svg)](https://www.nuget.org/packages/cqrslite)

## The framework
CQRSlite is a small CQRS and Eventsourcing Framework. It is written in C# and targets .NET 4.5.2 and .NET Core. CQRSlite originated as a CQRS sample project Greg Young and I did in the autumn of 2010.
This code is located at http://github.com/gregoryyoung/m-r

CQRSlite has been made with pluggability in mind. So every standard implementation should be interchangeable with a custom one if needed.

## Getting started
A sample project is located with the code, this shows a common usage scenario of the framework. There are some features of CQRSlite, such as snapshotting that the sample does not show. These features are only documented through the tests.

Great introductions to CQRS and CQRSlite have been written by others. Here are two recommended:
* <https://www.codeproject.com/articles/991648/cqrs-a-cross-examination-of-how-it-works>
* <https://exceptionnotfound.net/real-world-cqrs-es-with-asp-net-and-redis-part-1-overview/>

## Features
* Command sending and event publishing
* Unit of work through session with aggregate tracking
* Repository for getting and saving aggregates
* Optimistic concurrency checking
* Message router with auto registration of handlers
* Snapshotting
* Caching with concurrency checks and updating to latest version

## Installation
To install CQRSlite, it you could either download the files and copy whats needed into your project, you can clone this project and reference it, or you can download releases from nuget. You will have to implement your own eventstore or use an exsiting eventstore with an adapter to fit the IEventStore interface.

## License
Copyright 2020 Gaute Magnussen

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
