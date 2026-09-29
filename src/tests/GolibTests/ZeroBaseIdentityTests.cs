// ZeroBaseIdentityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using Δruntime = go.runtime_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Go's mallocgc answers EVERY zero-byte allocation with one address, <c>&amp;runtime.zerobase</c>:
/// <c>new(struct{})</c>, a named empty struct, an all-zero-size struct, every element of a
/// <c>[]struct{}</c> and the data word of a <c>make([]T, 0)</c> are one pointer (measured under
/// go1.24.13; escaping values). golib gave each heap box its own identity, so all of them compared
/// unequal, and a <c>map[*struct{}]V</c> kept a key per allocation where Go keeps one.
/// </summary>
/// <remarks>
/// What must NOT join the zerobase is pinned beside it: a zero-size FIELD (Go points it at the end of
/// its struct), a native alias, an ordinary non-zero-size allocation, a non-empty backing's first
/// element, and runtime's opaque <c>*Func</c> handles, which are zero-size in Go but name functions,
/// not allocations.
/// </remarks>
[TestClass]
public class ZeroBaseIdentityTests
{
    private struct Void
    {
    }

    private struct VoidPair
    {
#pragma warning disable CS0169 // the fields exist to BE the shape under test
        private readonly Void m_first;
        private readonly EmptyStruct m_second;
#pragma warning restore CS0169
    }

    public struct Pair
    {
        public nint A;
        public EmptyStruct E;
    }

    private static readonly FieldRefFunc<EmptyStruct> s_e = FieldRef<Pair>.Create<EmptyStruct>("E");

    private static ж<T> Zero<T>() where T : struct => Ꮡ(new T());

    // ---- the zerobase: one pointer for every zero-byte allocation ----

    [TestMethod]
    public void TwoZeroSizeAllocationsAreOnePointer()
    {
        ж<Void> p1 = Zero<Void>(), p2 = Zero<Void>();

        Assert.IsTrue(p1 == p2, "new(struct{}) == new(struct{})");
        Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode(), "equal pointers hash alike");
        Assert.IsTrue(@unsafe.Pointer.FromPinnedBox(p1) == @unsafe.Pointer.FromPinnedBox(p2), "as unsafe.Pointer (address-take)");
        Assert.IsTrue(@unsafe.Pointer.FromBox(p1) == @unsafe.Pointer.FromBox(p2), "as unsafe.Pointer (pointer value)");
        Assert.AreEqual((nuint)(uintptr)p1, (nuint)(uintptr)p2, "and as numbers");
    }

    [TestMethod]
    public void ZeroSizeAllocationsOfDifferentTypesAreOneUnsafePointer()
    {
        @unsafe.Pointer v = @unsafe.Pointer.FromPinnedBox(Zero<Void>());

        Assert.IsTrue(v == @unsafe.Pointer.FromPinnedBox(Zero<EmptyStruct>()), "a named empty struct and struct{}");
        Assert.IsTrue(v == @unsafe.Pointer.FromPinnedBox(Zero<VoidPair>()), "an all-zero-size struct");
        Assert.IsTrue(@unsafe.Pointer.FromBox(Zero<Void>()) == @unsafe.Pointer.FromBox(Zero<VoidPair>()));
    }

    [TestMethod]
    public void EveryElementOfAZeroSizeSliceIsTheZeroSizeAllocation()
    {
        slice<Void> s = new(10);

        Assert.IsTrue(Ꮡ(s, 5) == Zero<Void>(), "&s[5] == new(struct{}) for a []struct{}");
        Assert.IsTrue(@unsafe.Pointer.FromPinnedBox(Ꮡ(s, 5)) == @unsafe.Pointer.FromPinnedBox(Zero<Void>()));
        Assert.IsTrue(@unsafe.SliceData(new slice<Void>(3)) == @unsafe.SliceData(s), "two []struct{} share one data pointer");
        Assert.IsTrue(@unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(new slice<EmptyStruct>(3))) == @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0)),
            "and so do slices of two different zero-size types");
    }

    [TestMethod]
    public void MadeZeroLengthSlicesShareTheZeroBase()
    {
        @unsafe.Pointer ints = @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(new slice<nint>(0)));
        @unsafe.Pointer bytes = @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(new slice<byte>(0)));

        Assert.IsTrue(ints == bytes, "make([]int, 0) and make([]byte, 0) have one data pointer");
        Assert.IsTrue(ints == @unsafe.Pointer.FromPinnedBox(Zero<Void>()), "and it is the zero-size allocation's");
    }

    [TestMethod]
    public void ZeroSizeAllocationsCollapseAsMapKeys()
    {
        ж<Void> p1 = Zero<Void>(), p2 = Zero<Void>();
        map<ж<Void>, nint> m = new() { [p1] = 1 };

        m[p2]++;

        Assert.AreEqual((nint)1, len(m), "map[*struct{}]int keeps ONE key for two news");
        Assert.AreEqual((nint)2, m[p1]);
    }

    [TestMethod]
    public void RuntimeZeroBaseIsTheZeroSizeAllocation()
    {
        @unsafe.Pointer zerobase = Δruntime.GoZeroBaseProbe();

        Assert.IsTrue(zerobase == @unsafe.Pointer.FromPinnedBox(Zero<Void>()), "unsafe.Pointer(new(struct{})) == &zerobase");
        Assert.IsTrue(zerobase == @unsafe.Pointer.FromBox(Zero<EmptyStruct>()));
        Assert.IsTrue(zerobase == @unsafe.Pointer.FromPinnedBox(Ꮡ(new slice<Void>(4), 2)), "&s[2] of a []struct{} == &zerobase");
        Assert.AreEqual((nuint)(uintptr)zerobase, (nuint)(uintptr)@unsafe.Pointer.FromPinnedBox(Zero<Void>()), "the same number");
    }

    [TestMethod]
    public void AZeroSizeAllocationRoundTripsThroughItsNumber()
    {
        ж<Void> p = Zero<Void>();
        ж<Void> back = (ж<Void>)(uintptr)p;

        Assert.IsTrue(back == Zero<Void>(), "(*struct{})(unsafe.Pointer(uintptr(unsafe.Pointer(p)))) is still the zerobase");
        Assert.AreEqual(default(Void), back.Value);
    }

    [TestMethod]
    public void UnsafeSliceOverAZeroSizeElementKeepsGosLength()
    {
        slice<Void> s = new(10);
        slice<Void> rebuilt = @unsafe.Slice(Ꮡ(s, 0), 10);

        Assert.AreEqual((nint)10, len(rebuilt), "unsafe.Slice(&s[0], 10) over a []struct{}");
        Assert.AreEqual(default(Void), rebuilt[9]);
    }

    // ---- what stays out of the zerobase ----

    [TestMethod]
    public void NonZeroSizeAllocationsStayDistinct()
    {
        ж<nint> a = Ꮡ((nint)0), b = Ꮡ((nint)0);

        Assert.IsFalse(a == b, "new(int) != new(int)");
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(a) == @unsafe.Pointer.FromPinnedBox(b));
        Assert.IsFalse(@unsafe.Pointer.FromPinnedBox(a) == @unsafe.Pointer.FromPinnedBox(Zero<Void>()), "and neither is the zerobase");
    }

    [TestMethod]
    public void AZeroSizeFieldIsNotTheZeroBase()
    {
        heap(new Pair(), out ж<Pair> x);
        heap(new Pair(), out ж<Pair> y);

        @unsafe.Pointer fx = @unsafe.Pointer.FromBox(x.of(s_e));
        @unsafe.Pointer fy = @unsafe.Pointer.FromBox(y.of(s_e));

        Assert.IsFalse(fx == @unsafe.Pointer.FromBox(Zero<EmptyStruct>()), "&x.e != &zerobase: Go points a zero-size field at the end of its struct");
        Assert.IsFalse(fx == fy, "&x.e != &y.e for distinct x and y");
    }

    [TestMethod]
    public void ANativeZeroSizePointerIsNotTheZeroBase()
    {
        ж<Void> native = new NativeBox<Void>(0x4000u);

        Assert.IsFalse(native == Zero<Void>(), "a native alias names its address, never zerobase");
    }

    [TestMethod]
    public void TheDataPointerOfACapacityBearingEmptySliceIsNotTheZeroBase()
    {
        @unsafe.Pointer data = @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(new slice<nint>(0, 4)));

        Assert.IsFalse(data == @unsafe.Pointer.FromPinnedBox(Zero<Void>()), "&s[:1][0] of make([]int, 0, 4) is its backing, not zerobase");
    }

    [TestMethod]
    public void FuncForPCAnswersOneHandlePerEntry()
    {
        var (pc, _) = Δruntime.GoCallerSitesProbe();
        uintptr here = reflect_package.ValueOf((Action)wrapperprobe_package.plain).Pointer();

        Assert.AreEqual("wrapperprobe.plain", (string)Δruntime.FuncForPC(here).Name(), "the second function's pc must resolve");
        Assert.IsTrue(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(pc), "FuncForPC(pc) == FuncForPC(pc), as the pc++ loop in symtab_test.go relies on");
        Assert.IsTrue(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(pc - 1), "one entry, one *Func");
        Assert.IsFalse(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(here), "two functions are two *Func, though Func is zero-size");
        Assert.AreEqual("runtime.GoCallerSitesProbe", (string)Δruntime.FuncForPC(pc).Name(), "and each keeps its own name");
    }
}
