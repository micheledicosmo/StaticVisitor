using System;

namespace Sid.Tools.StaticVisitor
{
    /// <summary>
    /// Represents the visit to a type
    /// </summary>
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
    
    /// <summary>
    /// Represents the visit to the type passed to the visitor
    /// </summary>
    public class InitialTypeTypeVisit : TypeVisit
    {
        public InitialTypeTypeVisit(Type initialType): base(initialType) {}
        
        public override string ToString()
        {
            return $"{base.ToString()} (initial)";
        }
    }

    /// <summary>
    /// Represents the visit to a type caused by the previous type inheriting it
    /// </summary>
    public abstract class InheritingTypeTypeVisit : TypeVisit
    {
        protected InheritingTypeTypeVisit(Type inheritingType): base(inheritingType) {}
    }

    /// <summary>
    /// Represents the visit to a type caused by it being the base type of the previous type
    /// </summary>
    public class InheritingBaseTypeTypeVisit : InheritingTypeTypeVisit
    {
        public InheritingBaseTypeTypeVisit(Type baseType): base(baseType){}
        
        public override string ToString()
        {
            return $"{base.ToString()} (base type)";
        }
    }
    
    /// <summary>
    /// Represents the visit to a type (an interface) caused by the previous type implementing it
    /// </summary>
    public class InheritingInterfaceTypeVisit : InheritingTypeTypeVisit
    {
        public InheritingInterfaceTypeVisit(Type @interface): base(@interface){}
        
        public override string ToString()
        {
            return $"{base.ToString()} (implemented)";
        }
    }

    /// <summary>
    /// Represents the visit to a type caused by the previous type encompassing it
    /// </summary>
    public abstract class EncompassedTypeTypeVisit : TypeVisit
    {
        protected EncompassedTypeTypeVisit(Type encompassedType): base(encompassedType) {}
    }
    
    /// <summary>
    /// Represents the visit to a type cause by the previous type being a generic type and specifying it as a parameter type
    /// </summary>
    public class ParameterTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ParameterTypeTypeVisit(Type parameterType): base(parameterType) {}
        
        public override string ToString()
        {
            return $"{base.ToString()} (parameter type)";
        }
    }
    
    /// <summary>
    /// Represents the visit to a type cause by the previous type having it as an element type (e.g. being an array of this type)
    /// </summary>
    public class ElementTypeTypeVisit : EncompassedTypeTypeVisit
    {
        public ElementTypeTypeVisit(Type elementType): base(elementType) {}
        
        public override string ToString()
        {
            return $"{base.ToString()} (element type)";
        }
    }
    
    /// <summary>
    /// Represents the visit to a type cause by it being a derived type of (assignable type to) the previous type
    /// </summary>
    public class AssignableTypeTypeVisit : TypeVisit
    {
        public AssignableTypeTypeVisit(Type assignableType): base(assignableType) {}
        
        public override string ToString()
        {
            return $"{base.ToString()} (assignable)";
        }
    }
    
    /// <summary>
    /// Represents the visit to a type cause by the previous type having a property of this type
    /// </summary>
    public class PropertyTypeVisit : TypeVisit
    {
        public PropertyTypeVisit(Type propertyType, string propertyName):base(propertyType)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
        
        public override string ToString()
        {
            return $"{base.ToString()} (property)";
        }
    }
}
