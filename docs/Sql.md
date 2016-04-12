# TrackableData.Sql

TrackableData.Sql provides a general adapter for loading and saving trackable data itself and
updating data with changes which a tracker holds. 
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

SQL wrapper class provides SQL generate methods for every operations. For example, load
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
var sql = mapper.BuildSqlForCreate(dict);
using (var command = _sqlProvider.CreateDbCommand(sql, dbConnection)) {
    return await command.ExecuteNonQueryAsync();
}
```

For simple cases, a helper method simplifies code like this:

```csharp
await mapper.CreateAsync(dbConnection, dict);
```

### Create Table

Creating table feature is not for production. Basically it has been developed
for test and demo only. SQL `CREATE TABLE` command has lot of options to work well under
certain condition. So if you decide to use this feature, watch SQL that create table method
generates carefully.

```csharp
var sql = _mapper.BuildCreateTableSql(dropIfExists: true);
runSql(sql);
```

```csharp
await _mapper.ResetTableAsync(dbConnection);
``` 

### Create Data

**C**RUD

### Delete Data

CRU**D**

### Load Data

C**R**UD

### Update Data

CR**U**D
