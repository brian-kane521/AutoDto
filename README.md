# AutoDto

AutoDto is a C# [source generator](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) for scaffolding basic data transfer object (DTO) classes from database model classes. When exposing an API to perform CRUD operations it is often desirable to separate the user-facing data structures from those mapped to the underlying schema. For instance, a request to create a new entry might not expose an ID field if it is automatically generated by the database, but such a field will be present in a model scaffolded by an [ORM](https://en.wikipedia.org/wiki/Object%E2%80%93relational_mapping). AutoDto was created to ease the burden of writing lots of boilerplate DTO classes that are often very similar to their corresponding DB models.

AutoDto is aimed at projects utilizing [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/), and will only generate DTOs for types referenced in a class deriving from `DbContext`. 
This is implemented simply by searching through the root namespace of the assembly where AutoDto is added and searching recursively for the first class derived from `DbContext` - there is no logic in place to ensure that this is in fact Entity Framework Core, just that the base type is named DbContext.
Once a subclass of `DbContext` is found, AutoDto iterates over each property of the class that is a `DbSet`, and scaffolds Request/Response DTO classes for the types referenced in the DbSets.

Note that the current implementation is very simple, and **will only handle properties** in the target classes. It is designed to handle basic object properties as well as `ICollection`s - basically, if you've [scaffolded your EF models](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/?tabs=dotnet-core-cli) from an existing database, AutoDto should be fully compatible with your project.

Generated classes support recursion - for instance, if you have a class named `Child`, and another class `Parent` that has a property of type `ICollection<Child>`, then AutoDto would generate the class `CreateParentRequest` with a property of type `ICollection<CreateChildRequest>` as long as the `Child` class is also targeted for scaffolding (see the [Configuration](#configuration) section).

## Installation

AutoDto is packaged using nuget, [hosted at this link.](https://www.nuget.org/packages/AutoEfDto/)

To install, open a terminal in your project directory and run the following:
`dotnet add package AutoEfDto --version 1.0.2`

Or in the Visual Studio Package Manager Console:
`NuGet\Install-Package AutoEfDto -Version 1.0.2`

## Basic Usage

A typical project utilizing EF Core will contain a database context class:
```cs
internal class ExampleDbContext : DbContext
{
    public virtual DbSet<ParentClass> ParentClass { get; set; }

    public virtual DbSet<ChildClass> ChildClass { get; set; }
}
```

As well as model classes corresponding to the database tables:
```cs
// ChildClass.cs
internal class ChildClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// ParentClass.cs
internal class ParentClass
{
    public int Id { get; set; }
    public virtual ChildClass? FavoriteChild { get; set; } = null!;
    public virtual ICollection<ChildClass> Children { get; set; } = new List<ChildClass>();
}
```

In its default configuration, AutoDto would generate separate Create, Read, Update, and Delete request DTOs as follows:
```cs
// CreateChildClassRequest.cs
public partial class CreateChildClassRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// CreateParentClassRequest.cs
public partial class CreateParentClassRequest
{
    public int Id { get; set; }
    public CreateChildClassRequest? FavoriteChild { get; set; } = null!;
    public ICollection<CreateChildClassRequest> Children { get; set; } = new List<CreateChildClassRequest>();
}
```
As well as a generic response DTO for each type:
```cs
// ChildClassResponse.cs
public partial class ChildClassResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// ParentClassResponse.cs
public partial class ParentClassResponse
{
    public int Id { get; set; }
    public ChildClassResponse? FavoriteChild { get; set; } = null!;
    public ICollection<ChildClassResponse> Children { get; set; } = new List<ChildClassResponse>();
}
```

By default, request DTOs follow the naming convention `"{RequestType}{BaseClassName}Request"` where `RequestType` is one of "Create", "Read", "Update", or "Delete".

All generated classes have the `partial` modifier so that they can be extended separately within your project without editing the source files, which are overwritten at compile time.

## Configuration

There are [many](https://martinfowler.com/eaaCatalog/dataTransferObject.html) [reasons](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/august/pros-and-cons-of-data-transfer-objects) for leveraging DTOs as an abstraction, and generally DTOs will not be a one-to-one mapping of the corresponding domain models. 
AutoDto includes several attributes that can be used to customize its behavior and conventions to better fit different use-cases. The most important is the `AutoDtoConfiguration` attribute, which can be **applied to the DbContext class** to set the default behavior.

The `AutoDtoConfiguration` attribute can also be applied to individual classes to override the default behavior for that individual class.

### Configuration Attribute

The default configuration for AutoDto (which applies when no `AutoDtoConfiguration` attribute is found) is equivalent to the following:
```cs
[AutoDtoConfiguration(
    ClassDiscoveryBehavior= ClassDiscoveryBehavior.Default, 
    RequestDtoNamingTemplate = "{RequestType}{BaseClassName}Request",
    ResponseDtoNamingTemplate = "{ResponseType}{BaseClassName}Response",
    GenerateRequestTypes = GeneratedRequestType.Default, 
    GenerateResponseTypes = GeneratedResponseType.Default,
    RequestTypesIncludingAllPropertiesByDefault = GeneratedRequestType.All,
    ResponseTypesIncludingAllPropertiesByDefault = GeneratedResponseType.All)]
```

#### Class Discovery Behavior

The `ClassDiscoveryBehavior` parameter configures which classes AutoDto will generate DTOs for by default. It is defined as follows:
```cs
public enum ClassDiscoveryBehavior
{
    IncludeAllDbSets,
    ExcludeAll,
    Default = IncludeAllDbSets,
}
```

`IncludeAllDbSets` is the default value, instructing AutoDto to generate DTOs for all types for which there is a `DbSet` within the `DbContext`, **except** those marked with an [AutoDtoIgnore](#ignore-attribute) attribute.
`ExcludeAll` will cause AutoDto to only generate DTOs for classes marked with an [AutoDtoInclude](#include-attribute) attribute. Note AutoDto will still only discover classes for which there is a `DbSet`.

#### Request Naming Convention

The `RequestDtoNamingTemplate` parameter allows you to customize the names of the generated request DTOs. It accepts a string value that can **optionally** contain the substrings **{RequestType}** and **{BaseClassName}**. If present, AutoDto will replace **{RequestType}** with "Create", "Read", "Update", or "Delete" for each respective DTO class, while **{BaseClassName}** will be replaced with the original class name **for all of the request types**. Any other characters in the string will be used exactly as they appear in the generated class names.

The default template is "{RequestType}{BaseClassName}Request".

**IMPORTANT:** if multiple request types are being generated (see [Generated Request Types](#generated-request-types)), you **must** include **{RequestType}** in the naming template to guarantee the generated classes will have unique names. Failure to do so may lead to invalid code generation.

#### Response Naming Convention

The `ResponseDtoNamingTemplate` parameter allows you to customize the names of the generated response DTOs. It accepts a string value that can **optionally** contain the substrings **{ResponseType}** and **{BaseClassName}**. If present, AutoDto will replace **{ResponseType}** with "Create", "Read", "Update", or "Delete" for each respective DTO class, while **{BaseClassName}** will be replaced with the original class name **for all of the response types**. Any other characters in the string will be used exactly as they appear in the generated class names. Note that for the ["Generic" response type](#generated-response-types), **{ResponseType}** will be replaced with an empty string.

The default template is "{ResponseType}{BaseClassName}Response".

**IMPORTANT:** if multiple response types are being generated (see [Generated Response Types](#generated-response-types)), you **must** include **{ResponseType}** in the naming template to guarantee the generated classes will have unique names. Failure to do so may lead to invalid code generation.

#### Generated Request Types

The `GenerateRequestTypes` parameter specifies which types of request DTOs AutoDto will generate. It is defined as the following:

```cs
[Flags]
public enum GeneratedRequestType
{
    None = 0,
    Create = 1,
    Read = 2,
    Update = 4,
    Delete = 8,
    All = Create | Read | Update | Delete,
    Default = Create | Update,
}
```

You can explicitly specify which request types you want to be generated by using bitwise-or, for instance `GenerateRequestTypes = GeneratedRequestType.Create | GeneratedRequestType.Update` will only generate Create and Update request types.

#### Generated Response Types

The `GeneratedResponseType` parameter specifies which types of response DTOs AutoDto will generate. It is defined as the following:

```cs
[Flags]
public enum GeneratedResponseType
{
    None = 0,
    Create = 1,
    Read = 2,
    Update = 4,
    Delete = 8,
    AllExceptGeneric = Create | Read | Update | Delete,
    All = Create | Read | Update | Delete | Generic,
    Generic = 16,
    Default = AllExceptGeneric,
}
```

You can explicitly specify which response types you want to be generated by using bitwise-or, for instance `GeneratedResponseTypes = GeneratedResponseType.Create | GeneratedResponseType.Update` will only generate Create and Update response types.


#### Properties Included in Requests

The `RequestTypesIncludingAllPropertiesByDefault` parameter can be used to indicate that only some generated request types should default to including all of the properties present in the base class.
For example, passing `RequestTypesIncludingAllPropertiesByDefault = GeneratedRequestType.Create | GeneratedRequestType.Update` will cause AutoDto to include all of the properties present in the base class when generating the Create and Update request types, **except** for properties with an [AutoDtoIgnore](#ignore-attribute) attribute. For all other request types, AutoDto will **only** include properties with an [AutoDtoInclude](#include-attribute) attribute.

#### Properties Included in Responses

The `ResponseTypesIncludingAllPropertiesByDefault` parameter can be used to indicate that only some generated response types should default to including all of the properties present in the base class.
For example, passing `ResponseTypesIncludingAllPropertiesByDefault = GeneratedResponseType.Create | GeneratedResponseType.Update` will cause AutoDto to include all of the properties present in the base class when generating the Create and Update response types, **except** for properties with an [AutoDtoIgnore](#ignore-attribute) attribute. For all other response types, AutoDto will **only** include properties with an [AutoDtoInclude](#include-attribute) attribute.

### Ignore Attribute

The `[AutoDtoIgnore]` attribute can be used to indicate that **a class or a property** should be ignored by AutoDto. 

When present on a class, DTOs **will not** be generated for that class regardless of other configuration parameters.

When present on a property, that property will not be included in any request DTOs specified by the `RequestTypesWherePropertyIsIgnored` parameter nor any response DTOs specified by the `ResponseTypesWherePropertyIsIgnored` parameter.
The `[AutoDtoIgnore]` attribute without any parameters is equivalent to the following:
```cs
[AutoDtoIgnore(
    RequestTypesWherePropertyIsIgnored = GeneratedRequestType.All,
    ResponseTypesWherePropertyIsIgnored = GeneratedResponseType.All)]
```
Thus by default for a property, the `[AutoDtoIgnore]` attribute will cause AutoDto to exclude the property from all generated DTOs.

### Include Attribute

The `[AutoDtoInclude]` attribute can be used to indicate that **a class or a property** should be included by AutoDto. 

When present on a class, DTOs **will** be generated for that class regardless of other configuration parameters. 

When present on a property, that property will only be included in request DTOs specified by the `RequestTypesWherePropertyIsIncluded` parameter as well as response DTOs specified by the `ResponseTypesWherePropertyIsIncluded` parameter, **unless AutoDto is configured to include all properties by default**. This attribute is meant to be used when AutoDto is configured globally or for the class containing the property **not to include all properties** (see [Properties Included in Requests](#properties-included-in-requests) and [Properties Included in Responses](#properties-included-in-responses))
The `[AutoDtoInclude]` attribute without any parameters is equivalent to the following:
```cs
[AutoDtoInclude(
    RequestTypesWherePropertyIsIncluded = GeneratedRequestType.All,
    ResponseTypesWherePropertyIsIncluded = GeneratedResponseType.All)]
```
Thus by default for a property, the `[AutoDtoInclude]` attribute will cause AutoDto to include the property in all generated DTOs.

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
