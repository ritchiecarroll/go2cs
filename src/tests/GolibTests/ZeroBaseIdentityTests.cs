// ZeroBaseIdentityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
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

    // Generic `==` over a pointer type argument (slices.Contains, a comparable type parameter) is AreEqual<T>. It took
    // the interface-value path, whose type match reads the box CLASS, so an element box and a heap box naming the
    // zerobase were unequal there while equal under ж's `==`. Go has one `==`.
    [TestMethod]
    public void GenericEqualityAgreesWithPointerEqualityAcrossBoxKinds()
    {
        slice<Void> s = new(10);
        ж<Void> element = Ꮡ(s, 5), heap = Zero<Void>();

        Assert.IsTrue(element == heap, "&s[5] == new(struct{})");
        Assert.IsTrue(AreEqual(element, heap), "and through generic ==");
        Assert.IsTrue(AreEqual(heap, element), "in either order");

        ж<nint> one = Ꮡ((nint)1), other = Ꮡ((nint)1);

        Assert.IsTrue(AreEqual(one, one), "a pointer equals itself through generic ==");
        Assert.IsFalse(AreEqual(one, other), "two allocations stay two pointers through generic ==");
    }

    [TestMethod]
    public void GenericEqualityAgreesWithPointerEqualityForANativeElementAndANativeBox()
    {
        const int Length = 8;
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(Length * sizeof(ulong));

        try
        {
            ж<array<ulong>> view = NativeArrayBox<ulong>.Over(addr, Length);
            ж<ulong> element = view.at<ulong>(4);                                // a native element box
            ж<ulong> rebuilt = (ж<ulong>)(uintptr)(addr + 4 * sizeof(ulong));    // a NativeBox, one address

            Assert.AreNotEqual(element.GetType(), rebuilt.GetType(), "two box kinds, or the pair proves nothing");
            Assert.IsTrue(element == rebuilt, "one native address is one pointer");
            Assert.IsTrue(rebuilt == element, "in either order");
            Assert.IsTrue(AreEqual(element, rebuilt), "and through generic ==");
            Assert.IsTrue(AreEqual(rebuilt, element), "in either order");

            ж<ulong> next = view.at<ulong>(5);

            Assert.IsFalse(rebuilt == next, "the next element is another pointer");
            Assert.IsFalse(next == rebuilt);
            Assert.IsFalse(AreEqual(rebuilt, next), "and through generic ==");
            Assert.IsFalse(AreEqual(next, rebuilt));
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void GenericEqualityAgreesWithPointerEqualityForANamedPointerType()
    {
        slice<Void> s = new(10);
        voidPtr element = new(Ꮡ(s, 5)), heap = new(Zero<Void>()), nilPtr = new(nil);

        Assert.IsTrue(element == heap, "type P *struct{}: P(&s[5]) == P(new(struct{}))");
        Assert.IsTrue(heap == element, "in either order");
        Assert.IsTrue(AreEqual(element, heap), "and through generic ==");
        Assert.IsTrue(AreEqual(heap, element), "in either order");

        Assert.IsFalse(element == nilPtr, "a non-nil P is not the nil P");
        Assert.IsFalse(nilPtr == element);
        Assert.IsFalse(AreEqual(element, nilPtr), "and through generic ==");
        Assert.IsFalse(AreEqual(nilPtr, element));

        Assert.IsTrue(AreEqual(nilPtr, (voidPtr)null!), "the nil P and a null reference are one nil");
        Assert.IsTrue(AreEqual((voidPtr)null!, nilPtr), "in either order");
    }

    // A stand-in for go2cs-gen's named-pointer wrapper (`type P *struct{}`), with the members its templates emit that
    // the comparison reads: Equals over the box and the reference-identity == (Templates/InheritedType/
    // PointerTypeTemplate and InheritedTypeTemplate's "Pointer" EqualityExpression).
    [GoType("ж<Void>")]
    private sealed class voidPtr : IPointer<Void>, INilPointer
    {
        private readonly ж<Void> m_value;

        public voidPtr(ж<Void> value) => m_value = value;

        public voidPtr(NilType _) => m_value = default!;

        public ref Void Value => ref m_value.Value;

        public bool IsNull => m_value is null || m_value.IsNull;

        public bool IsNilPointer => m_value is null || m_value.IsNilPointer;

        public override bool Equals(object? obj) => obj switch
        {
            voidPtr other => m_value == other.m_value,
            null => IsNilPointer,
            _ => false
        };

        public override int GetHashCode() => m_value?.GetHashCode() ?? 0;

        public static bool operator ==(voidPtr left, voidPtr right) => Equals(left, right);

        public static bool operator !=(voidPtr left, voidPtr right) => !(left == right);

        static Void IPointer<Void>.operator ~(IPointer<Void> value) => value.Value;
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
    public void FuncForPCAnswersOneHandlePerFunction()
    {
        var (pc, _) = Δruntime.GoCallerSitesProbe();
        uintptr here = reflect_package.ValueOf((Action)wrapperprobe_package.plain).Pointer();

        Assert.AreEqual("wrapperprobe.plain", (string)Δruntime.FuncForPC(here).Name(), "the second function's pc must resolve");
        Assert.IsTrue(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(pc), "FuncForPC(pc) == FuncForPC(pc), as the pc++ loop in symtab_test.go relies on");
        Assert.IsTrue(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(pc - 1), "one entry, one *Func");
        Assert.IsFalse(Δruntime.FuncForPC(pc) == Δruntime.FuncForPC(here), "two functions are two *Func, though Func is zero-size");
        Assert.AreEqual("runtime.GoCallerSitesProbe", (string)Δruntime.FuncForPC(pc).Name(), "and each keeps its own name");
    }

    // Go's *Func points into the one pclntab entry of its function, so every pc of the function, at any call site,
    // answers the same pointer, and a frame of it carries that pointer too. runtime's TestFunctionAlignmentTraceback
    // compares the two (symtab_test.go: frame.Func != f), and reaches that comparison on an arm64 process.
    [TestMethod]
    public void FuncForPCIsOneFuncPerFunctionAndTheFuncFramesNextSets()
    {
        var (first, second) = Δruntime.GoCallerSitesProbe();

        Assert.IsTrue(Δruntime.FuncForPC(first) == Δruntime.FuncForPC(second), "two call sites of one function are one *Func");
        Assert.AreEqual(Δruntime.FuncForPC(first).Entry(), Δruntime.FuncForPC(second).Entry(), "one function, one entry");

        slice<uintptr> pcs = new slice<uintptr>(1);
        pcs[0] = first;
        var (frame, _) = Δruntime.CallersFrames(pcs).Next();

        Assert.IsTrue(frame.Func == Δruntime.FuncForPC(first), "FuncForPC(pc) is the *Func Frames.Next sets for pc");
    }
}
