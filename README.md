# StaticVisitor (namespace: Sid.Tools.StaticVisitor)
A customizable data structure static visitor for C#.

This package, released under MIT license, allows to visit static data structures easily in C#. Static visit means that it's the types of the data structure that get visited, not the instances.

## Usages
The basic usage is to create an instance of `Sid.Tools.StaticVisitor.StaticVisitor` and pass an `IList<Action>` that will collect the results of the visit, then invoke the `Visit` method.

Overloads for more advanced behaviour also exist, where you specify a custom `Action<Type>` which defines what custom action to execute upon visit.

Please not that not all types are visited by default: by default [primitive types](https://docs.microsoft.com/en-us/dotnet/api/system.type.isprimitive) and `object` are not visited; this behaviour can be customized (see below).

Visit behaviour can be customized by passing a custom `StaticVisitorConfiguration` instance.

### Basic example
The method `DoSomething`
```
public class DataStructure {
   public Property Property1 {get;}
}

public class Property {}

public void DoSomething() {
   var visitor = new StaticVisitor(out var list);
   visitor.Visit(typeof(DataStructure));
}
```
will populate `list` with the types
- `DataStructure`
- `Property`

### Custom type filtering example
The following code
```
var visitor = new Sid.Tools.StaticVisitor.StaticVisitor(out var list, new Tools.DataStructure.StaticVisitorConfiguration()
   {
      TypeCanBeVisited = x =>
         Sid.Tools.StaticVisitor.StaticVisitorConfiguration.DefaultTypeCanBeVisited(x)
         && !x.IsValueType
   });
```
will specify which types should be visited and which shouldn't.
Specifically it will preserve the default filtering, but also exclude types that are value types.

### Advanced configuration
Please refer to the docstring documentation in https://github.com/micheledicosmo/StaticVisitor/blob/master/StaticVisitor/StaticVisitor.cs in the object `StaticVisitorConfiguration`.


## Order of visit
The types get visited following this order:
- Inherited types
- Encompassing types
- The type itself
- Properties
