using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Sid.Tools.StaticVisitor.Tests
{
    [TestClass]
    public class Basic
    {
        private class DataStructure { }

        [TestMethod]
        public void OutCtorOk()
        {
            var visitor = new StaticVisitor(out var actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
        }

        [TestMethod]
        public void OutCtorWithConfigurationOk()
        {
            var configurationCalled = false;
            var configuration = new StaticVisitorConfiguration
            {
                TypeCanBeVisited = x =>
                {
                    configurationCalled = true;
                    return false;
                }
            };
            var visitor = new StaticVisitor(out var actual, configuration);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(configurationCalled);
        }

        [TestMethod]
        public void CollectionCtorOk()
        {
            var actual = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var visitor = new StaticVisitor(actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
        }

        [TestMethod]
        public void CollectionCtorWithConfigurationOk()
        {
            var configurationCalled = false;
            var configuration = new StaticVisitorConfiguration
            {
                TypeCanBeVisited = x =>
                {
                    configurationCalled = true;
                    return false;
                }
            };
            var actual = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var visitor = new StaticVisitor(actual, configuration);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(configurationCalled);
        }

        [TestMethod]
        public void ActionCtorOk()
        {
            var actual = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var visitor = new StaticVisitor(stack => actual.Add(stack.Clone()));
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
        }

        [TestMethod]
        public void ActionCtorWithConfigurationOk()
        {
            var configurationCalled = false;
            var configuration = new StaticVisitorConfiguration
            {
                TypeCanBeVisited = x =>
                {
                    configurationCalled = true;
                    return false;
                }
            };
            var actual = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var visitor = new StaticVisitor(stack => actual.Add(stack.Clone()), configuration);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(configurationCalled);
        }

        [TestMethod]
        public void TypeCanBeVisitedOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                TypeCanBeVisited = x => true,
            });
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 2);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
            Assert.IsTrue(actual[0].CurrentType() == typeof(object));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingBaseTypeTypeVisit));
        }
    }

    [TestClass]
    public class Properties
    {
        private class DataStructure
        {
            public PropertyObject Property { get; set; }
        }

        private class PropertyObject { }

        [TestMethod]
        public void BasicOk()
        {
            var actual = new System.Collections.Generic.List<System.Collections.Generic.Stack<TypeVisit>>();
            var visitor = new StaticVisitor(actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 2);
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(PropertyObject));
            Assert.IsTrue(actual[0].CurrentVisit() is PropertyTypeVisit));
            Assert.IsTrue(((PropertyTypeVisit)actual[0].CurrentVisit()).PropertyName == "Property"));
        }

        [TestMethod]
        public void CustomFilterOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                PropertyCanBeVisited = x => false
            });
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
        }
    }

    [TestClass]
    public class Inherited
    {
        private class DataStructureA : DataStructureB { }

        private class DataStructureB { }

        [TestMethod]
        public void BasicOk()
        {
            var visitor = new StaticVisitor(out var actual);
            visitor.Visit(typeof(DataStructureA));
            Assert.IsTrue(actual.Count == 2);
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructureA));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));

            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructureB));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingBaseTypeTypeVisit));
        }

        [TestMethod]
        public void DisabledOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitInheritedTypes = false
            });
            visitor.Visit(typeof(DataStructureA));
            Assert.IsTrue(actual.Count == 1);

            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructureA));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
        }
    }

    [TestClass]
    public class Implemented
    {
        private class DataStructure : IInterface1, IInterface2 { }

        private interface IInterface1 : IInterfaceBase { }
        private interface IInterface2 : IInterfaceBase { }
        private interface IInterfaceBase { }

        [TestMethod]
        public void BasicOk()
        {
            var visitor = new StaticVisitor(out var actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 4);

            
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterface1));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterface2));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterfaceBase));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
        }

        [TestMethod]
        public void AllowMultipleVisitsOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                AvoidMultipleVisits = false
            });
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 6);
            
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructure));
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterface1));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterfaceBase));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterface2));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterfaceBase));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(IInterfaceBase));
            Assert.IsTrue(actual[0].CurrentVisit() is InheritingInterfaceTypeVisit));
                && x.CurrentVisit() is InheritingInterfaceTypeVisit));
        }
    }

    [TestClass]
    public class Encompassing
    {
        private class GenericType<T>
        {
        }

        private class EncompassedType { }

        [TestMethod]
        public void ParameterTypeOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitInheritedTypes = false
            });
            visitor.Visit(typeof(GenericType<EncompassedType>));
            
            Assert.IsTrue(actual.Count == 2);
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(GenericType<EncompassedType>);
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));

            Assert.IsTrue(actual[0].CurrentType() == typeof(EncompassedType));
            Assert.IsTrue(actual[0].CurrentVisit() is ParameterTypeTypeVisit));
        }

        [TestMethod]
        public void ElementTypeOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitInheritedTypes = false
            });
            visitor.Visit(typeof(EncompassedType[]));
            Assert.IsTrue(actual.Count == 2);
            
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(EncompassedType[]);
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(EncompassedType);
            Assert.IsTrue(actual[0].CurrentVisit() is ElementTypeTypeVisit));

        }

        [TestMethod]
        public void DisabledOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitInheritedTypes = false,
                VisitEncompassingTypes = false
            });
            visitor.Visit(typeof(EncompassedType[]));
            Assert.IsTrue(actual.Count == 1);
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(EncompassedType[]);
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));

        }
    }

    [TestClass]
    public class Assignable
    {
        private class DataStructureA { }

        private class DataStructureB : DataStructureA { }

        [TestMethod]
        public void BasicOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitAssignableTypesOf = t => t.Namespace == typeof(Assignable).Namespace
            });
            visitor.Visit(typeof(DataStructureA));
            Assert.IsTrue(actual.Count == 2);
            
            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructureA);
            Assert.IsTrue(actual[0].CurrentVisit() is InitialTypeTypeVisit));

            Assert.IsTrue(actual[0].CurrentType() == typeof(DataStructureB);
            Assert.IsTrue(actual[0].CurrentVisit() is AssignableTypeTypeVisit));
        }
    }
}
