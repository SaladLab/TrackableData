# TrackableData

[![NuGet Status](http://img.shields.io/nuget/v/TrackableData.svg?style=flat)](https://www.nuget.org/packages/TrackableData/)
[![Build status](https://ci.appveyor.com/api/projects/status/qylsoqv4k5ra4fmf?svg=true)](https://ci.appveyor.com/project/veblush/trackabledata)
[![Coverage Status](https://coveralls.io/repos/github/SaladLab/TrackableData/badge.svg?branch=master)](https://coveralls.io/github/SaladLab/TrackableData?branch=master)
[![Coverity Status](https://scan.coverity.com/projects/8371/badge.svg?flat=1)](https://scan.coverity.com/projects/saladlab-trackabledata)

Simple library to track changes of poco, list and dictionary.

## Example

Source
```csharp
var u = new TrackableUserData();
u.SetDefaultTracker();

u.Name = "Bob";
u.Level = 1;
u.Gold = 10;

Console.WriteLine(u.Tracker);
u.Tracker.Clear();

u.Level += 10;
u.Gold += 100;

Console.WriteLine(u.Tracker);
```

Output
```
{ Name:->Bob, Level:0->1, Gold:0->10 }
{ Level:1->11, Gold:10->110 }
```

## Why do you make another trackable library?

There are several trackable libraries including famous entity framework. But primary reasons for making new library is

 - Lean Library
   - Minimal library.
   - Easy to understand and extend.
   
 - Support Unity3D (.NET Framework 3.5)
   - Unity only supports .NET Framework 3.5
   - Common libraries support only .NET Framework 4.0 and above.

# Manual

## Types supported

### POCO

TrackablePoco traces property set.

You already saw this in Example.

### Dictionary

TrackableDictionary traces add, modify and remove.

Source
```csharp
var dict = new TrackableDictionary<int, string>();
dict.SetDefaultTracker();

dict.Add(1, "One");
dict.Add(2, "Two");
dict.Add(3, "Three");

Console.WriteLine(dict.Tracker);
dict.Tracker.Clear();

dict.Remove(1);
dict[2] = "TwoTwo";
dict.Add(4, "Four");

Console.WriteLine(dict.Tracker);
```

Output
```
{ +1:One, +2:Two, +3:Three }
{ -1:One, =2:Two->TwoTwo, +4:Four }
```

### List

TrackableList traces add, modify and remove.

Source
```csharp
var list = new TrackableList<string>();
list.SetDefaultTracker();

list.Add("One");
list.Add("Two");
list.Add("Three");

Console.WriteLine(list.Tracker);
list.Tracker.Clear();

list.RemoveAt(0);
list[1] = "TwoTwo";
list.Add("Four");

```

Output
```
[ +0:One, +1:Two, +2:Three ]
[ -0:One, =1:Three=>TwoTwo, +2:Four ]
```

## Etc

### Tracker

TODO

### Nested Tracker

TODO

## Pluging

### Json.NET

TODO

### ProtocolBuffer.NET

TODO

### MSSQL

TODO
Poco, Dictionary. Not List.

### MongoDB

TODO: Implementation
