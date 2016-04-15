# TrackableData.Sql

TrackableData.Sql provides a general adapter for loading and saving trackable data itself
and updating data with changes which a tracker holds.
Because this SQL adapter works with ADO.NET, it's quite easy to support other SQL drivers
which is not listed here if that provides ADO.NET driver.

## Where can I get it?

For SQL Server:

```
Install-Package TrackableData.MsSql
```

For MySQL:

```
Install-Package TrackableData.MySql
```

For PostgreSQL:

```
Install-Package TrackableData.PostgresSql
```

## How to use

### General

#### List is not supported.

SQL doesn't have similar idiom for list data structure. But except for list, all types are
supported.

#### SqlProvider

Trackable.Sql uses ISqlProvider instance to deal with differences that SQL drivers make.
Because of this, all sql mapper requires ISqlProvider instance in their constructors.

```csharp
var mapper = new TrackablePocoSqlMapper(
    MsSqlProvider.Instance, // or MySqlProvider or PostgreSqlProvider
    ...)
```

#### SQL generate methods and helper async methods.

SQL mapper class provides SQL generate methods for every operations. For example, load
operation for TrackableDictionary can be achived by two ways. First, SQL mapper and
dictionary are prepared.

```csharp
var mapper = new TrackableDictionarySqlMapper<int, string>(...);

var dict = new TrackableDictionary<int, string>() {
    { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
```

SQL for creating trackable data is generated with BuildSqlForCreate method of mapper.
Generated SQL is quite raw but this can be used for batching SQLs.

```csharp
// generate sql
var sql = mapper.BuildSqlForCreate(dict);

// run sql
using (var command = _sqlProvider.CreateDbCommand(sql, dbConnection)) {
    return await command.ExecuteNonQueryAsync();
}
```

Second, a helper method simplifies code for most cases.

```csharp
await mapper.CreateAsync(dbConnection, dict);
```

### CRUD cases

This section describes basic operations of SQL, CRUD. As an example, following
poco IPerson will be used:

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    [TrackableProperty("sql.primary-key")] int Id { get; set; }
    string Name { get; set; }
    int Age { get; set; }
}
```

```csharp
var mapper = new TrackablePocoSqlMapper<IPerson>(
    MySqlProvider.Instance,
    "Person");
```

#### Create table

Creating table it not one of crud operations.
But this feature is good for building test environment and why it has been developed.
So if you decide to use this feature in production, watch generated SQL carefully.

```csharp
await _mapper.ResetTableAsync(dbConnection);
```

This method will create table like (MySql is used in this case):

```sql
DROP TABLE IF EXISTS `Person`;
CREATE TABLE `Person` (
  `Id` INT NOT NULL,
  `Name` VARCHAR(10000) CHARACTER SET utf8,
  `Age` INT NOT NULL,
  PRIMARY KEY (`Id`)
);
```

When fine-tuned control is required like index or column type, write SQL manually.

#### Create data

Following code creates 1 person, set initial state and save it to DB.

```csharp
var person = new TrackablePerson();
person.Id = 1;
person.Name = "Testor";
person.Age = 10;
await _mapper.CreateAsync(dbConnection, person);
```

CreateAsync method generates following SQL and run it on DB.

```sql
INSERT INTO `Person` (`Id`,`Name`,`Age`) VALUES (1,'Testor',10);
```

#### Read data

Following code read a person whose id is 1.

```csharp
var person = await _mapper.LoadAsync(dbConnection, 1);
Print(person);                    // { Id:1, Name:"Tester", Age:10 }
```

LoadAsync method generates following SQL and run it on DB for fetching data.

```sql
SELECT `Id`,`Name`,`Age` FROM `Person` WHERE `Id`=1;
```

#### Update data

Following code read a person whose id is 1, update its state and save it to DB.

```csharp
var person = await _mapper.LoadAsync(dbConnection, 1);
person.SetDefaultTracker();
person.Name = "Admin";
person.Age = 20;
return _mapper.SaveAsync(dbConnection, person.Tracker, id);
```

SaveAsync method generates following SQL and run it on DB.

```sql
UPDATE `Person` SET `Name`='Admin',`Age`=20 WHERE `Id`=1;
```

#### Delete data

Following code delete a person whose id is 1 on DB.

```csharp
await DeleteAsync(1);
```

DeleteAsync method generates following SQL and run it on DB.

```sql
DELETE FROM `Person` WHERE `Id`=1;
```

### Advanced topics

#### TrackablePropertyAttribute

With TrackablePropertyAttribute, you can control how SQL mapper work for each members.

##### sql.ignore

TrackableProperty("sql.ignore") excludes the specified member from SQL mapping table.
Following poco doesn't have `Age` property in its SQL table.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    int Id { get; set; }
    string Name { get; set; }
    [TrackableProperty("sql.ignore")] int Age { get; set; }
}
```

##### sql.column

TrackableProperty("sql.column:name") can specify column name of the specified member.
By default SQL mapper uses property name for its column name.
Following poco uses column `Old` for property Age.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    int Id { get; set; }
    string Name { get; set; }
    [TrackableProperty("sql.column:Old")] int Age { get; set; }
}
```

##### sql.primary-key

TrackableProperty("sql.primary-key") specifies the column primary in POCO.
With this property you can fetch exact one item from DB.
Following poco sets column `Id` primary.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    [TrackableProperty("sql.primary-key")] int Id { get; set; }
    string Name { get; set; }
    int Age { get; set; }
}
```

Because Id is primary, you can read a person whose Id is 1.

```csharp
var person = await _mapper.LoadAsync(dbConnection, 1);
```

##### sql.identity

TrackableProperty("sql.identity") specifies the column identity in POCO.
When a data is created on DB, value of identity column is set by DB automatically.
Following poco sets column `Id` primary and identity.

```csharp
public interface IPerson : ITrackablePoco<IPerson> {
    [TrackableProperty("sql.primary-key", "sql.identity")] int Id { get; set; }
    string Name { get; set; }
    int Age { get; set; }
}
```

When a person is created, DB generates new unique value for Id and returns it.

```csharp
var person = new TrackablePerson();
person.Name = "Testor";
person.Age = 10;
await _mapper.CreateAsync(dbConnection, person);
Console.WriteLine(person.Id);         // DB generates person's Id
```

#### Head key column

Head keys are used for dividing rows in a table.
For example, TrackableDictionary reads all rows when loading it.
But with head key, you can change how it interact with a table.

At first, following mapper doesn't use head key.

```csharp
var mapper = new TrackableDictionarySqlMapper<int, string>(
    sqlProvider,
    "Dict1",
    new ColumnDefinition("Id"),
    new ColumnDefinition("Value", typeof(string)),
    null);
```

Without head key, loading dictionary means reading all rows of `Dict1` table.

```csharp
var dict = await _mapper.LoadAsync(dbConnection);
// SELECT `Id`,`Value` FROM `Dict1`;
```

Then, change mapper to use head key whose name is `HeadKey`.

```csharp
var mapper = new TrackableDictionarySqlMapper<int, string>(
    sqlProvider,
    "Dict2",
    new ColumnDefinition("Id"),
    new ColumnDefinition("Value", typeof(string)),
    new[]
    {
        new ColumnDefinition("HeadKey", typeof(int)),
    });    
```

With head key, head key value should be specified for operation and
it limits operation scope on a table.

```csharp
var dict = await _mapper.LoadAsync(dbConnection, 1);
// SELECT `Id`,`Value` FROM `Dict2` WHERE `HeadKey`=1;
```
