using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

[TestClass]
public class ClrPrimitiveKindTests
{
    // Every CLR primitive is a struct whose one instance field is of its OWN type: System.Char holds a
    // `char m_value`, System.Int32 an `int m_value`. C# forbids a user struct from containing itself
    // (CS0523), but the runtime's primitives are the exception, so a walk that descends into a type's
    // fields never ends on one. KindOf's Struct is the one kind every such walk descends into
    // (StructFieldsComparable, the field projections), so a primitive that falls through to Struct
    // overflows the stack, and a .NET stack overflow cannot be caught: it kills the process.
    //
    // System.Char was that primitive. No Go type converts to it, but a C# lambda whose body returns a
    // bare 'a' infers Func<int, char>, and P2's #23 r2 base emitted exactly that: fmt's %v and %T of
    // it died with C# exit 134 "Stack overflow." at GoReflect.IsComparable -> StructFieldsComparable
    // (58,048 frames, measured on linux at 3d6155ed3a). A char is a UTF-16 code unit, so it is Go's
    // uint16 (seat #40).
    private static readonly Type[] s_primitives =
    [
        typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int),
        typeof(uint), typeof(long), typeof(ulong), typeof(nint), typeof(nuint), typeof(float), typeof(double)
    ];

    [TestMethod]
    public void TheListIsEveryClrPrimitive()
    {
        // The runtime's own answer, so a primitive the list misses fails here rather than going unguarded.
        int primitives = 0;

        foreach (Type type in typeof(object).Assembly.GetExportedTypes())
        {
            if (!type.IsPrimitive)
                continue;

            primitives++;
            Assert.IsTrue(Array.IndexOf(s_primitives, type) >= 0, $"{type} is a CLR primitive the list does not hold");
        }

        Assert.AreEqual(s_primitives.Length, primitives);
    }

    [TestMethod]
    public void EveryClrPrimitiveIsAGoScalarNotAStruct()
    {
        // Every kind is checked before any walk runs: at a base where one primitive still answers Struct,
        // this fails by name instead of overflowing on IsComparable below.
        foreach (Type type in s_primitives)
            Assert.AreNotEqual(GoReflect.Struct, GoReflect.KindOf(type), $"{type} classifies as Struct, whose walk never ends on it");

        foreach (Type type in s_primitives)
            Assert.IsTrue(GoReflect.IsComparable(type), $"{type} is comparable in Go");
    }

    [TestMethod]
    public void ACharIsGoUint16()
    {
        Assert.AreEqual(GoReflect.Uint16, GoReflect.KindOf(typeof(char)));
        Assert.AreEqual("uint16", GoReflect.GoTypeName(typeof(char)));

        // The func types P2's base inferred: the name never carries the CLR's `Char`.
        Assert.AreEqual("func(int32) uint16", GoReflect.GoTypeName(typeof(Func<int, char>)));
        Assert.AreEqual("func() (uint16, bool)", GoReflect.GoTypeName(typeof(Func<(char, bool)>)));
    }
}
