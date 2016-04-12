# Trackable.Protobuf

It supports [protobuf-net](https://www.nuget.org/packages/protobuf-net).

## Where can I get it?

```
Install-Package TrackableData.Protobuf
```

## How to use

### Serialize/Deserialize TrackableData

Protobuf.net can serialize TrackableDictionary, List and Set without any settings.
But with POCO and collection, interface should be annotated with protobuf.net
attributes properly. For IPerson POCO explained before, [ProtoContract] and [ProtoMember]
attributes are added like common protobuf.net use cases.

```csharp
[ProtoContract]
public interface IPerson : ITrackablePoco<IPerson> {
    [ProtoMember(1)] string Name { get; set; }
    [ProtoMember(2)] int Age { get; set; }
}
```

CodeGenerator detects protobuf attributes and makes use of them in generating trackable
and tracker classes.

### Serialize/Deserialize Tracker

On the other hand, tracker needs configuration before being serialized. Protobuf.net
requires classes to be annotated with proper attributes or covered by surrogate classes.

```csharp
var tracker = new TrackableSetTracker<int>();

// create protobuf.net type model which is configured to use
// TrackableSetTrackerSurrogate<int> for serializing TrackableSetTracker<int>
var model = TypeModel.Create();
model.Add(typeof(TrackableSetTracker<int>), false)
     .SetSurrogate(typeof(TrackableSetTrackerSurrogate<int>));

// tracker can be (de)serialized properly
var tracker2 = model.DeepClone(tracker);
```

With these surrogates, old value of operation will not be serialized for reducing
the size of output.

### Protobuf.net precompiler supports

Protobuf provides precompile tool for supporting AOT environment that forbid dynamic
code generation on the fly. But this tool doesn't use surrogate classes at all.
To handle this limitation, customized precompile is provided along with TrackableData.Templates
and it detects following code pattern.

```csharp
[ProtoContract]
public class ProtobufSurrogateDirectives {
    public TrackableDictionaryTrackerSurrogate<int, ItemData> T1;
    public TrackableDictionaryTrackerSurrogate<int, string> T2;
    public TrackableSetTrackerSurrogate<int> T3;
    public TrackableListTrackerSurrogate<string> T4;
}
```

Previous code has no effect for runtime but for modified precompile tool it specifies
surrogate classes for each types.
