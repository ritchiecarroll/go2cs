using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.ProxyReflect;

namespace go.ProxyReflect
{
    // The shape go2cs-gen emits for Go's `lib.Ord2[T Lesser[T]]` instantiated at `*elem.Node` from another package:
    // the type argument is lib's generic constraint proxy `жLesser<elem.Node>`, a pointer-sourced adapter implementing
    // the constraint closed over ITSELF. Nested in `<pkg>_package` classes, as converted types are, so their Go names and
    // package paths derive as the converter's do.
    public static class elem_package
    {
        public struct Node { public nint K; }
    }

    public static class lib_package
    {
        public interface Lesser<T> { bool Less(T _); }

        public interface Stringer { string String(); }

        public struct Ord2<T> where T : Lesser<T> { public T V; }

        public sealed class жLesser<E> : Lesser<жLesser<E>>, IжAdapter
        {
            private readonly ж<E> m_box;

            public жLesser(ж<E> box) => m_box = box;

            public object? Box => m_box;

            bool Lesser<жLesser<E>>.Less(жLesser<E> _) => false;
        }

        // An INTERFACE pointer adapter: pointer-sourced too, but its interface is not closed over itself.
        public sealed class NodeжStringer : Stringer, IжAdapter
        {
            private readonly ж<elem_package.Node> m_box;

            public NodeжStringer(ж<elem_package.Node> box) => m_box = box;

            public object? Box => m_box;

            string Stringer.String() => "node";
        }
    }
}

namespace GolibTests
{
    // A constraint proxy is, to reflection, the pointer it stands for (R10). Two places did not apply that: an
    // instantiated type's NAME read the proxy's own package path (`lib.Ord2[ProxyReflect/lib.жLesser[...]]` where Go prints
    // `lib.Ord2[*ProxyReflect/elem.Node]`), and reflect.Set refused to store a *T in a proxy-typed field ("value of type
    // *elem.Node is not assignable to type *elem.Node"). The behavioral ForeignConstraintProxy project prints both.
    [TestClass]
    public class ConstraintProxyReflectionTests
    {
        [TestMethod]
        public void AProxyTypeArgumentNamesThePointerItStandsFor()
        {
            Assert.AreEqual("lib.Ord2[*ProxyReflect/elem.Node]", GoReflect.GoTypeName(typeof(lib_package.Ord2<lib_package.жLesser<elem_package.Node>>)));
        }

        [TestMethod]
        public void APointerStoresIntoAProxySlotWrappedInANewProxy()
        {
            ж<elem_package.Node> box = builtin.Ꮡ(new elem_package.Node { K = 9 });

            Assert.IsTrue(GoReflect.TryMarshalAssignable(box, typeof(lib_package.жLesser<elem_package.Node>), out object? marshalled, GoReflect.GoTypeRelation.Assignable));
            Assert.IsInstanceOfType(marshalled, typeof(lib_package.жLesser<elem_package.Node>));
            Assert.AreSame(box, ((IжAdapter)marshalled!).Box);
        }

        [TestMethod]
        public void AnInterfaceAdapterIsNotAProxySlot()
        {
            ж<elem_package.Node> box = builtin.Ꮡ(new elem_package.Node { K = 9 });

            Assert.IsFalse(GoReflect.TryMarshalAssignable(box, typeof(lib_package.NodeжStringer), out _, GoReflect.GoTypeRelation.Assignable));
        }
    }
}
