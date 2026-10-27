using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using Δruntime = go.runtime_package;

namespace GolibTests;

// builtin.GetGoTypeName named a C# int (Go's int32, and rune) "int" and a C# byte (Go's uint8) "byte", where
// Go's %T and every runtime panic text say "int32" and "uint8"; Go's int is nint here. The mapping dated from
// when an untyped Go int could reach an interface as a C# int, which 96b13eec20 retired. Two panic texts read
// the name through it: the pinner's non-pointer refusal and the golib fallback of a failed type assertion.
// GoEqualityComparer's "hash of unhashable type" also calls it, but only for a value adapter whose wrapped
// value is null, which no primitive can be. MSTest runs this assembly serially (no [Parallelize]), which is
// what makes clearing the runtime's type-assertion hook safe for the length of an arm.
[TestClass]
public class GoTypeNamePanicTextTests
{
    [ClassInitialize]
    public static void RunRuntimeModuleInitializer(TestContext _) =>
        RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

    // Each primitive as Go's %T spells it, checked against GoReflect.GoTypeName, the renderer reflect uses.
    [TestMethod]
    public void EachPrimitiveIsNamedAsGoSpellsIt()
    {
        (object value, string want)[] rows =
        [
            (true, "bool"),
            ((nint)1, "int"),
            ((sbyte)1, "int8"),
            ((short)1, "int16"),
            (1, "int32"),
            (1L, "int64"),
            ((nuint)1, "uint"),
            ((byte)1, "uint8"),
            ((ushort)1, "uint16"),
            (1U, "uint32"),
            (1UL, "uint64"),
            ((uintptr)1, "uintptr"),
            (1.0F, "float32"),
            (1.0D, "float64"),
            (new Complex(1, 2), "complex128"),
            (new complex64(1, 2), "complex64"),
            ((@string)"s", "string")
        ];

        foreach ((object value, string want) in rows)
        {
            Assert.AreEqual(want, GetGoTypeName(value), $"GetGoTypeName({value.GetType().Name})");
            Assert.AreEqual(GoReflect.GoTypeName(value.GetType()), GetGoTypeName(value), $"{value.GetType().Name}: agrees with GoReflect");
        }
    }

    // runtime/pinner_impl.cs: Go's pinnerGetPtr names the argument's type, "runtime.Pinner: argument is not a pointer: int32".
    [TestMethod]
    public void PinOfANonPointerNamesItsGoType()
    {
        AssertPinRefuses(5, "int32");
        AssertPinRefuses((byte)1, "uint8");
        AssertPinRefuses((nint)5, "int");
        AssertPinRefuses((nuint)5, "uint");
        AssertPinRefuses(5U, "uint32");
    }

    // RuntimeErrorPanic.TypeAssertion's own text, used when no runtime has registered TypeAssertionError: Go says
    // "interface conversion: interface {} is int32, not string".
    [TestMethod]
    public void TheTypeAssertionFallbackNamesTheDynamicGoType()
    {
        var registered = RuntimeErrorPanic.TypeAssertionErrorValue;

        try
        {
            RuntimeErrorPanic.TypeAssertionErrorValue = null;

            AssertFallback(5, "int32");
            AssertFallback((byte)1, "uint8");
            AssertFallback((nint)5, "int");
        }
        finally
        {
            RuntimeErrorPanic.TypeAssertionErrorValue = registered;
        }
    }

    private static void AssertPinRefuses(object value, string goType)
    {
        Δruntime.Pinner pinner = new(nil);

        try
        {
            pinner.Pin(value);
            Assert.Fail($"Pin({goType}) must panic");
        }
        catch (PanicException ex)
        {
            Assert.AreEqual($"runtime error: runtime.Pinner: argument is not a pointer: {goType}", ex.Message, $"Pin({goType})");
        }
    }

    private static void AssertFallback(object value, string goType)
    {
        PanicException ex = RuntimeErrorPanic.TypeAssertion(typeof(object), value, typeof(@string));
        Assert.AreEqual($"interface conversion: interface {{}} is {goType}, not string", ex.Message, $"{goType}.(string)");
    }
}
