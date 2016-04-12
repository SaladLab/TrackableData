# Manual

## Basic

There are trackable and tracker. Trackable uses tracker to write down
its changes. For example TrackableSet uses TrackableSetTracker like this:

```csharp
var set = new TrackableSet<int>() { 1, 2, 3 };
set.SetDefaultTracker();                    // attach TrackableSetTracker to set

set.Remove(2);                              // make changes and set write down these
set.Add(4);                                 // changes to TrackableSetTracker

Console.WriteLine(set.Tracker);             // show changes written to Tracker: { -2, +4 } 
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
person.SetDefaultTracker();                 // attach tracker to person

person.Name = "Bob";                        // make changes
person.Age = 20;

Console.WriteLine(person.Tracker);          // show changes
                                            // { Name:Alice->Bob, Age:10->20 }
```


### Dictionary

TrackableDictionary traces add, modify and remove. Key and value should be immutable.

Source

```csharp
var dict = new TrackableDictionary<int, string>() {
    { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
dict.SetDefaultTracker();                   // attach tracker to dictionary

dict.Remove(1);                             // make changes
dict[2] = "TwoTwo";
dict.Add(4, "Four");

Console.WriteLine(dict.Tracker);            // show changes
                                            // { -1:One, =2:Two->TwoTwo, +4:Four }
```

### List

TrackableList traces add, modify and remove. Also it can detect push and pop at
front and back of list. This special actions for List could be used for
saving data to list collection of Redis and MongoDB which lacks random
access options. Value should be immutable.

```csharp
var list = new TrackableList<string>() { "One", "Two", "Three" };
list.SetDefaultTracker();                   // attach tracker to set

list.RemoveAt(0);                           // make changes
list[1] = "TwoTwo";
list.Add("Four");

Console.WriteLine(list.Tracker);            // show changes
                                            // [ -0:One, =1:Three=>TwoTwo, +2:Four ]
```

### Set

TrackableSet traces add and remove. Value should be immutable.

```csharp
var set = new TrackableSet<int>() { 1, 2, 3 };
set.SetDefaultTracker();                    // attach tracker to set

set.Remove(2);                              // make changes
set.Add(4);

Console.WriteLine(set.Tracker);             // show changes: { -2, +4 }
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

c.Person.Name = "Bob";                      // make changes
c.Person.Age = 30;

c.Dictionary[1] = "OneModified";
c.Dictionary.Remove(2);
c.Dictionary[4] = "FourAdded";

c.List[0] = "OneModified";
c.List.RemoveAt(1);
c.List.Insert(1, "TwoInserted");

Console.WriteLine(c.Tracker);               // show changes
                                            // { TODO }
```

## Operations

### Tracker

Trackable data can have tracker or not. When tracking is not needed,
Tracker property of trackable data could be null. In this case all change are
still applied to trackable data itself but changes are not written to tracker.

```csharp
var set = new TrackableSet<int>();
set.SetDefaultTracker();                    // attach tracker to set

set.Add(2);                                 // make changes
set.Add(4);

Console.WriteLine(set.Tracker);             // show changes: { +2, +4 }

var tracker = set.Tracker;
set.Tracker = null;                         // turn off tracking

set.Add(3);                                 // make changes

Console.WriteLine(tracker);                 // show changes on tracker: { +2, +4 }
Console.WriteLine(set);                     // show set itself: { 2, 3, 4 }
```

### Apply changes to other data

Changes which tracker has can be applied to other trackable. With this feature,
change can be propagated to other trackable data.

```csharp
var set = new TrackableSet<int>();
set.SetDefaultTracker();                    // attach tracker to set

set.Add(1);                                 // make changes
set.Add(2);

var set2 = new TrackableSet<int>();         // create new TrackableSet
set.Tracker.ApplyTo(set2);                  // apply changes to this set

Console.WriteLine(set2);                    // show new TrackableSet: { 1, 2 }
```

This feature is essential for multi tier application. And changes can be applied
to not only trackable data but also normal data.

```csharp
var set2 = new HashSet<int>();              // create new HashSet
set.Tracker.ApplyTo(set2);                  // apply changes to this set

Console.WriteLine(set2);                    // show new set: { 1, 2 }
```

### Apply changes to Tracker

Changes which track can be applied to other tracker. With this feature,
changes that multiple trackers hold are easily merged to one tracker. 

```csharp
var set = new TrackableSet<int>();
set.SetDefaultTracker();                    // attach tracker to set

set.Add(1);                                 // make changes
set.Add(2);

Console.WriteLine(set2);                    // show changes: { +1, +2 }

var tracker1 = set.Tracker;
set.SetDefaultTracker();                    // create new tracker and attach it

set.Add(3);                                 // make other changes

Console.WriteLine(set2.Tracker);            // show other changes: { +3 }

set.Tracker.ApplyTo(tracker1);              // apply other changes to tracker1
Console.WriteLine(tracker1);                // show merged changes: { +1, +2, +3 }
```

### Revert changes

When change happens, a tracker holds old values of changes. For example,
TrackableDictionary keeps old values when an entity is removed or modified.
With these values, every changes that tracker traces can be reverted easily.

```csharp
var dict = new TrackableDictionary<int, string>();
dict.SetDefaultTracker();                   // attach tracker to dictionary

dict[1] = "One";                            // make changes
dict[2] = "Two";

Console.WriteLine(dict);                    // show dictionary: { 1: "One", 2: "Two "}

dict.Tracker.RollbackTo(dict);              // revert changes
Console.WriteLine(dict);                    // show dictionary: { }

dict.Tracker.Clear();                       // for further operation, clear tracker.
```

### Tracker.HasChangeSet event

When a tracker has first change (which means HasChange becomes true from false),
it fires HasChangeSet event. It can be used for removing unnecessary change checking code.

```csharp
var set = new TrackableSet();
set.SetDefaultTracker();
set.Tracker.HasChangeSet += _ => {          // listen HasChangeSet event
  Console.WriteLine("* Changed *"); 
}

set.Add(1);                                 // * Changed *
set.Add(2);

set.Tracker.Clear();                        // clear all changes.
set.Add(3);                                 // * Changed *
set.Add(4);
```

After tracker being cleared, HasChangeSet event is fired again when gets new first change.

