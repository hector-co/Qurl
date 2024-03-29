# Qurl

Allows creating filter objects based on URL query strings.


## Installation

Install with nuget

    Install-Package Qurl
    

### Asp.Net Core extension

    Install-Package Qurl.AspNetCore

### Swagger definitions

    Install-Package Qurl.SwaggerDefinitions
    
## Usage

Qurl maps query strings paramaters to filter properties.

First, a model needs to be defined using the abstract class `FilterProperty<>`for each filter property:
```csharp
public class SampleDtoFilter
{
    public FilterProperty<int> Id { get; set; }
    public FilterProperty<string> Title { get; set; }
    public FilterProperty<bool> Active { get; set; }
}
```

Then it is possible to create instances of this class using `Qurl.QueryBuilder`:

```csharp
var queryString = "title=testTitle&active=true";
var query = QueryBuilder.FromQueryString<Query<SampleDtoFilter>>(queryString);
```

`FilterProperty<>` is an abstract class, so the concrete types for each property are resolved at runtime. In the previous example `query.Title` and `query.Active` will be instances of `EqualsFilterProperty<>`.

### Query string operators

Operators can be passed in query string to defining runtime property types. These are the available operators:

|Operator    |Type                                |Usage|
|:----------:|------------------------------------|-|
| (none), eq |`EqualsFilterProperty<>`            |"param=value"|
| ne         |`NotEqualsFilterProperty<>`         |"param[ne]=value|
| lt         |`LessThanFilterProperty<>`          |"param[lt]=value"|
| lte        |`LessThanOrEqualFilterProperty<>`   |"param[lte]=value"|
| gt         |`GreaterThanFilterProperty<>`       |"param[gt]=value"|
| gte        |`GreaterThanOrEqualFilterProperty<>`|"param[gte]=value"|
| ct         |`ContainsFilterProperty<>`          |"param[ct]=value"|
| sw         |`StartsWithFilterProperty<>`          |"param[sw]=value"|
| ew         |`EndsWithFilterProperty<>`          |"param[ew]=value"|
| in         |`InFilterProperty<>`                |"param[in]=value1,value2,valuen"|
| nin        |`NotInFilterProperty<>`             |"param[nin]=value1,value2,valuen"|
| rng        |`RangeFilterProperty<>`             |"param[rng]=valueFrom,valueTo"|

*The `ct` (the equivalent to SQL 'like'), `sw` and `ew` operators only works with strings*

Parameters in query string are specified this way:
    
    "id[gt]=2&title[ct]=testTitle&active=true"
    

### Queryable extensions

Given the model:

```csharp
public class SampleDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Active { get; set; }
}
```

The previous `SampleDtoFilter` class can be used in an `IQueryable<SampleDto>` collection, using the `ApplyQuery` extension method. This works very well with Entity Framework contexts:

```csharp
var queryString = "id[gt]=2&title[ct]=testTitle&active=true";
var query = QueryBuilder.FromQueryString<Query<SampleDtoFilter>>(queryString);

var context = new SampleEfContext(); // Entity Framework context
var filteredObjects = context.Set<SampleDto>().ApplyQuery(query);
```

### Sorting and Paging

More examples in unit tests and sample project

### Asp.Net Core model binder

`Qurl.AspNetCore` provides a model binder can be used for integrating Qurl with AspNetCore. In `Startup` class:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...
    services.AddQurlModelBinder();
}
```
Then in the controller:

```csharp
[Route("api/sample")]
public class SampleDtosController : Controller
{
    [HttpGet]
    public IActionResult Get([FromQuery]Query<SampleDtofilter> query)
    {
        // ...
    }
}
```

### Swagger definitions

`Qurl.SwaggerDefinitions` provides an extension method for adding Qurl definitions to `Swashbuckle.AspNetCore`. In `Startup` class

```csharp
public void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddSwaggerGen(options =>
    {
        options.AddQurlDefinitions();
    });
}
