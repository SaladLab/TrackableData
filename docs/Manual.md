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
