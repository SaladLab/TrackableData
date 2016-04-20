# TrackableData.MongoDB

## Where can I get it?

```
Install-Package TrackableData.MongoDB
```

## How to use

### General

#### Underlying driver

TrackableData.Redis uses official
[MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver/)
for interacting with MongoDB.

#### Supported type

All types are supported. But you cannot remove an element anywhere in list,
because MongoDB doens't provide this function for array.
An element at only begining and end of list can be removed.

#### Command generate methods and helper async methods.

MongoDB mapper provides command generate methods for every operations.
For example, update operation for TrackableDictionary can be achived by two ways.
First, MongoDB mapper and dictionary are prepared.

```csharp
var mapper = new TrackableDictionaryMongoDbMapper<int, string>();

var dict = new TrackableDictionary<int, string>() {
    { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
dict.SetDefaultTracker();
// modify dict here
```

Update commands for saving traced changes are generated with BuildUpdatesForSave method.
Generated command can be used for batching.

```csharp
// generate update commands
var updates = mapper.BuildUpdatesForSave(null, dict.Tracker);

// run commands
await collection.UpdateOneAsync(
  Builders<BsonDocument>.Filter.Eq("_id", id),
  updates);
```

Second, a helper method simplifies code for most cases.

```csharp
await mapper.SaveAsync(collection, dict.Tracker);
```

### Operations

This section describes basic operations of SQL, CRUD. As an example, following
poco IPerson will be used:

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
  ObjectId Id { get; set; }
  string Name { get; set; }
  int Age { get; set; }
}
```

For mapping trackable data to redis data, following poco mapper will be used.

```csharp
var mapper = new TrackablePocoMongoDbMapper<IPerson>();
```

#### Create data

Following code creates 1 person, set initial state and save it to MongoDB.

```csharp
var person = new TrackablePerson();
person.Id = 1;
person.Name = "Testor";
person.Age = 10;
await mapper.CreateAsync(collection, person);
```

CreateAsync method generates following command and run it on MongoDB.

```javascript
db.collection.insert({ _id: 1, Name: "Testor", Age: 10 })
```

#### Read data

Following code read a person whose id is 1 from collection of MongoDB.

```csharp
var person = await mapper.LoadAsync(collection, 1);
Print(person);                    // { Id:1, Name:"Tester", Age:10 }
```

LoadAsync method generates following command and run it on redis for fetching data.

```javascript
db.inventory.find({ _id: 1 })
```

#### Update data

Following code read a person whose id is 1, update its state and save it to MongoDB.

```csharp
var person = await mapper.LoadAsync(collection, 1);
person.SetDefaultTracker();
person.Name = "Admin";
person.Age = 20;
return mapper.SaveAsync(collection, person.Tracker, person.Id);
```

SaveAsync method generates following command and run it on MongoDB.

```javascript
db.collection.update(
  { _id: 1 },
  { $set: { Name: "Admin", Age: 20 } })
```

#### Delete data

Following code delete a person whose id is 1 from collection of MongoDB.

```csharp
await mapper.DeleteAsync(collection, 1);
```

DeleteAsync method generates following command and run it on MongoDB.

```javascript
db.collection.remove({ _id : 1 })
```

### Type-Mapping

#### POCO

```csharp
public interface ITestPoco : ITrackablePoco<ITestPoco> {
  ObjectId Id { get; set; }
  string Name { get; set; }
  int Age { get; set; }
  int Extra { get; set; }
}

// Create MongoDB mapper
var mapper = new TrackablePocoMongoDbMapper<ITestPoco>();

// Load data whose key is "1" with prepared mapper
var poco = await mapper.LoadAsync(collection, 1);
```

#### Dictionary

```csharp
var mapper = new TrackableDictionaryMongoDbMapper<int, string>();    
var dictionary = mapper.LoadAsync(collection, 1);
```

#### List

```csharp
var mapper = new TrackableListMongoDbMapper<string>();
var list = mapper.LoadAsync(collection, 1);
```

#### Set

```csharp
var mapper = new TrackableSetMongoDbMapper<int>();
var set = mapper.LoadAsync(collection, 1);
```

#### Container

```csharp
public interface ITestContainer : ITrackableContainer<ITestContainer> {
  TrackableTestPocoForContainer Person { get; set; }
  TrackableDictionary<int, MissionData> Missions { get; set; }
  TrackableList<TagData> Tags { get; set; }
}

// Create MongoDB mapper
var mapper = new TrackableContainerMongoDbMapper<ITestContainer>();

// Load data whose key is "1" with prepared mapper
var container = await mapper.LoadAsync(collection, 1);
```

### Advanced topics

#### TrackablePropertyAttribute

With TrackablePropertyAttribute, you can control how MongoDB mapper work for each members.

##### mongodb.ignore

TrackableProperty("mongodb.ignore") excludes the specified member from MongoDB mapping table.
Following poco doesn't have `Age` property at its document in collection.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    int Id { get; set; }
    string Name { get; set; }
    [TrackableProperty("mongodb.ignore")] int Age { get; set; }
}
```

##### mongodb.identity

TrackableProperty("mongodb.identity") specifies which is an identity field of POCO. 
By default, MongoDB mapper sets a property whose name is `id` identity one.
Following poco uses property `CustomID` as identity.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
  [TrackableProperty("mongodb.identity")] long CustomId { get; set; }
  string Name { get; set; }
  int Age { get; set; }
}
```

#### Head key

MongoDB uses document data model and every document is top-level and accessed by
query selector. Basically there is no hiearchy but document itself is json document, so
we can get hierarchy with help of it.

Without headkey, all trackable data is treated as top-level document.

```csharp
await mapper.CreateAsync(collection, person1); // person1.Id = 1
await mapper.CreateAsync(collection, person2); // person2.Id = 2
```

You can see person 1 and 2 are documents.

```javascript
db.collection.insert({ _id: 1, Name: "Testor", Age: 10 })
db.collection.insert({ _id: 2, Name: "Testor", Age: 10 })
```

But with headkey, trackable data could be a part of top-level document.

```csharp
await mapper.CreateAsync(collection, person1, "Head"); // person1.Id = 1
await mapper.CreateAsync(collection, person2, "Head"); // person2.Id = 2
```

You can see person 1 and 2 are in same document whose id is "Head".

```javascript
db.collection.insert(
  { _id: "Head" },
  { $set: { "1": { "Name": "Admin", Age: 20 } } })

db.collection.update(
  { _id: "Head" },
  { $set: { "2": { "Name": "Admin", Age: 20 } } })
```

#### UniqueInt64Id

MongoDB ObjectID is 12 bytes long and consists of 

- a 4-byte value representing the seconds since the Unix epoch,
- a 3-byte machine identifier,
- a 2-byte process id, and
- a 3-byte counter, starting with a random value.

It's good enough but 12 bytes integer is not easy to use,
so alternative 8 bytes ObjectId is provided as UniqueInt64Id.

ObjectInt64Id is 8 bytes long and consists of 

- a 4-byte value representing the seconds since the Unix epoch,
- a **1-byte** machine identifier,
- a **1-byte** process id, and
- a **2-byte** counter, starting with a random value.

With a price of more conflict posibility, it gives you easy-to-use type.

```csharp
var id = UniqueInt64Id.GenerateNewId();
// use id for unique key
```

#### BsonClassMap

MongoDB.Driver uses BsonClassMap to map .NET object to BsonDocument.
TrackableData.MongoDB provides TypeMapper class to give BsonClassMap customized properties.
If you want to override this behaviour, call `RegisterClassMap`
before creating MongoDB mappers.
