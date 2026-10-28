// SliceLiteralBindingTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards how golib's <c>builtin.slice&lt;T&gt;</c> overloads bind (1.26.10.2 parity seat #30, L1).
/// </summary>
/// <remarks>
/// <para>
/// The converter writes Go's <c>[]int{1, 2, 3}</c> as <c>slice&lt;nint&gt;([1, 2, 3])</c>. A collection expression
/// converts to <c>T[]</c>, to <c>ReadOnlySpan&lt;T&gt;</c> and to <see cref="SliceLiteral{T}"/> alike, and the
/// <see cref="SliceLiteral{T}"/> overload's priority makes it bind there: a fresh backing array per evaluation, never
/// nil, not charged to <see cref="AllocationCounter"/> (as the <c>new T[]{…}</c> array of the earlier form was not).
/// The counted span overload would charge one allocation per literal, which moved math/big's TestNewIntAllocs away
/// from Go's count (measured 2 per run against 1).
/// </para>
/// <para>
/// No other argument converts to <see cref="SliceLiteral{T}"/>, so adding the overload must leave every existing
/// call where it was: a <c>u8</c> literal (<c>[]byte("…")</c>) on the counted span overload, an array on the
/// wrapping array overload, a golib <see cref="array{T}"/> where it bound before. The counter tells the overloads
/// apart: the span overload charges one allocation, the array overload and the literal overload none.
/// </para>
/// </remarks>
[TestClass]
public class SliceLiteralBindingTests
{
    [TestInitialize]
    public void EnableCounter() => AllocationCounter.Enable();

    [TestMethod]
    public void ACollectionExpressionBindsTheUncountedLiteralOverload()
    {
        long before = AllocationCounter.CurrentThreadCount;
        slice<nint> s = builtin.slice<nint>([1, 2, 3]);
        long charged = AllocationCounter.CurrentThreadCount - before;

        Assert.AreEqual(0L, charged, "a slice literal is not charged to the allocation counter");
        Assert.AreEqual((nint)3, s.Length);
        Assert.AreEqual((nint)3, s.Capacity, "len == cap");
        Assert.AreEqual((nint)2, s[1]);
    }

    [TestMethod]
    public void AnEmptyLiteralIsEmptyNotNilAndAllocatesNothing()
    {
        long before = AllocationCounter.CurrentThreadCount;
        slice<nint> s = builtin.slice<nint>([]);

        Assert.AreEqual(0L, AllocationCounter.CurrentThreadCount - before);
        Assert.IsFalse(s.IsNil, "Go's []T{} is not nil");
        Assert.AreEqual((nint)0, s.Length);
        Assert.AreEqual((nint)0, s.Capacity);
    }

    [TestMethod]
    public void EachEvaluationGetsItsOwnBackingArray()
    {
        slice<nint> first = Make(), second = Make();
        first[0] = 99;

        Assert.AreEqual((nint)1, second[0], "two evaluations of one literal share no elements");

        static slice<nint> Make() => builtin.slice<nint>([1, 2]);
    }

    [TestMethod]
    public void AU8LiteralStillBindsTheCountedSpanOverload()
    {
        long before = AllocationCounter.CurrentThreadCount;
        slice<byte> s = builtin.slice<byte>("abc"u8);

        Assert.AreEqual(1L, AllocationCounter.CurrentThreadCount - before, "[]byte(\"…\") copies through the counted span overload");
        Assert.AreEqual((nint)3, s.Length);
        Assert.AreEqual((byte)'b', s[1]);
    }

    [TestMethod]
    public void AnArrayStillBindsTheWrappingArrayOverload()
    {
        nint[] backing = [1, 2, 3];
        long before = AllocationCounter.CurrentThreadCount;
        slice<nint> s = builtin.slice<nint>(backing);

        Assert.AreEqual(0L, AllocationCounter.CurrentThreadCount - before);
        backing[0] = 42;
        Assert.AreEqual((nint)42, s[0], "the array overload wraps its argument, so the slice aliases it");
    }

    [TestMethod]
    public void AGolibArrayStillBindsWhereItBoundBefore()
    {
        array<nint> arr = new nint[] { 1, 2, 3 }.array();
        long before = AllocationCounter.CurrentThreadCount;
        slice<nint> s = builtin.slice<nint>(arr);
        long charged = AllocationCounter.CurrentThreadCount - before;

        arr[0] = 42;

        // Pinned at the base golib (before SliceLiteral<T> existed): see the commit that adds this test.
        Assert.AreEqual(ArrayCharged, charged, "the overload an array<T> argument binds is unchanged");
        Assert.AreEqual(ArrayAliases, s[0] == 42, "and so is whether the slice aliases the array");
    }

    // The base golib's answer for an array<T> argument, measured before the SliceLiteral<T> overload was added.
    private const long ArrayCharged = 0;
    private const bool ArrayAliases = true;
}
