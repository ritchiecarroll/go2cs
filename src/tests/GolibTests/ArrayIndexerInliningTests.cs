// ArrayIndexerInliningTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// golib <c>array&lt;T&gt;</c>'s <c>int</c> and <c>nint</c> indexers carry AggressiveInlining (P-256 R2, COORD
/// ruling 2026-10-22), so an element read compiles to the bounds check and the load, as Go's array index does.
/// Fiat's limb loops read through them dozens of times per field multiply, from callers too large for the JIT to
/// inline them on its own budget. The bounds arm holds the inlined check to Go's: a negative index or one at the
/// length panics with Go's text, and an alias window's index is relative to its own origin.
/// </summary>
[TestClass]
public class ArrayIndexerInliningTests
{
    [TestMethod]
    public void TheIntAndNintIndexersCarryAggressiveInlining()
    {
        foreach (Type index in new[] { typeof(int), typeof(nint) })
        {
            MethodInfo? getter = typeof(array<ulong>).GetProperty("Item", [index])?.GetMethod;
            Assert.IsNotNull(getter, $"array<T>'s {index.Name} indexer not found");
            Assert.IsTrue(getter.MethodImplementationFlags.HasFlag(MethodImplAttributes.AggressiveInlining),
                $"array<T>'s {index.Name} indexer must carry AggressiveInlining: an over-budget caller (a fiat field multiply) leaves it a call otherwise");
        }
    }

    [TestMethod]
    public void TheInlinedIndexersKeepGosBoundsAndWindow()
    {
        array<ulong> backing = new(8);
        array<ulong> window = array<ulong>.Alias(new slice<ulong>(backing.Source, 2, 6), 4);

        window[1] = 7UL;
        window[(nint)3] = 9UL;
        Assert.AreEqual(7UL, backing[3], "an int index on a window is relative to the window's origin");
        Assert.AreEqual(9UL, backing[(nint)5], "an nint index on a window is relative to the window's origin");

        Assert.AreEqual("runtime error: index out of range [4] with length 4",
            Assert.ThrowsException<PanicException>(() => window[4]).Message, "an int index at the length");
        Assert.AreEqual("runtime error: index out of range [-1]",
            Assert.ThrowsException<PanicException>(() => window[(nint)(-1)]).Message, "a negative nint index");
    }
}
