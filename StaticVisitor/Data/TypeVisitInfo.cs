using System;

namespace Sid.Tools.StaticVisitor
{
    public abstract class TypeVisit
    {
        protected TypeVisit(Type type)
        {
            @Type = type;
        }

        public Type @Type { get; }

        public override string ToString()
        {
            return $"[{@Type}]";
        }
    }
    
    public class InitialTypeTypeVisit : TypeVisit
    {
        public InitialTypeTypeVisit(Type initialType): base(initialType) {}
    }

    public abstract class InheritingTypeTypeVisit : TypeVisit
    {
        protected InheritingTypeTypeVisit(Type inheritingType): base(inheritingType) {}
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
        protected EncompassedTypeTypeVisit(Type encompassedType): base(encompassedType) {}
    }
    
    public class ParameterTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ParameterTypeTypeVisit(Type parameterType): base(parameterType) {}
    }

    public class ElementTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ElementTypeTypeVisit(Type elementType): base(elementType) {}
    }

    public class AssignableTypeTypeVisit : TypeVisit
    {
        public AssignableTypeTypeVisit(Type assignableType): base(assignableType) {}
    }
    
    public class PropertyTypeVisit : TypeVisit
    {
        public PropertyTypeVisit(Type propertyType, string propertyName):base(propertyType)
        {
            PropertyName = propertyName;
        }

        public Type PropertyType => @Type;

        public string PropertyName { get; }
    }
}