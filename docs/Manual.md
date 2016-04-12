# Manual

## Basic

There are trackable and tracker. Trackable uses tracker to write down
its changes. For example TrackableSet uses TrackableSetTracker like this:

```csharp
var set = new TrackableSet<int>() { 1, 2, 3 };
set.SetDefaultTracker();                // attach TrackableSetTracker to set

set.Remove(2);                          // make changes and set write down these
set.Add(4);                             // changes to TrackableSetTracker

Console.WriteLine(set.Tracker);         // show changes written to Tracker
                                        // { -2, +4 }
```

## Types

POCO, dictionary, list, set and container are supported.

### POCO

Tracking POCO is implemented with code generation. For example if you want
to design Person containing Name and Age. First you need to write interface
IPerson inheriting ITrackablePoco\<IPerson\>. Code generator detects this
signature and generates tracking code for the sake of you.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    string Name { get; set; }
    int Age { get; set; }
}
```

CodeGenerator writes a trackable class for IPerson.

```csharp
public partial class TrackablePerson : IPerson {
    public IPocoTracker<IPerson> Tracker { get; set; }
    public string Name { ... }
    public int Age { ... }
}
```

You can use TrackablePerson for tracking person data.


```csharp
var person = new TrackablePerson { Name = "Alice", Age = 10 };
person.SetDefaultTracker();             // attach tracker to person

person.Name = "Bob";                    // make changes
person.Age = 20;

Console.WriteLine(person.Tracker);      // show changes
                                        // { Name:Alice->Bob, Age:10->20 }
```


### Dictionary

TrackableDictionary traces add, modify and remove. Key and value should be immutable.

Source

```csharp
var dict = new TrackableDictionary<int, string>()
dict.Add(1, "One");
dict.Add(2, "Two");
dict.Add(3, "Three");

dict.SetDefaultTracker();               // attach tracker to dictionary

dict.Remove(1);                         // make changes
dict[2] = "TwoTwo";
dict.Add(4, "Four");

Console.WriteLine(dict.Tracker);        // show changes
                                        // { -1:One, =2:Two->TwoTwo, +4:Four }
```

### List

TrackableList traces add, modify and remove. Also it can detect push and pop at
front and back of list. This special actions for List could be used for
saving data to list collection of Redis and MongoDB which lacks random
access options. Value should be immutable.

```csharp
var list = new TrackableList<string>() { "One", "Two", "Three" };
list.SetDefaultTracker();               // attach tracker to set

list.RemoveAt(0);                       // make changes
list[1] = "TwoTwo";
list.Add("Four");

Console.WriteLine(list.Tracker);        // show changes
                                        // [ -0:One, =1:Three=>TwoTwo, +2:Four ]
```

### Set

TrackableSet traces add and remove. Value should be immutable.

```csharp
var set = new TrackableSet<int>() { 1, 2, 3 };
set.SetDefaultTracker();                // attach tracker to set

set.Remove(2);                          // make changes
set.Add(4);

Console.WriteLine(set.Tracker);         // show changes
                                        // { -2, +4 }
```

### Container

Container is collection of other trackable data. For example following interface
defines IDataContainer containing of POCO Person, Dictionary and List.
Like POCO, container needs code generation.

```csharp
public interface IDataContainer : ITrackableContainer<IDataContainer> {
    TrackablePerson Person { get; set; }
    TrackableDictionary<int, string> Dictionary { get; set; }
    TrackableList<string> List { get; set; }
}
```

Code generator generates Trackable and Tracker class for IDataContainer.

```csharp
public partial class TrackableDataContainer : IDataContainer {
    public TrackableDataContainerTracker Tracker { ... }
    public TrackablePerson Person { ... }
    public TrackableDictionary<int, string> Dictionary { ... }
    public TrackableList<string> List { ... }
}

public class TrackableDataContainerTracker : IContainerTracker<IDataContainer> {
    public TrackablePocoTracker<IPerson> PersonTracker { ... }
    public TrackableDictionaryTracker<int, string> DictionaryTracker { ... }
    public TrackableListTracker<string> ListTracker { get; set; } { ... }
}
```

Use it.

```csharp
var c = CreateTestContainerWithTracker();

c.Person.Name = "Bob";                  // make changes
c.Person.Age = 30;

c.Dictionary[1] = "OneModified";
c.Dictionary.Remove(2);
c.Dictionary[4] = "FourAdded";

c.List[0] = "OneModified";
c.List.RemoveAt(1);
c.List.Insert(1, "TwoInserted");

Console.WriteLine(c.Tracker);           // show changes
                                        // { TODO }
```

## Operations

### Tracker

### Apply changes to Trackable

### Apply changes to Tracker

### Rollback

### Tracker.HasChangeSet
