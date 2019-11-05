using System;

namespace Sid.Tools.StaticVisitor.Core
{
    public abstract class StackEntry { }
    
    public class InitialTypeStackEntry : StackEntry
    {
        public InitialTypeStackEntry(Type initialType)
        {
            InitialType = initialType;
        }

        public Type InitialType { get; }
    }

    public abstract class InheritingTypeStackEntry : StackEntry
    {
        public InheritingTypeStackEntry(Type inheritingType)
        {
            InheritingType = inheritingType;
        }

        public Type InheritingType { get; }
    }

    public class InheritingBaseTypeStackEntry : InheritingTypeStackEntry
    {
        public InheritingBaseTypeStackEntry(Type baseType): base(baseType){}
    }

    public class InheritingInterfaceStackEntry : InheritingTypeStackEntry
    {
        public InheritingInterfaceStackEntry(Type @interface): base(@interface){}
    }

    public abstract class EncompassedTypeStackEntry : StackEntry
    {

    }
    
    public class ParameterTypeStackEntry : EncompassedTypeStackEntry
    {
        public ParameterTypeStackEntry(Type parameterType)
        {
            ParameterType = parameterType;
        }

        public Type ParameterType { get; }
    }

    public class ElementTypeStackEntry : EncompassedTypeStackEntry
    {
        public ElementTypeStackEntry(Type elementType)
        {
            ElementType = elementType;
        }

        public Type ElementType { get; }
    }

    public class AssignedTypeStackEntry : StackEntry
    {
        public AssignedTypeStackEntry(Type assignableType)
        {
            AssignableType = assignableType;
        }

        public Type AssignableType { get; }
    }
    
    public class PropertyStackEntry : StackEntry
    {
        public PropertyStackEntry(Type propertyType, string propertyName)
        {
            PropertyType = propertyType;
            PropertyName = propertyName;
        }

        public Type PropertyType { get; }

        public string PropertyName { get; }
    }
}