using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sid.Tools.StaticVisitor.Core.Tests
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
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
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
            var actual = new System.Collections.Generic.List<System.Type>();
            var visitor = new StaticVisitor(actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
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
            var actual = new System.Collections.Generic.List<System.Type>();
            var visitor = new StaticVisitor(actual, configuration);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(configurationCalled);
        }

        [TestMethod]
        public void ActionCtorOk()
        {
            var actual = new System.Collections.Generic.List<System.Type>();
            var visitor = new StaticVisitor(x => actual.Add(x));
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 1);
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
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
            var actual = new System.Collections.Generic.List<System.Type>();
            var visitor = new StaticVisitor(x => actual.Add(x), configuration);
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
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
            Assert.IsTrue(actual.Contains(typeof(object)));
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
            var visitor = new StaticVisitor(out var actual);
            visitor.Visit(typeof(DataStructure));
            Assert.IsTrue(actual.Count == 2);
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
            Assert.IsTrue(actual.Contains(typeof(PropertyObject)));
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
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
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
            Assert.IsTrue(actual.Contains(typeof(DataStructureA)));
            Assert.IsTrue(actual.Contains(typeof(DataStructureB)));
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
            Assert.IsTrue(actual.Contains(typeof(DataStructureA)));
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
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
            Assert.IsTrue(actual.Contains(typeof(IInterface1)));
            Assert.IsTrue(actual.Contains(typeof(IInterface2)));
            Assert.IsTrue(actual.Contains(typeof(IInterfaceBase)));
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
            Assert.IsTrue(actual.Contains(typeof(DataStructure)));
            Assert.IsTrue(actual.Contains(typeof(IInterface1)));
            Assert.IsTrue(actual.Contains(typeof(IInterfaceBase)));
            Assert.IsTrue(actual.Contains(typeof(IInterfaceBase)));
            Assert.IsTrue(actual.Contains(typeof(IInterface2)));
            Assert.IsTrue(actual.Contains(typeof(IInterfaceBase)));
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
        public void GenericTypeOk()
        {
            var visitor = new StaticVisitor(out var actual, new StaticVisitorConfiguration()
            {
                VisitInheritedTypes = false
            });
            visitor.Visit(typeof(GenericType<EncompassedType>));
            Assert.IsTrue(actual.Count == 2);
            Assert.IsTrue(actual.Contains(typeof(GenericType<EncompassedType>)));
            Assert.IsTrue(actual.Contains(typeof(EncompassedType)));
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
            Assert.IsTrue(actual.Contains(typeof(EncompassedType[])));
            Assert.IsTrue(actual.Contains(typeof(EncompassedType)));
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
            Assert.IsTrue(actual.Contains(typeof(EncompassedType[])));
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
                VisitAssignableTypes = true
            });
            visitor.Visit(typeof(DataStructureA));
            Assert.IsTrue(actual.Count == 2);
            Assert.IsTrue(actual.Contains(typeof(DataStructureA)));
            Assert.IsTrue(actual.Contains(typeof(DataStructureB)));
        }
    }
}
