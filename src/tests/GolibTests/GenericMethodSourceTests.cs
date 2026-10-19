using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using GoTypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// A Go method of a generic type is emitted as a generic extension method (`String<K, V>(this Pair<K, V>)`), and golib's
// method-set scan finds its OPEN definition. The candidate source closes it over the value's type arguments
// (GoTypeExtensions.CloseGoMethod, G's design of 2026-10-19), so no consumer is ever handed an open method:
//   1. through the type's generated IGoMethodSource (go2cs-gen), the only route Native AOT can always answer;
//   2. with dynamic code, by unifying the receiver with the value's element type;
//   3. otherwise refused BY NAME, as a Go panic.
// Each fixture type is used by one route only: the candidate cache is keyed by the closed element type.
[TestClass]
public class GenericMethodSourceTests
{
    private static string[] Names(Type t) =>
        [.. Enumerable.Range(0, GoReflect.GoMethodCount(t)).Select(i => GoReflect.GoMethodName(t, i))];

    // Route 1, with dynamic code switched off: the source answers every method, value and pointer, and what the
    // candidate source hands out IS the source's closed method.
    [TestMethod]
    public void TheGeneratedSourceClosesEveryMethodWithoutDynamicCode()
    {
        GoTypeExtensions.DynamicCodeSupportedOverride = false;

        try
        {
            Type value = typeof(genmeth_package.Pair<@string, nint>);

            CollectionAssert.AreEqual(new[] { "Key", "String" }, Names(value));
            CollectionAssert.AreEqual(new[] { "Key", "String", "Swap" }, Names(typeof(ж<genmeth_package.Pair<@string, nint>>)));

            MethodInfo[] candidates = [.. GoTypeExtensions.GetGoMethodSetCandidates(value, true)];
            IGoMethodSource source = default(genmeth_package.Pair<@string, nint>);

            Assert.IsFalse(candidates.Any(static candidate => candidate.ContainsGenericParameters), "no candidate is open");
            Assert.AreEqual(source.GoMethod("String", false), candidates.Single(static candidate => candidate.Name == "String"));
            Assert.AreEqual(source.GoMethod("Swap", true), candidates.Single(static candidate => candidate.Name == "Swap"));

            Delegate func = GoReflect.GoMethodFunc(value, 1)!;
            Assert.AreEqual((@string)"z=26", func.DynamicInvoke(new genmeth_package.Pair<@string, nint>("z", 26)));
        }
        finally
        {
            GoTypeExtensions.DynamicCodeSupportedOverride = null;
        }
    }

    // Route 1 serves the interface probe too: a two-parameter generic satisfies fmt.Stringer's shape by full signature.
    [TestMethod]
    public void TheClosedMethodSatisfiesAnInterfaceBySignature()
    {
        Assert.IsTrue(typeof(genmeth_package.Pair<@string, int>).StructurallyImplements(typeof(genmeth_package.Stringer)));
        Assert.IsTrue(typeof(ж<genmeth_package.Pair<@string, int>>).StructurallyImplements(typeof(genmeth_package.Swapper)));
        Assert.IsFalse(typeof(genmeth_package.Pair<@string, int>).StructurallyImplements(typeof(genmeth_package.Swapper)), "Swap is a pointer-receiver method");
    }

    // Route 2: no source, dynamic code. The receiver unifies position by position, whatever order the emitted method
    // declares its type parameters in (`Second<B, A>(this Duo<A, B>)`).
    [TestMethod]
    public void WithoutASourceDynamicCodeClosesByReceiverUnification()
    {
        Type value = typeof(genmeth_package.Duo<@string, double>);
        MethodInfo second = GoTypeExtensions.GetGoMethodSetCandidates(value, false).Single(static candidate => candidate.Name == "Second");

        Assert.IsFalse(second.ContainsGenericParameters);
        CollectionAssert.AreEqual(new[] { typeof(double), typeof(@string) }, second.GetGenericArguments());
        Assert.AreEqual(1.5, GoReflect.GoMethodFunc(value, 0)!.DynamicInvoke(new genmeth_package.Duo<@string, double>("a", 1.5)));
    }

    // Route 3: no source and no dynamic code. The method set is refused BY NAME, as a Go panic a recover sees, never an
    // open method handed to a binder and never a CLR exception that ends the process.
    [TestMethod]
    public void WithoutASourceOrDynamicCodeTheMethodIsRefusedByName()
    {
        GoTypeExtensions.DynamicCodeSupportedOverride = false;

        try
        {
            PanicException panic = Assert.ThrowsException<PanicException>(() => GoReflect.GoMethodCount(typeof(genmeth_package.Duo<sbyte, byte>)));

            StringAssert.Contains(panic.Message, "Second");
            StringAssert.Contains(panic.Message, "Duo");
        }
        finally
        {
            GoTypeExtensions.DynamicCodeSupportedOverride = null;
        }
    }

    // golib's OWN generic extension methods (`slice<T>(this in slice<T>)`) are plumbing, never Go methods, and are not
    // closed over a []int's element: no candidate of a []int is generic. (Its `this object` candidates are golib plumbing
    // too, admitted by the candidate source and dropped from the method TABLE by GetGoMethodSetEntries.)
    [TestMethod]
    public void GolibsOwnGenericExtensionMethodsAreNotCandidates()
    {
        Assert.IsFalse(GoTypeExtensions.GetGoMethodSetCandidates(typeof(slice<nint>), true).Any(static candidate => candidate.IsGenericMethod));
        Assert.AreEqual(0, GoReflect.GoMethodCount(typeof(slice<nint>)));
    }

    // CreateStaticDelegate binds what it is handed: an open method has no delegate, and it no longer guesses a closing
    // from the receiver's FIRST type argument (which was right for one type parameter and wrong for two).
    [TestMethod]
    public void CreateStaticDelegateDoesNotCloseAnOpenMethod()
    {
        MethodInfo open = typeof(genmeth_package).GetMethod(nameof(genmeth_package.Second))!;

        Assert.IsNull(open.CreateStaticDelegate(typeof(Func<genmeth_package.Duo<@string, double>, double>)));
    }

    // A method reflect cannot bind is a Go panic naming the type and the method.
    [TestMethod]
    public void AnUnbindableMethodFuncIsAGoPanicNamingTheMethod()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => GoReflect.GoMethodFunc(typeof(genmeth_package.Odd), 0));

        StringAssert.Contains(panic.Message, "Out");
    }

    // The same holds for the BOUND method value (reflect's Value.Method), whose binder is a separate compile.
    [TestMethod]
    public void AnUnbindableMethodValueIsAGoPanicNamingTheMethod()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => GoReflect.GoMethodValue(typeof(genmeth_package.Odd), 0, default(genmeth_package.Odd)));

        StringAssert.Contains(panic.Message, "Out");
        StringAssert.Contains(panic.Message, "Odd");
    }
}

public static class genmeth_package
{
    public interface Stringer
    {
        @string String();
    }

    public interface Swapper
    {
        int Swap(int v);
    }

    // The emitted shape of `type Pair[K comparable, V any] struct{ k K; v V }` with its generated method source.
    public struct Pair<K, V>(K k, V v) : IGoMethodSource
    {
        public K k = k;
        public V v = v;

        private delegate K ᴛGoMethod0(Pair<K, V> arg0);
        private delegate @string ᴛGoMethod1(Pair<K, V> arg0);
        private delegate V ᴛGoMethod2(ж<Pair<K, V>> recv, V arg1);

        MethodInfo? IGoMethodSource.GoMethod(string name, bool pointerReceiver) => (name, pointerReceiver) switch
        {
            ("Key", false) => ((ᴛGoMethod0)Key<K, V>).Method,
            ("String", false) => ((ᴛGoMethod1)String<K, V>).Method,
            ("Swap", true) => ((ᴛGoMethod2)Swap<K, V>).Method,
            _ => null
        };
    }

    // A generic type with NO method source: route 2, or the refusal.
    public struct Duo<A, B>(A a, B b)
    {
        public A a = a;
        public B b = b;
    }

    public struct Odd;

    public static K Key<K, V>(this Pair<K, V> p) => p.k;

    public static @string String<K, V>(this Pair<K, V> p) => $"{p.k}={p.v}";

    [GoRecv] public static V Swap<K, V>(this ref Pair<K, V> p, V v)
    {
        V old = p.v;
        p.v = v;
        return old;
    }

    public static V Swap<K, V>(this ж<Pair<K, V>> Ꮡp, V v) => Ꮡp.Value.Swap(v);

    public static B Second<B, A>(this Duo<A, B> d) => d.b;

    // An out parameter has no Func<> form, so reflect cannot build this method's func.
    public static void Out(this Odd o, out nint n) => n = 1;
}
