// TrimStage3c2aTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using sort = go.sort_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// Trim stage 3c-2a (docs/PLAN-golib-full-trim.md, section 9): the reflect sites that started from a TYPE and closed a
// generic helper over it now ask a zero value of the type for its face (a slice's and an array's default members,
// ISupportMake's), or, for a struct conversion, copy field-wise. These pin each entry point's answers. The slice-header
// boxes are pinned by SliceHeaderReinterpretTests and HeaderSliceReinterpretTests, TryByteSliceAs by
// GoReflectBridgeClosureTests.
[TestClass]
public class TrimStage3c2aTests
{
    private struct Plain { public nint A; public object? B; }

    private struct WithRefsA { public @string S; public slice<int> Sl; public ж<int>? P; }

    private struct WithRefsB { public @string S; public slice<int> Sl; public ж<int>? P; }

    private struct InnerA { public nint X; public @string S; }

    private struct InnerB { public nint X; public @string S; }

    private struct OuterA { public InnerA In; public nint N; }

    private struct OuterB { public InnerB In; public nint N; }

    private struct Pair { public nint X; public nint Y; }

    private struct WrappedPair { public Pair Value; }

    private struct RefFieldA { public object? O; }

    private struct RefFieldB { public string? O; }

    private struct TwoInts { public int A; public int B; }

    private struct OneLong { public long X; }

    [TestMethod]
    public void AZeroValueIsTheTypesDefault()
    {
        object? nilSlice = GoReflect.DefaultValueOf(typeof(slice<int>));
        Assert.IsInstanceOfType(nilSlice, typeof(slice<int>), "a container struct's zero is the zero struct, not null");
        Assert.AreEqual((nint)0, ((slice<int>)nilSlice!).Length);

        Plain plain = (Plain)GoReflect.DefaultValueOf(typeof(Plain))!;
        Assert.AreEqual((nint)0, plain.A);
        Assert.IsNull(plain.B);

        Assert.IsNull(GoReflect.DefaultValueOf(typeof(ж<int>)), "a reference type's zero is null");
        Assert.IsNull(GoReflect.DefaultValueOf(typeof(int?)), "a Nullable boxes to null, as default(T) does");
    }

    [TestMethod]
    public void ANestedSizedArrayBuildsEveryLevel()
    {
        // [2][3]int: the inner length lives only in the dims, so each element must be built at it.
        array<array<int>> outer = (array<array<int>>)GoReflect.MakeSizedArray(typeof(array<array<int>>), [2, 3], 0);

        Assert.AreEqual((nint)2, outer.Length);

        for (int i = 0; i < 2; i++)
            Assert.AreEqual((nint)3, outer[i].Length, $"element {i} is built at the inner length");

        outer[0][1] = 7;
        Assert.AreEqual(0, outer[1][1], "each element has its own backing");

        // [2][2][2]int: three levels, through the outer fill at each.
        array<array<array<int>>> deep = (array<array<array<int>>>)GoReflect.MakeSizedArray(typeof(array<array<array<int>>>), [2, 2, 2], 0);
        Assert.AreEqual((nint)2, deep[1][1].Length);
    }

    [TestMethod]
    public void AContainerIsMadeThroughItsOwnMake()
    {
        slice<int> made = (slice<int>)GoReflect.MakeContainer(typeof(slice<int>), 3, 5);
        Assert.AreEqual((nint)3, made.Length);
        Assert.AreEqual((nint)5, made.Capacity);

        map<@string, int> m = (map<@string, int>)GoReflect.MakeContainer(typeof(map<@string, int>));
        m["a"u8] = 1;
        Assert.AreEqual((nint)1, len(m));

        channel<int> c = (channel<int>)GoReflect.MakeContainer(typeof(channel<int>), 1);
        c.Send(4);
        Assert.AreEqual(4, c.Receive());

        // A generated NAMED slice type is made as itself, Go's named result.
        object named = GoReflect.MakeContainer(typeof(sort.IntSlice), 2, 2);
        Assert.IsInstanceOfType(named, typeof(sort.IntSlice));
        Assert.AreEqual((nint)2, ((sort.IntSlice)named).Length);

        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.MakeContainer(typeof(Plain)), "a type with no make is refused by name");
    }

    [TestMethod]
    public void AStructConversionCopiesFieldWiseAndSurvivesAGc()
    {
        WithRefsB converted = convertThenCollect();

        // Read back only after a forced, compacting collection: a raw byte copy that bypassed the write barriers could
        // leave these references stale; the field-wise copy stores each through the runtime.
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

        Assert.AreEqual("hello", converted.S.ToString());
        Assert.AreEqual((nint)3, converted.Sl.Length);
        Assert.AreEqual(30, converted.Sl[2]);
        Assert.AreEqual(42, converted.P!.Value);
    }

    // The source and its references are unreachable once this returns, so only the copy keeps them alive.
    private static WithRefsB convertThenCollect()
    {
        heap(42, out ж<int> pointer);

        WithRefsA source = new() { S = new @string("hello"u8.ToArray()), Sl = new int[] { 10, 20, 30 }.slice(), P = pointer };

        Assert.IsTrue(GoReflect.TryReinterpretValue(source, typeof(WithRefsB), out object? result));
        WithRefsB converted = (WithRefsB)result!;

        // A copy, as Go's conversion is: reassigning a field of one is not seen through the other.
        source.S = new @string("changed"u8.ToArray());
        Assert.AreEqual("hello", converted.S.ToString());

        return converted;
    }

    [TestMethod]
    public void ANestedFieldOfACompatibleTypeIsCopiedThrough()
    {
        OuterA source = new() { In = new InnerA { X = 5, S = new @string("in"u8.ToArray()) }, N = 9 };

        Assert.IsTrue(GoReflect.TryReinterpretValue(source, typeof(OuterB), out object? result));

        OuterB converted = (OuterB)result!;
        Assert.AreEqual((nint)5, converted.In.X);
        Assert.AreEqual("in", converted.In.S.ToString());
        Assert.AreEqual((nint)9, converted.N);
    }

    [TestMethod]
    public void ASingleFieldWrapperWrapsAndUnwraps()
    {
        Pair pair = new() { X = 1, Y = 2 };

        Assert.IsTrue(GoReflect.TryReinterpretValue(pair, typeof(WrappedPair), out object? wrapped));
        Assert.AreEqual((nint)2, ((WrappedPair)wrapped!).Value.Y);

        Assert.IsTrue(GoReflect.TryReinterpretValue(wrapped, typeof(Pair), out object? unwrapped));
        Assert.AreEqual((nint)1, ((Pair)unwrapped!).X);
    }

    [TestMethod]
    public void AShapeThatIsNotOneRepresentationIsRefused()
    {
        Assert.IsFalse(GoReflect.TryReinterpretValue(new RefFieldA { O = "x" }, typeof(RefFieldB), out _), "reference fields of different types are never one representation");
        Assert.IsFalse(GoReflect.TryReinterpretValue(new Plain(), typeof(ж<int>), out _), "a reference destination is refused");

        // The old gate also punned two reference-free structs of different shapes by size; reflect only converts Go structs
        // of identical underlying type, whose lifted fields correspond one to one, so the field-wise copy refuses the pun.
        Assert.IsFalse(GoReflect.TryReinterpretValue(new TwoInts { A = 1, B = 2 }, typeof(OneLong), out _), "no byte pun between different shapes");
    }

    [TestMethod]
    public void TheUntypedFromBoxAgreesWithTheTypedOne()
    {
        // Managed.
        heap(3, out ж<int> managed);
        assertSame(@unsafe.Pointer.FromBox(managed), @unsafe.Pointer.FromBoxObject(managed), "managed");

        // Nil, as a nil box and as a null.
        assertSame(@unsafe.Pointer.FromBox(global::go.ж<int>.NilBox), @unsafe.Pointer.FromBoxObject(global::go.ж<int>.NilBox), "nil box");
        Assert.AreEqual((nuint)0, (nuint)(uintptr)@unsafe.Pointer.FromBoxObject(null), "a null is the nil pointer");

        // The zerobase element a zero-capacity slice header names.
        ж<int> zeroBase = GoZeroCapacityElement<int>.Element;
        assertSame(@unsafe.Pointer.FromBox(zeroBase), @unsafe.Pointer.FromBoxObject(zeroBase), "zerobase");

        // A native alias.
        nint block = Marshal.AllocHGlobal(16);

        try
        {
            ж<int> native = new NativeBox<int>((nuint)block);
            assertSame(@unsafe.Pointer.FromBox(native), @unsafe.Pointer.FromBoxObject(native), "native");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }

        Assert.ThrowsException<ArgumentException>(() => @unsafe.Pointer.FromBoxObject(new object()), "a value that is not a box is refused by name");

        static void assertSame(@unsafe.Pointer typed, @unsafe.Pointer untyped, string what)
        {
            Assert.AreEqual((nuint)(uintptr)typed, (nuint)(uintptr)untyped, $"{what}: the same number");
            Assert.AreSame(typed.RetainedSource, untyped.RetainedSource, $"{what}: the same retained box");
        }
    }
}
