using System;

namespace Sid.Tools.StaticVisitor.Core
{
    public abstract class TypeVisit { }
    
    public class InitialTypeTypeVisit : TypeVisit
    {
        public InitialTypeTypeVisit(Type initialType)
        {
            InitialType = initialType;
        }

        public Type InitialType { get; }
    }

    public abstract class InheritingTypeTypeVisit : TypeVisit
    {
        public InheritingTypeTypeVisit(Type inheritingType)
        {
            InheritingType = inheritingType;
        }

        public Type InheritingType { get; }
    }

    public class InheritingBaseTypeTypeVisit : InheritingTypeTypeVisit
    {
        public InheritingBaseTypeTypeVisit(Type baseType): base(baseType){}
    }

    public class InheritingInterfaceTypeVisit : InheritingTypeTypeVisit
    {
        public InheritingInterfaceTypeVisit(Type @interface): base(@interface){}
    }

    public abstract class EncompassedTypeTypeVisit : TypeVisit
    {

    }
    
    public class ParameterTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ParameterTypeTypeVisit(Type parameterType)
        {
            ParameterType = parameterType;
        }

        public Type ParameterType { get; }
    }

    public class ElementTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ElementTypeTypeVisit(Type elementType)
        {
            ElementType = elementType;
        }

        public Type ElementType { get; }
    }

    public class AssignedTypeTypeVisit : TypeVisit
    {
        public AssignedTypeTypeVisit(Type assignableType)
        {
            AssignableType = assignableType;
        }

        public Type AssignableType { get; }
    }
    
    public class PropertyTypeVisit : TypeVisit
    {
        public PropertyTypeVisit(Type propertyType, string propertyName)
        {
            PropertyType = propertyType;
            PropertyName = propertyName;
        }

        public Type PropertyType { get; }

        public string PropertyName { get; }
    }
}