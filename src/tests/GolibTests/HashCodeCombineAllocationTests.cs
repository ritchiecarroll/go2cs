// HashCodeCombineAllocationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Guards the typed <c>go.golib.HashCode.Combine</c> overloads that a generated struct's <c>GetHashCode</c>
/// binds. Before them the only overload was <c>Combine(params object[])</c>, so every hash of a struct
/// allocated an array and boxed each value field: the converted TypeScript checker hashes struct map
/// keys on its hot path (docs/phase4/SCOUT-ts-perf.md, item S4).
/// </summary>
/// <remarks>
/// The typed overloads must also give the SAME hash as the params form, since both hash
/// <c>value?.GetHashCode()</c>; the equality arms pin that, so the change moves no map's bucket layout.
/// No JIT gate is needed: a generic instantiation over value types neither boxes nor builds a params
/// array at any optimization level.
/// </remarks>
[TestClass]
public class HashCodeCombineAllocationTests
{
    private const int LoopCount = 1000;

    private struct Probe
    {
        public long A;
        public double B;

        // A generated Go struct always overrides GetHashCode; without the override the constrained call
        // falls to ValueType.GetHashCode, which boxes, and the probe would measure that box instead.
        public override int GetHashCode() => System.HashCode.Combine(A, B);
    }

    [TestMethod]
    public void CombiningThreeValueFieldsAllocatesNothing()
    {
        long a = 7;
        double b = 2.5;
        Probe c = new() { A = 3, B = 4.5 };
        int sink = 0;

        for (int warm = 0; warm < 1000; warm++)
            sink ^= go.golib.HashCode.Combine(a, b, c);

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int run = 0; run < LoopCount; run++)
            sink ^= go.golib.HashCode.Combine(a + run, b, c);

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        GC.KeepAlive(sink);
        Assert.AreEqual(0L, allocated, $"HashCode.Combine over three value fields allocated {allocated} B in {LoopCount} calls");
    }

    [TestMethod]
    public void TheTypedOverloadsHashAsTheParamsFormDoes()
    {
        long a = 7;
        double b = 2.5;
        Probe c = new() { A = 3, B = 4.5 };
        string? d = null;

        Assert.AreEqual(go.golib.HashCode.Combine(new object[] { a }), go.golib.HashCode.Combine(a));
        Assert.AreEqual(go.golib.HashCode.Combine(new object[] { a, b, c }), go.golib.HashCode.Combine(a, b, c));
        Assert.AreEqual(go.golib.HashCode.Combine(new object?[] { a, b, c, d }!), go.golib.HashCode.Combine(a, b, c, d));
        Assert.AreEqual(go.golib.HashCode.Combine(new object[] { a, b, c, a, b, c, a, b }), go.golib.HashCode.Combine(a, b, c, a, b, c, a, b));
    }
}
