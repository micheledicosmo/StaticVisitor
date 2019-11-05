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
        private readonly Action<System.Collections.Generic.Stack<TypeVisit>> Action { get; }

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

        #region ctor List with Stack
        
        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visit stacks will be added</param>
        public StaticVisitor(out System.Collections.Generic.IList<System.Collections.Generic.Stack<TypeVisit>> visits)
        {
            visits = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var listClosure = visits;
            Action = stack => listClosure.Add(stack.Clone());
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visit stacks will be added</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(out System.Collections.Generic.IList<System.Collections.Generic.Stack<TypeVisit>> visits, StaticVisitorConfiguration configuration)
        {
            visits = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var listClosure = visits;
            Action = stack => listClosure.Add(stack.Clone());
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visit stacks will be added. Use an ordered collection to preserve order of visit</param>
        public StaticVisitor(System.Collections.Generic.ICollection<System.Collections.Generic.Stack<TypeVisit>> visits)
        {
            Action = x => visits.Add(x.Clone());
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="collection">The collection to which visit stacks will be added. Use an ordered collection to preserve order of visit</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(System.Collections.Generic.ICollection<System.Collections.Generic.Stack<TypeVisit>> visits, StaticVisitorConfiguration configuration)
        {
            Action = x => visits.Add(x.Clone());
            this.configuration = configuration;
        }
        #endregion

        #region ctor Action
            
        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="action">The action to be invoked when a stack is visited</param>
        public StaticVisitor(Action<System.Collections.Generic.Stack<TypeVisit>> action)
        {
            Action = action;
            configuration = new StaticVisitorConfiguration();
        }

        /// <summary>
        /// Creates a new instance of <see cref="StaticVisitor"/>
        /// </summary>
        /// <param name="action">The action to be invoked when a stack is visited</param>
        /// <param name="configuration">The custom configuration which defines the visitor behaviour</param>
        public StaticVisitor(Action<System.Collections.Generic.Stack<TypeVisit>> action, StaticVisitorConfiguration configuration)
        {
            Action = action;
            this.configuration = configuration;
        }

        #endregion

        /// <summary>
        /// Starts visiting the type
        /// </summary>
        /// <param name="type"></param>
        public void Visit(Type type)
        {
            VisitInternalWithStackWrapping(
                type,
                new System.Collections.Generic.Stack<TypeVisit>(),
                new System.Collections.Generic.HashSet<Type>(),
                new InitialTypeTypeVisit(type)
                );
        }

        private void VisitInternal(System.Collections.Generic.Stack<TypeVisit> stack, System.Collections.Generic.ISet<Type> visitedSet)
        {
            var type = stack.CurrentType();

            if (configuration.AvoidMultipleVisits && visitedSet.Contains(type))
                return;
            else
                visitedSet.Add(type);

            if (!configuration.TypeCanBeVisited(type))
                return;
            
            Action(stack);

            if (configuration.VisitInheritedTypes)
                foreach (var (inheritedType, stackEntry) in type.GetInheritedTypes())
                    VisitInternalWithStackWrapping(inheritedType, stack, visitedSet, stackEntry);

            if (configuration.VisitEncompassingTypes)
                foreach (var (encompassingType, stackEntry) in type.GetEncompassingTypes())
                    VisitInternalWithStackWrapping(encompassingType, stack, visitedSet, stackEntry);


            if (configuration.VisitAssignableTypesOf(type))
            {
                OnTrace($"Iterating through {type} assignable types now...", stackLevel);
                var assignableTypes = type.GetAssignableTypes(AppDomain.CurrentDomain);
                foreach (var (assignableType, stackEntry) in assignableTypes)
                    VisitInternalWithStackWrapping(assignableType, stack, visitedSet, stackEntry);
            }

            foreach (var (propertyType, stackEntry) in
                type
                    .GetProperties()
                    .Where(x => configuration.PropertyCanBeVisited(x))
                    .Select(x => (type: x.PropertyType, stackEntry: (TypeVisit)new PropertyTypeVisit(x.PropertyType, x.Name)))
                )
                    VisitInternalWithStackWrapping(propertyType, stack, visitedSet, stackEntry);
        }

        private void VisitInternalWithStackWrapping(
            Type type,
            System.Collections.Generic.Stack<TypeVisit> stack,
            System.Collections.Generic.ISet<Type> visitedSet,
            TypeVisit typeVisit)
        {
            stack.Push(typeVisit);
            VisitInternal(stack, visitedSet);
            stack.Pop();
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns all interfaces and type directly inherited by <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Collections.Generic.IEnumerable<(Type type, TypeVisit stackEntry)>
            GetInheritedTypes(this Type type)
        {
            var baseType = type.BaseType.ToEnumerable().Select(x => (type: x, stackEntry: (TypeVisit)new InheritingBaseTypeTypeVisit(x)));
            var interfaces = type.GetInterfaces().Select(x => (type: x, stackEntry: (TypeVisit)new InheritingInterfaceTypeVisit(x)));
            return baseType.Concat(interfaces);
        }

        /// <summary>
        /// Returns a named tuple with the given <paramref name="type"/> and its encompassing types (i.e. parameter types of generic types, and element type).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Collections.Generic.IEnumerable<(Type type, TypeVisit stackEntry)>
            GetEncompassingTypes(this Type type)
        {
            if (type == null)
                return Enumerable.Empty<(Type type, TypeVisit stackEntry)>();

            var parameterTypes = type.IsGenericType
                ? type.GenericTypeArguments.Where(y => y != type).Select(y => (type: y, stackEntry: (TypeVisit)new ParameterTypeTypeVisit(y)))
                : Enumerable.Empty<(Type type, TypeVisit stackEntry)>();

            var elementType = type.HasElementType
                ? type.GetElementType().ToEnumerable().Select(y => (type: y, stackEntry: (TypeVisit)new ElementTypeTypeVisit(y)))
                : Enumerable.Empty<(Type type, TypeVisit stackEntry)>();

            return parameterTypes.Concat(elementType);
        }

        public static System.Collections.Generic.IEnumerable<(Type type, TypeVisit stackEntry)>
            GetAssignableTypes(this Type type, AppDomain appDomain)
        {
            return appDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(x => type != x)
                .Where(type.IsAssignableFrom)
                .Select(y => (type: y, stackEntry: (TypeVisit)new AssignableTypeTypeVisit(y)));
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

        public static System.Collections.Generic.Stack<T> Clone<T>(this System.Collections.Generic.Stack<T> stack)
        {
            return new System.Collections.Generic.Stack<T>(new System.Collections.Generic.Stack<T>(stack));
        }
        
        /// <summary>
        /// Sugar for accessing the first element of the stack
        /// </summary>
        public static TypeVisit CurrentVisit(this System.Collections.Generic.Stack<TypeVisit> stack)
        {
            return stack.First();
        }

        /// <summary>
        /// Sugar for accessing the visited type in the first element of the stack
        /// </summary>
        public static Type CurrentType(this System.Collections.Generic.Stack<TypeVisit> stack)
        {
            return stack.CurrentVisit().Type;
        }
    }
}
