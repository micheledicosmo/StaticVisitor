using System;
using System.Linq;

namespace Sid.Tools.StaticVisitor
{
    /// <summary>
    /// Simple container for <see cref="StaticVisitor"/> configuration parameters
    /// </summary>
    public class StaticVisitorConfiguration
    {
        /// <summary>
        /// Should return <c>true</c> if the given type should be visited. If it returns <c>false</c>, the type won't be visited (i.e. <see cref="StaticVisitor.Action"/> won't be called, and introspection on the object won't be performed). By default only <c>object</c> and primitive types are excluded. If modifying this behaviour, the default behaviour can be accessed via <see cref="DefaultTypeCanBeVisited"/>.
        /// </summary>
        /// <example>
        /// <code>
        ///var visitor = new Tools.DataStructure.StaticVisitor(out var list, new Tools.DataStructure.StaticVisitorConfiguration()
        ///<para/>  {
        ///<para/>    TypeCanBeVisited = x =&gt;
        ///<para/>      Tools.DataStructure.StaticVisitorConfiguration.DefaultTypeCanBeVisited(x)
        ///<para/>      &amp;&amp; !x.IsValueType
        ///<para/>  });</code>
        /// </example>
        public Func<Type, bool> TypeCanBeVisited { get; set; } = DefaultTypeCanBeVisited;

        public readonly static Func<Type, bool> DefaultTypeCanBeVisited = type => !type.IsPrimitive && type != typeof(object);

        /// <summary>
        /// Should return <c>true</c> if the given property should be visited. By default all properties are visited (defaults to <c>true</c>).
        /// </summary>
        public Func<System.Reflection.PropertyInfo, bool> PropertyCanBeVisited { get; set; } = x => true;

        /// <summary>
        /// Indicates whether types should be visited only once, no matter if they appear multiple times in the data structure. By default multiple visits are disabled (defaults to <c>true</c>).
        /// </summary>
        public bool AvoidMultipleVisits { get; set; } = true;

        /// <summary>
        /// Indicates whether the type and interfaces that a type directly inherits should be visited. By default inherited types are visited (defaults to <c>true</c>).
        /// </summary>
        public bool VisitInheritedTypes { get; set; } = true;

        /// <summary>
        /// Indicates whether, when a type which has encompassing types is visited (i.e. parameter types or element type), its encompassing types should be visited as well. By default encompassing types are visited (defaults to <c>true</c>).
        /// </summary>
        /// <remarks>
        /// This property specifies whether to visit only generic type arguments; that is, the types that have been specified for the visited generic types. If the generic type is a generic type definition, no generic type argument type will be visited (i.e. the possibly expected behaviour that all possible types that can be used as a generic type are visited is not the actual one).
        /// </remarks>
        public bool VisitEncompassingTypes { get; set; } = true;

        /// <summary>
        /// Indicates whether the types in the current app domain that implement a visited type should be discovered through reflection and visited. By default assignable types are not visited (defaults to <c>false</c>).
        /// </summary>
        public bool VisitAssignableTypes { get; set; } = false;
    }

    /// <summary>
    /// A customizable visitor for static data structures
    /// </summary>
    public class StaticVisitor
    {
        public Action<Type> Action { get; }

        private readonly StaticVisitorConfiguration configuration;

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visited types will be added</param>
        public StaticVisitor(out System.Collections.Generic.IList<Type> collection)
        {
            collection = new System.Collections.Generic.List<Type>();
            Action = collection.Add;
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visited types will be added. Use an ordered collection to preserve order of visit</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(out System.Collections.Generic.IList<Type> collection, StaticVisitorConfiguration configuration)
        {
            collection = new System.Collections.Generic.List<Type>();
            Action = collection.Add;
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visited types will be added. Use an ordered collection to preserve order of visit</param>
        public StaticVisitor(System.Collections.Generic.ICollection<Type> collection)
        {
            Action = collection.Add;
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visited types will be added</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(System.Collections.Generic.ICollection<Type> collection, StaticVisitorConfiguration configuration)
        {
            Action = collection.Add;
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="action">The action to be invoked when a type is visited</param>
        public StaticVisitor(Action<Type> action)
        {
            Action = action;
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="action">The action to be invoked when a type is visited</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(Action<Type> action, StaticVisitorConfiguration configuration)
        {
            Action = action;
            this.configuration = configuration;
        }

        /// <summary>
        /// Starts visiting the type
        /// </summary>
        /// <param name="type"></param>
        public void Visit(Type type)
        {
            VisitInternal(type, new System.Collections.Generic.HashSet<Type>());
        }

        private void VisitInternal(Type type, System.Collections.Generic.ISet<Type> visitedSet)
        {
            if (configuration.AvoidMultipleVisits && visitedSet.Contains(type))
                return;
            else
                visitedSet.Add(type);

            if (!configuration.TypeCanBeVisited(type))
                return;

            Action(type);

            if (configuration.VisitInheritedTypes)
                foreach (var inheritedType in type.GetInheritedTypes())
                    VisitInternal(inheritedType, visitedSet);

            if (configuration.VisitEncompassingTypes)
                foreach (var encompassingType in type.GetEncompassingTypes())
                    VisitInternal(encompassingType, visitedSet);

            if (configuration.VisitAssignableTypes)
            {
                var assignableTypes = type.GetAssignableTypes(AppDomain.CurrentDomain);
                foreach (var assignableType in assignableTypes)
                    VisitInternal(assignableType, visitedSet);
            }

            foreach (var propertyType in
                type
                    .GetProperties()
                    .Where(x => configuration.PropertyCanBeVisited(x))
                    .Select(x => x.PropertyType))
                VisitInternal(propertyType, visitedSet);
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns all interfaces and type directly inherited by <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Collections.Generic.IEnumerable<Type>
            GetInheritedTypes(this Type type)
        {
            var baseType = type.BaseType.ToEnumerable();
            var interfaces = type.GetInterfaces();
            return baseType.Concat(interfaces);
        }

        /// <summary>
        /// Returns a named tuple with the given <paramref name="type"/> and its encompassing types (i.e. parameter types of generic types, and element type).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Collections.Generic.IEnumerable<Type>
            GetEncompassingTypes(this Type type)
        {
            if (type == null)
                return Enumerable.Empty<Type>();

            var parameterTypes = type.IsGenericType
                ? type.GenericTypeArguments.Where(y => y != type)
                : Enumerable.Empty<Type>();

            var elementType = type.HasElementType
                ? type.GetElementType().ToEnumerable()
                : Enumerable.Empty<Type>();

            return parameterTypes.Concat(elementType);
        }

        public static System.Collections.Generic.IEnumerable<Type>
            GetAssignableTypes(this Type type, AppDomain appDomain)
        {
            return appDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => type != x)
                .Where(type.IsAssignableFrom);
        }

        /// <summary>
        /// Wrapes <paramref name="item"/> in an <see cref="System.Collections.Generic.IEnumerable<typeparamref name="T"/>"/>
        /// </summary>
        /// <typeparam name="T">The type of the item to be wrapped</typeparam>
        /// <param name="item">The item to be wrapped</param>
        /// <returns></returns>
        public static System.Collections.Generic.IEnumerable<T> ToEnumerable<T>(this T item)
        {
            if (item == null)
                yield break;
            yield return item;
        }
    }
}
