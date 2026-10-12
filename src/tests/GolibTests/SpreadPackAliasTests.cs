// SpreadPackAliasTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards the ESCAPING variadic pack (COORD ruling 2026-10-12, route (a)): in Go, <c>f(xs...)</c> passes the slice
/// ITSELF, so a callee that keeps its pack (<c>return xs</c>, a store, a capture) holds the caller's backing array and
/// capacity. The converter emits such a callee's prologue as <c>var xs = xsʗp.slice();</c>, which copied. golib now
/// records, per thread, the slice behind the last spread (<c>xs.ꓸꓸꓸ</c>), and the escaping prologue returns that
/// slice when its span IS the recorded window: the same first element and the same length. Anything else is copied
/// as before. Either prologue consumes the record, so a record lives from one spread to one prologue.
/// </summary>
/// <remarks>
/// Each test is written against what Go prints; the copy is the control that proves a test can see the difference.
/// The callees below are the converted shapes by hand: an escaping prologue (<c>span.slice()</c>) and a viewing one
/// (<c>span.sslice()</c>).
/// </remarks>
[TestClass]
public class SpreadPackAliasTests
{
    // func keep(xs ...int) []int { return xs }
    private static slice<nint> keep(params Span<nint> xsʗp) => xsʗp.slice();

    // func outer(xs ...int) []int { return keep(xs...) }
    private static slice<nint> outer(params Span<nint> xsʗp)
    {
        var xs = xsʗp.slice();
        return keep(xs.ꓸꓸꓸ);
    }

    // func viaView(xs ...int) []int { return keep(xs[0:2:2]...) }, where viaView's pack does not escape (a view).
    private static slice<nint> viaView(params Span<nint> xsʗp)
    {
        var xs = xsʗp.sslice();
        Span<nint> window = xs;
        return keep(window[..2]);
    }

    // func sum(xs ...int) int, a viewing callee.
    private static nint sum(params Span<nint> xsʗp)
    {
        var xs = xsʗp.sslice();
        nint total = 0;

        foreach (nint x in (Span<nint>)xs)
            total += x;

        return total;
    }

    private static slice<nint> Of(params nint[] values) => values.slice();

    [TestMethod]
    public void KeepAliasesTheSpreadSliceAndKeepsItsCapacity()
    {
        // Go: ys := keep(xs...); ys[1] = 8 -> xs is [7 8]
        var xs = Of(7, 2);
        var ys = keep(xs.ꓸꓸꓸ);
        ys[1] = 8;

        Assert.AreEqual((nint)8, xs[1], "the kept pack is the caller's slice in Go");
        Assert.AreEqual(cap(xs), cap(ys));
    }

    [TestMethod]
    public void NestedSpreadAliasesTheInnermostSlice()
    {
        // Go: keep(keep(zs...)...) is zs
        var zs = Of(1, 2, 3);
        var r = keep(keep(zs.ꓸꓸꓸ).ꓸꓸꓸ);
        r[0] = 100;

        Assert.AreEqual((nint)100, zs[0]);
    }

    [TestMethod]
    public void ReSpreadFromAnEscapingCalleeAliasesTheCallersSlice()
    {
        var xs = Of(1, 2);
        var r = outer(xs.ꓸꓸꓸ);
        r[0] = 5;

        Assert.AreEqual((nint)5, xs[0]);
    }

    [TestMethod]
    public void ALiteralPackAfterAViewedSpreadIsAFreshArray()
    {
        // A viewing callee consumes the record; a later literal pack is other memory and is copied, as in Go.
        var xs = Of(4, 4);
        Assert.AreEqual((nint)8, sum(xs.ꓸꓸꓸ));

        var r = keep(4, 4);
        r[0] = 9;

        Assert.AreEqual((nint)4, xs[0]);
    }

    [TestMethod]
    public void StaleWindowThroughAViewNeverWidensCapacity()
    {
        // The case found in the sizing (2026-10-12): viaView(xs...) re-spreads xs[0:2:2] through a VIEW. That span has
        // the recorded window's first element and length but a smaller capacity. Go: keep's pack has cap 2, so its
        // append reallocates and xs's third slot keeps 3. Without the view prologue clearing the record, the
        // escaping prologue took the stale record and the append wrote 99 into the caller's array.
        var backing = new nint[] { 1, 2, 3 };
        var xs = backing.slice(0, 2, 3);
        var r = viaView(xs.ꓸꓸꓸ);
        _ = append(r, (nint)99);

        Assert.AreEqual((nint)2, cap(r), "keep's pack is xs[0:2:2]");
        Assert.AreEqual((nint)3, backing[2], "the append in keep must not write the caller's array");
    }

    [TestMethod]
    public void SameArraySameLengthOtherOffsetIsRejected()
    {
        var backing = new nint[] { 1, 2, 3, 4 };
        var left = backing.slice(0, 2, 4);
        var right = backing.slice(2, 4, 4);

        Span<nint> recorded = left.ꓸꓸꓸ;   // a record for `left` ...
        var r = keep(right.ToSpan());       // ... and a span of `right` reaching the escaping prologue
        r[0] = 50;

        Assert.AreEqual(2, recorded.Length);
        Assert.AreEqual((nint)3, backing[2], "a window at another offset is not the recorded slice");
    }

    [TestMethod]
    public void AnInterleavedSpreadMakesThePendingOneFallBackToTheCopy()
    {
        // Reentrancy (a type initializer or a deferred call between the getter and the prologue): the interleaved
        // spread replaces the record, so the pending span no longer matches and is copied. Safe, never wrong.
        var a = Of(1, 1);
        var b = Of(2, 2);

        Span<nint> pending = a.ꓸꓸꓸ;
        _ = keep(b.ꓸꓸꓸ);
        var r = keep(pending);
        r[0] = 70;

        Assert.AreEqual((nint)1, a[0]);
    }

    [TestMethod]
    public void AnotherThreadsSpreadDoesNotDisturbThisThreadsRecord()
    {
        var a = Of(3, 3);
        Span<nint> pending = a.ꓸꓸꓸ;

        var other = new Thread(() => { _ = keep(Of(3, 3).ꓸꓸꓸ); });
        other.Start();
        other.Join();

        var r = keep(pending);
        r[0] = 80;

        Assert.AreEqual((nint)80, a[0], "the record is per thread");
    }

    [TestMethod]
    public void GoroutinesSpreadingConcurrentlyEachKeepTheirOwnSlice()
    {
        // Many goroutines spread and keep at once; every kept pack must be its own caller's slice.
        const int goroutines = 8;
        const int rounds = 2_000;
        int failures = 0;
        using var done = new CountdownEvent(goroutines);

        for (int g = 0; g < goroutines; g++)
        {
            nint id = g;

            goǃ(() =>
            {
                try
                {
                    for (int i = 0; i < rounds; i++)
                    {
                        var mine = Of(id, i);
                        var kept = keep(mine.ꓸꓸꓸ);
                        kept[1] = -1;

                        if (mine[1] != -1 || kept[0] != id)
                            Interlocked.Increment(ref failures);
                    }
                }
                finally
                {
                    done.Signal();
                }
            });
        }

        Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(60)), "the goroutines did not finish");
        Assert.AreEqual(0, failures);
    }

    [TestMethod]
    public void AZeroLengthWindowWithCapacityAliasesOnAppend()
    {
        // Go: keep(xs[:0]...) with cap 2, then append, writes into xs's array.
        var backing = new nint[] { 1, 2 };
        var xs = backing.slice(0, 0, 2);
        _ = append(keep(xs.ꓸꓸꓸ), (nint)9);

        Assert.AreEqual((nint)9, backing[0]);
    }

    [TestMethod]
    public void ANilSpreadStaysNil()
    {
        // Go: keep(nil...) is nil. The nil rule is golib's existing one and must survive the record.
        slice<nint> nilSlice = default;
        var r = keep(nilSlice.ꓸꓸꓸ);

        Assert.IsTrue(r == nil, "a nil spread is a nil pack");
    }

    [TestMethod]
    public void AnEscapingCalleeWithNoSpreadStillCopiesItsLiteralPack()
    {
        // Control: no spread, no record, the copy as before.
        var r = keep(1, 2);
        Assert.AreEqual((nint)2, len(r));
    }
}
