#nullable enable

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// Seat #34, gap 2: reflect's Method(i).Type closes a Func/Action family over the method's signature, which Native AOT
// refuses for a value type it never compiled (NotSupportedException, raw, ending the process: the i7's AotReflectGaps
// arm). Where the family cannot close, the receiver type's generated method set (go2cs-gen's IGoMethodSet) answers with
// a delegate compiled code created, and its type is the method's func type; a method no generated set answers is a Go
// panic naming it, never the raw exception. GoReflect.RefuseFamiliesForTest makes the JIT refuse the family as Native AOT
// does, so both routes run here. Each fixture type is read by one test only: the func-type caches are keyed by method,
// and a type an earlier test resolved would answer from them without reaching either route.
[TestClass]
public class AotMethodTypeFallbackTests
{
    private static T Refused<T>(Func<T> read)
    {
        GoReflect.RefuseFamiliesForTest = true;

        try
        {
            return read();
        }
        finally
        {
            GoReflect.RefuseFamiliesForTest = false;
        }
    }

    [TestMethod]
    public void TheFuncTypesComeFromTheGeneratedMethodSetWhenNoFamilyCloses()
    {
        Type t = typeof(aotfallback_package.Gen);
        Type p = typeof(ж<aotfallback_package.Gen>);
        int index = GoReflect.GoMethodIndex(t, "Add");

        Assert.AreEqual(typeof(Func<aotfallback_package.Gen, nint, nint>), Refused(() => GoReflect.GoMethodFuncType(t, index)), "Method(i).Type through T");
        Assert.AreEqual(typeof(Func<ж<aotfallback_package.Gen>, nint, nint>), Refused(() => GoReflect.GoMethodFuncType(p, GoReflect.GoMethodIndex(p, "Add"))), "Method(i).Type through *T");
        Assert.AreEqual(typeof(Func<nint, nint>), Refused(() => GoReflect.GoMethodValueType(t, index)), "the method value's type");

        Delegate func = Refused(() => GoReflect.GoMethodFunc(t, index))!;
        Delegate value = Refused(() => GoReflect.GoMethodValue(t, index, new aotfallback_package.Gen { n = 2 }));

        Assert.AreEqual((nint)3, ((Func<aotfallback_package.Gen, nint, nint>)func)(new aotfallback_package.Gen { n = 1 }, 2), "Method(i).Func calls");
        Assert.AreEqual((nint)7, ((Func<nint, nint>)value)(5), "Value.Method(i) calls");
    }

    [TestMethod]
    public void WithoutAGeneratedMethodSetTheRefusalIsAGoPanic()
    {
        Type t = typeof(aotfallback_package.Bare);
        int index = GoReflect.GoMethodIndex(t, "Add");

        PanicException func = Assert.ThrowsException<PanicException>(() => Refused(() => GoReflect.GoMethodFuncType(t, index)), "Method(i).Type");
        PanicException value = Assert.ThrowsException<PanicException>(() => Refused(() => GoReflect.GoMethodValueType(t, index)), "the method value's type");

        StringAssert.Contains(func.Message, "no func type for method Add");
        StringAssert.Contains(value.Message, "no method value type for method Add");
    }
}

public static class aotfallback_package
{
    // Arms as go2cs-gen writes them (MethodSet.cs): the unbound func through T and through *T, and the method value
    // bound through *T, whose box the arm only casts.
    public struct Gen : IGoMethodSet
    {
        public nint n;

        readonly Delegate? IGoMethodSet.ᴛMethod(string method, bool pointer, bool bound, object? receiver) => (method, pointer, bound) switch
        {
            ("Add", false, false) => new Func<Gen, nint, nint>(Add),
            ("Add", false, true) => BindValue((Gen)receiver!),
            ("Add", true, false) => new Func<ж<Gen>, nint, nint>(static (box, d) => Add(box.Value, d)),
            ("Add", true, true) => BindBox((ж<Gen>?)receiver),
            _ => null
        };

        private static Func<nint, nint> BindValue(Gen g) => d => Add(g, d);

        private static Func<nint, nint> BindBox(ж<Gen>? box) => d => Add(box!.Value, d);
    }

    // The same method with no generated set.
    public struct Bare
    {
        public nint n;
    }

    public static nint Add(this Gen g, nint d) => g.n + d;

    public static nint Add(this Bare b, nint d) => b.n + d;
}
