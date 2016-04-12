# TrackableData.Json

It supports [Json.Net](https://www.nuget.org/packages/Newtonsoft.Json).

## Where can I get it?

```
Install-Package TrackableData.Json
```

## How to use

### Serialize/Deserialize TrackableData

Json.NET can (de)serialize all types of trackalbe data without any configuration.
TrackableData itself can be considered as normal .NET classes.

### Serialize/Deserialize Tracker

Tracker can be (de)serialized well like TrackableData, too. But more terse representation
of tracker, dedicated converter can be used like:

```csharp
var dict = new TrackableDictionary<int, string>() {
    { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
dict.SetDefaultTracker();                   // attach tracker to dictionary

dict.Remove(1);                             // make changes
dict[2] = "TwoTwo";
dict.Add(4, "Four");

var t = dict.Tracker;
Print(JsonConvert.SerializeObject(t));      // show tracker as json without converter
                                            // TODO:
var s = new JsonSerializerSettings {
    Converters = new JsonConverter[] {
        new TrackableDictionaryTrackerJsonConverter<int, string>()
    }
};
Print(JsonConvert.SerializeObject(t, s));   // show tracker as json with converter
                                            // TODO:
```
