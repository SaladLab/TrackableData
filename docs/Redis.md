# TrackableData.Redis

## Where can I get it?

```
Install-Package TrackableData.Redis
```

## How to use

### General

#### Underlying driver

TrackableData.Redis uses
[StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis)
for interacting with redis.

#### Supported type

All types are supported. But operation for redis list is a quite limited.
List works like queue so you can add and remove an element at the beginning and end of list.
But you can modify an element anywhere in list.

#### Key prefix

Redis saves value with unique key. Redis uses same key namescope with whole data structures.
So you need to make unique key for every trackable data. For example, when you
have trackable poco Person and dictionary Inventory, you can make name of those with
unique prefix to separate scope of key namespace.

```csharp
// Load person whose key is 1 with prefix "Person:"
var person = await personMapper.LoadAsync(redis, "Person:" + 1);

// Load inventory whose key is 1 with prefix "Inventory:"
var inventory = await inventoryMapper.LoadAsync(redis, "Inventory:" + 1);
```

### Operations

This section describes basic operations of SQL, CRUD. As an example, following
poco IPerson will be used:

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
  int Id { get; set; }
  string Name { get; set; }
  int Age { get; set; }
}
```

For mapping trackable data to redis data, following poco mapper will be used.

```csharp
var mapper = new TrackablePocoRedisMapper<IPerson>();
```

#### Create data

Following code creates 1 person, set initial state and save it to Redis.

```csharp
var person = new TrackablePerson();
person.Id = 1;
person.Name = "Testor";
person.Age = 10;
await mapper.CreateAsync(redis, person, "Person:" + person.Id.ToString());
```

CreateAsync method generates following command and run it on redis.

```
HMSET Person:1 Id 1 Name "Testor" Age 10
```

#### Read data

Following code read a person whose id is 1.

```csharp
var person = await mapper.LoadAsync(redis, "Person:" + 1);
Print(person);                    // { Id:1, Name:"Tester", Age:10 }
```

LoadAsync method generates following command and run it on redis for fetching data.

```
HGETALL Person:1
```

#### Update data

Following code read a person whose id is 1, update its state and save it to redis.

```csharp
var person = await mapper.LoadAsync(redis, "Person:" + 1);
person.SetDefaultTracker();
person.Name = "Admin";
person.Age = 20;
return mapper.SaveAsync(redis, person.Tracker, "Person:" + 1);
```

SaveAsync method generates following command and run it on redis.

```
HMSET Person:1 Name "Admin" Age 20
```

#### Delete data

Following code delete a person whose id is 1 on redis.

```csharp
await mapper.DeleteAsync(redis, "Person:" + 1);
```

DeleteAsync method generates following command and run it on redis.

```
DEL Person:1
```

### Type-Mapping

#### POCO

```csharp
public interface ITestPoco : ITrackablePoco<ITestPoco> {
    int Id { get; set; }
    string Name { get; set; }
    int Age { get; set; }
    int Extra { get; set; }
}

// Create redis mapper
var mapper = new TrackablePocoRedisMapper<ITestPoco>();

// Load data whose key is "TestPoco:1" with prepared mapper
var poco = await mapper.LoadAsync(redis, "TestPoco:" + 1);
```

#### Dictionary

```csharp
var mapper = new TrackableDictionaryRedisMapper<int, string>();    
var dictionary = mapper.LoadAsync(redis, "TestDictionary:" + 1);
```

#### List

```csharp
var mapper = new TrackableListRedisMapper<string>();
var list = mapper.LoadAsync(redis, "TestList:" + 1);
```

#### Set

```csharp
var mapper = new TrackableSetRedisMapper<int>();
var set = mapper.LoadAsync(redis, "TestSet:" + 1);
```

#### Container

Two types of redis mapper for container are implemented. Redis doesn't provide
key hierarchy and two  mapper solve this limitation differently.

At first following container will be used for explanation.

```csharp
public interface ITestContainer : ITrackableContainer<ITestContainer> {
  TrackableTestPocoForContainer Person { get; set; }
  TrackableDictionary<int, MissionData> Missions { get; set; }
  TrackableList<TagData> Tags { get; set; }
}
```

##### TrackableContainerRedisMapper

TrackableContainerRedisMapper uses common way to give you hierarchy.
When it saves properties of collection, it adds suffix to collection key.
Following code uses TrackableContainerRedisMapper and
load data whose key is TestContainerA:1.

```csharp
var mapperA = new TrackableContainerRedisMapper<ITestContainer>();
var container = mapper.LoadAsync(redis, "TestContainerA:" + 1);
```

Saved data looks like this.
Three properties of container are saved in different keys.

```javascript
"TestContainerA:1:Person" = {
  "Name": "Testor", "Age": "10", "Extra": "0"
}
"TestContainerA:1:Missions" = {
  "1": {"Kind":101,"Count":1,"Note":"Handmade Sword"},
  "2": {"Kind":102,"Count":3,"Note":"Lord of Ring"}
}
"TestContainerA:1:Tags" = [
 {"Text":"Hello","Priority":1}
 {"Text":"World","Priority":2}
]
```

If you want to change suffix name of property, use
TrackableProperty("redis.keysuffix:name").

##### TrackableContainerHashesRedisMapper

TrackableContainerHashesRedisMapper saves all property values into hashes with
matched field key. For example, following load data whose key is TestContainerA:1.

```csharp
var mapperB = new TrackableContainerHashesRedisMapper<ITestContainer>();
var container = mapper.LoadAsync(redis, "TestContainerB:" + 1);
```

Saved data looks like this.
Three properties of container are saved in one hashes.

```javascript
"TestContainerB:1" = {
  "Person": {"Name":"Testor","Age":11,"Extra":0},
  "Missions": {"1":{"Kind":103,"Count":3,"Note":"Just Arrived"},"2":"..."},
  "Tags": [{"Text":"Hello","Priority":1},{"Text":"World","Priority":2},"..."]
}
```

### Advanced topics

#### TrackablePropertyAttribute

With TrackablePropertyAttribute, you can control how redis mapper work for each members.

##### redis.ignore

TrackableProperty("redis.ignore") excludes the specified member from redis mapping table.
Following poco doesn't have `Age` property in its redis hashes.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    int Id { get; set; }
    string Name { get; set; }
    [TrackableProperty("redis.ignore")] int Age { get; set; }
}
```

##### redis.field

TrackableProperty("redis.field:name") can specify field name of the specified member.
By default redis mapper uses property name for its field name.
Following poco uses column `Old` for property Age.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    int Id { get; set; }
    string Name { get; set; }
    [TrackableProperty("redis.field:Old")] int Age { get; set; }
}
```

##### redis.keysuffix

TrackableProperty("redis.keysuffix:suffix") can specify suffix of the specified member
for TrackableContainerRedisMapper.
By default redis mapper uses property name for its suffix name.
Following container uses suffix `Values` for property Tags.

```csharp
public interface ITestContainer : ITrackableContainer<ITestContainer> {
  TrackableTestPocoForContainer Person { get; set; }
  TrackableDictionary<int, MissionData> Missions { get; set; }
  [TrackableProperty("redis.keysuffix:Values")] TrackableList<TagData> Tags { get; set; }
}
```

Saved ITestContainer looks like:

```javascript
"TestContainerA:1:Person" = { }
"TestContainerA:1:Missions" = { }
"TestContainerA:1:Values" = [ ]           // suffix Values instead of Tags
```

#### RedisTypeConverter

Redis stores value as blob. Because of this, all types in .NET are able to converted
to blob and from blob. By default redis mapper uses ToString for primitive types and
JsonConvert.SerializeObject for non primitive types. To customize this action,
configured RedisTypeConverter need to be passed to parameter when creating mapper.

```csharp
var converter = new RedisTypeConverter();
// register customized string converter.
converter.Register(
    v => ('`' + v + '`'),
    v => ((string)v).Trim('`'));
// create mapper with customized type converter.
var mapper = new TrackableDictionaryRedisMapper<int, string>(converter);
```    
