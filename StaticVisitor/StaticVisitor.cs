﻿using System;
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

        /// <summary>
        /// Default implementation of <see cref="TypeCanBeVisited"/> which can be used to extend the default behaviour. See example code in <see cref="TypeCanBeVisited"/>
        /// </summary>
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
        /// Should return <c>true</c> if the assignable types of the given type should be discovered in the current app domain through reflection and visited. By default no assignable types are visited (defaults to <c>false</c>).
        /// </summary>
        public Func<Type, bool> VisitAssignableTypesOf { get; set; } = x => false;
    }

    /// <summary>
    /// A customizable visitor for static data structures
    /// </summary>
    public class StaticVisitor
    {
        private readonly Action<Type> Action;

        /// <summary>
        /// Event raised when a tracing event occurs. Can be used for debugging purposes
        /// </summary>
        /// <example><code>visitor.Trace += (o, m) => System.Console.Out.WriteLine($"[VISITOR] {m}");</code></example>
        public event EventHandler<string> Trace;

        /// <summary>
        /// Raises the event <see cref="Trace"/>
        /// </summary>
        /// <param name="message">Tracing message</param>
        /// <param name="stackLevel">0-based position in the stack</param>
        protected virtual void OnTrace(string message, int stackLevel)
        {
            var handler = Trace;
            handler?.Invoke(this, $"{string.Empty.PadLeft(stackLevel * 3)}{message}");
        }

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
            VisitInternal(type, new System.Collections.Generic.HashSet<Type>(), -1);
        }

        private void VisitInternal(
            Type type,
            System.Collections.Generic.ISet<Type> visitedSet,
            int stackLevel)
        {
            stackLevel++;
            if (configuration.AvoidMultipleVisits && visitedSet.Contains(type))
            {
                OnTrace($"{type} visit avoided as type already visited.", stackLevel);
                stackLevel--;
                return;
            }
            else
                visitedSet.Add(type);

            if (!configuration.TypeCanBeVisited(type))
            {
                OnTrace($"{type} cannot be visited as per current configuration.", stackLevel);
                stackLevel--;
                return;
            }

            Action(type);

            if (configuration.VisitInheritedTypes)
            {
                OnTrace($"Iterating through {type} inherited types now...", stackLevel);
                foreach (var inheritedType in type.GetInheritedTypes())
                {
                    OnTrace($">Recursing over {inheritedType}", stackLevel);
                    VisitInternal(inheritedType, visitedSet, stackLevel);
                }
            }

            if (configuration.VisitEncompassingTypes)
            {
                OnTrace($"Iterating through {type} encompassing types now...", stackLevel);
                foreach (var encompassingType in type.GetEncompassingTypes())
                {
                    OnTrace($">Recursing over {encompassingType}", stackLevel);
                    VisitInternal(encompassingType, visitedSet, stackLevel);
                }
            }

            if (configuration.VisitAssignableTypesOf(type))
            {
                OnTrace($"Iterating through {type} assignable types now...", stackLevel);
                var assignableTypes = type.GetAssignableTypes(AppDomain.CurrentDomain);
                foreach (var assignableType in assignableTypes)
                {
                    OnTrace($">Recursing over {assignableType}", stackLevel);
                    VisitInternal(assignableType, visitedSet, stackLevel);
                }
            }
            else
            {
                OnTrace($"{type} assignable types cannot be visited as per current configuration.", stackLevel);
            }

            OnTrace($"Iterating through {type} properties' type now...", stackLevel);
            foreach (var propertyType in
                type
                    .GetProperties()
                    .Where(x => configuration.PropertyCanBeVisited(x))
                    .Select(x => x.PropertyType))
            {
                OnTrace($">Recursing over {propertyType}", stackLevel);
                VisitInternal(propertyType, visitedSet, stackLevel);
            }
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
