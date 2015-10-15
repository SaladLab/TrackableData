[![Build status](https://ci.appveyor.com/api/projects/status/qylsoqv4k5ra4fmf?svg=true)](https://ci.appveyor.com/project/veblush/trackabledata)

# TrackableData

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
## POCO

You already saw this in Example.

## Dictionary

Source
```csharp
var dict = new TrackableDictionary<int, string>();
dict.SetDefaultTracker();

dict.Add(1, "One");
dict.Add(2, "Two");
dict.Add(3, "Three");

var json = JsonConvert.SerializeObject(dict.Tracker, JsonSerializerSettings);
Console.WriteLine(json);
dict.Tracker.Clear();

dict.Remove(1);
dict[2] = "TwoTwo";
dict.Add(4, "Four");

var json2 = JsonConvert.SerializeObject(dict.Tracker, JsonSerializerSettings);
Console.WriteLine(json2);
```

Output
```
{ +1:One, +2:Two, +3:Three }
{ -1:One, =2:Two->TwoTwo, +4:Four }
```

## List

Source
```csharp
var list = new TrackableList<string>();
list.SetDefaultTracker();

list.Add("One");
list.Add("Two");
list.Add("Three");

var json = JsonConvert.SerializeObject(list.Tracker, JsonSerializerSettings);
Console.WriteLine(json);
list.Tracker.Clear();

list.RemoveAt(0);
list[1] = "TwoTwo";
list.Add("Four");

var json2 = JsonConvert.SerializeObject(list.Tracker, JsonSerializerSettings);
Console.WriteLine(json2);
```

Output
```
[ +0:One, +1:Two, +2:Three ]
[ -0:One, =1:Three=>TwoTwo, +2:Four ]
```

# Pluging
 

## Json.NET

TODO

## ProtocolBuffer.NET

TODO

## MSSQL

TODO
Poco, Dictionary. Not List.

## MongoDB

TODO: Implementation
