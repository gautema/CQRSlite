#CQRSlite - the framework

CQRSlite is a small CQRS and Eventsourcing Framework. It is written in C# and targets .NET 4.0. CQRSlite originated as a CQRS sample project Greg Young and I did in the autumn of 2010.
This code is located at http://github.com/gregoryyoung/m-r

CQRSlite has been made with pluggability in mind. So every standard implementation should be interchangeable with a custom one if needed.

##Getting started

A sample project is located with the code, this shows a common usage scenario of the framework. There are some features of CQRSlite, such as snapshotting that the sample does not show. These features are only documented through the tests.

I have tried to keep the project running in .NET 4.0 and Mono 2.10. But the sample has some problems in Mono right now. The project should compile without any setup.

##License
This code is licensed under the Apache 2.0 license.

http://www.apache.org/licenses/LICENSE-2.0
