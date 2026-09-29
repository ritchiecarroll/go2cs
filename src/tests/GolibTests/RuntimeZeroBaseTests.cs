// RuntimeZeroBaseTests.cs - Gbtc
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
/// runtime's answers for the zerobase, which Go treats as a pointer outside every heap span
/// (isGoPointerWithoutSpan, spanOfHeap == nil): SetFinalizer and AddCleanup return without
/// registering anything, Pinner.Pin ignores it, and isPinned answers true. One zerobase shared by
/// every zero-size allocation makes these load-bearing: a registration keyed on it would collide
/// across unrelated allocations.
/// </summary>
[TestClass]
public class RuntimeZeroBaseTests
{
    private struct Z
    {
    }

    private static ж<Z> NewZ() => Ꮡ(new Z());

    [TestMethod]
    public void SetFinalizerOnAZeroSizeAllocationRegistersNothing()
    {
        // Go's SetFinalizer returns at isGoPointerWithoutSpan BEFORE its "finalizer already set"
        // check, so a second call on the same zero-size object is silent (mfinal_test.go's
        // TestFinalizerZeroSizedStruct is the single-call form).
        ж<Z> z = NewZ();

        Δruntime.SetFinalizer(z, (Action<ж<Z>>)(_ => { }));
        Δruntime.SetFinalizer(z, (Action<ж<Z>>)(_ => { }));
        Δruntime.SetFinalizer(NewZ(), (Action<ж<Z>>)(_ => { }));
        Δruntime.SetFinalizer(z, null!);
    }

    [TestMethod]
    public void AddCleanupOnAZeroSizeAllocationRegistersNothing()
    {
        ж<Z> z = NewZ();

        Δruntime.Cleanup first = Δruntime.AddCleanup(z, (Action<string>)(_ => { }), "foo");
        Δruntime.Cleanup second = Δruntime.AddCleanup(NewZ(), (Action<string>)(_ => { }), "bar");

        first.Stop();
        second.Stop();
    }

    [TestMethod]
    public void IsPinnedAnswersTrueForAZeroSizeAllocation()
    {
        // Go: isPinned finds no heap span for zerobase and answers true, pinned or not.
        Assert.IsTrue(Δruntime.GoIsPinned(@unsafe.Pointer.FromPinnedBox(NewZ())), "a zero-size allocation is never a heap object");
    }

    [TestMethod]
    public void PinningAZeroSizeAllocationIsANoOp()
    {
        Δruntime.Pinner pinner = new(nil);
        ж<Z> z = NewZ();

        pinner.Pin(z);
        pinner.Pin(NewZ());
        Assert.IsTrue(Δruntime.GoIsPinned(@unsafe.Pointer.FromPinnedBox(z)));
        Assert.IsNull(Δruntime.GoPinCounter(@unsafe.Pointer.FromPinnedBox(z)), "Go's setPinned ignores it, so no counter accrues");

        pinner.Unpin();
        Assert.IsTrue(Δruntime.GoIsPinned(@unsafe.Pointer.FromPinnedBox(z)), "and unpinning changes nothing");
    }
}
