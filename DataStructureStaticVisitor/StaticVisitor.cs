using System;
using System.Linq;

namespace DataStructureStaticVisitor
{
    public class StaticVisitorConfiguration
    {
        /// <summary>
        /// Should return <c>true</c> if the given type should be visited. By default only non-primitive types are visited.
        /// </summary>
        public Func<Type, bool> TypeCanBeVisited { get; set; } = type => !type.IsPrimitive;

        /// <summary>
        /// Should return <c>true</c> if the given property should be visited. By default all properties are visited.
        /// </summary>
        public Func<System.Reflection.PropertyInfo, bool> PropertyCanBeVisited { get; set; } = x => true;

        /// <summary>
        /// Indicates whether types should be visited only once, no matter if they appear multiple times in the data structure
        /// </summary>
        public bool AvoidMultipleVisits { get; set; } = true;
        
        /// <summary>
        /// Indicates whether the type and interfaces that a type directly inherits should be visited
        /// </summary>
        public bool VisitInheritedTypes { get; set; } = true;
        
        ///// <summary>
        ///// Indicates whether, when a type which has encompassing types is visited (i.e. parameter types or element type), its encompassing types should be visited as well
        ///// </summary>
        public bool VisitEncompassingTypes { get; set; } = true;

        /// <summary>
        /// Indicates whether the types in the current app domain that implement a visited type should be discovered through reflection and visited
        /// </summary>
        public bool VisitAssignableTypes { get; set; } = true;
    }

    public class StaticVisitor
    {
        public Action<Type> Action { get; }

        private readonly StaticVisitorConfiguration configuration;

        public StaticVisitor(Action<Type> action)
        {
            Action = action;
            configuration = new StaticVisitorConfiguration();
        }

        public StaticVisitor(Action<Type> action, StaticVisitorConfiguration configuration)
        {
            Action = action;
            this.configuration = configuration;
        }

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

            if (configuration.VisitInheritedTypes)
                foreach (var inheritedType in type.GetInheritedTypes())
                    VisitInternal(inheritedType, visitedSet);

            if (configuration.VisitEncompassingTypes)
                foreach(var encompassingType in type.GetEncompassingTypes())
                    VisitInternal(encompassingType, visitedSet);
            
            Action(type);

            if (configuration.VisitAssignableTypes)
            {
                var assignableTypes = type.GetAssignableTypes(AppDomain.CurrentDomain);
                foreach(var assignableType in assignableTypes)
                    VisitInternal(assignableType, visitedSet);
            }

            foreach(var propertyType in
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
            var baseType = type.ToEnumerable();
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

        public static System.Collections.Generic.IEnumerable<T> ToEnumerable<T>(this T item)
        {
            if (item == null)
                yield break;
            yield return item;
        }
    }
}
