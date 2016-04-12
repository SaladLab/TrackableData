# TrackableData

[![NuGet Status](http://img.shields.io/nuget/v/TrackableData.svg?style=flat)](https://www.nuget.org/packages/TrackableData/)
[![Build status](https://ci.appveyor.com/api/projects/status/qylsoqv4k5ra4fmf?svg=true)](https://ci.appveyor.com/project/veblush/trackabledata)
[![Coverage Status](https://coveralls.io/repos/github/SaladLab/TrackableData/badge.svg?branch=master)](https://coveralls.io/github/SaladLab/TrackableData?branch=master)
[![Coverity Status](https://scan.coverity.com/projects/8371/badge.svg?flat=1)](https://scan.coverity.com/projects/saladlab-trackabledata)

TrackableData provides a way to track the changes of poco, list, set and dictionary. It's small and simple to use. For example:

```csharp
var u = new TrackableUserData();     // create UserData can track changes
u.SetDefaultTracker();               // set Tracker to track changes of UserData

u.Name = "Bob";                      // make changes
u.Level = 1;
u.Gold = 10;

Console.WriteLine(u.Tracker);        // watch what has changed via Tracker
                                     // { Name:->Bob, Level:0->1, Gold:0->10 }

u.Tracker.Clear();                   // clear all changes

u.Level += 10;                       // make another changes
u.Gold += 100;

Console.WriteLine(u.Tracker);        // watch what has changed via Tracker
                                     // { Level:1->11, Gold:10->110 }
```

## Where can I get it?

```
PM> Install-Package TrackableData
```

TrackableData uses compile-time code generation to track poco and user
container. So if you want use it, install TrackableData.Templates too.

```
PM> Install-Package TrackableData.Templates
```

## Why do you make another trackable library?

There are many good libraries for tracking data.
Most of them are ORM libraries like EntityFramework and NHibernate.
They provides several good-to-have features and affordable performance.
But just for tracking data it seems a little bit big.

This library has been developed for two goals.

#### Lean library.

It works only for tracking data and doesn't aim at being versatile ORM.
Because of that, it can be kept small and simple and also provides 
additional features like rollback and merge of changes. 
   
#### Support Unity3D (.NET Framework 3.5)

Unity3D only supports .NET Framework 3.5 until now. And dynamic code generation
is forbidden to support iOS and WebGL which uses IL2CPP.
Core library and transfer plugins are written under this limitation.

## Manual

Comprehensive manual for using TrackableData: [Manual](./docs/Manual.md)

## Plugins

TrackableData itself just tracks changes. For more jobs, plugins are required.
There are two categories for plugins.

### Transfer Changes

Common serialization library works for transfering changes. But more readable
and optimized representation can be achieved with help of plugin.

- Json: [TrackableData.Json](./docs/Json.md)
- Protocol Buffer: [TrackbleData.Protobuf](./docs/Protobuf.md)

### Save Changes

For data persistency, storage plugin is essential.

- Microsoft SQL Server: [TrackableData.MsSql](./docs/Sql.md)
- MySQL: [TrackableData.MySql](./docs/Sql.md)
- PostgreSQL: [TrackableData.PostgreSql](./docs/Sql.md)
- MongoDB: [TrackableData.MongoDB](./docs/MongoDB.md)
- Redis: [TrackableData.Redis](./docs/Redis.md)

