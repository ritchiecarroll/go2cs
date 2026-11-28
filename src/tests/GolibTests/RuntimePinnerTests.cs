// RuntimePinnerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using Δruntime = go.runtime_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Q45 — <c>runtime.Pinner</c> over the CLR heap (docs/phase4/DESIGN-runtime-pinner.md, gate 4 of §8).
/// </summary>
/// <remarks>
/// <para>
/// Each arm mirrors one of Go's own <c>runtime/pinner_test.go</c> rows through the public test seams
/// <c>runtime/pinner_impl.cs</c> exposes (<c>GoIsPinned</c>, <c>GoPinCounter</c>, <c>GoCgoCheckPointer</c>,
/// <c>GoPinnerLeakPanic</c>), so a regression in the hand-own reads here before the unbanked runtime
/// row is ever run. The NEGATIVE arms are the point: every "passes" assertion is followed by the same
/// check with the pin removed, which must go RED — a gate that cannot fail proves nothing.
/// </para>
/// <para>
/// What the arms encode, from the design: the pin is keyed by the pointer's referent ALLOCATION (an
/// element pin pins the whole backing; two Pinners on one object share one count); the cgo check is
/// a TWO-level walk (level 1 and level 2 pointer words must be pinned, level 3 is not inspected); a
/// native pointer is not a Go pointer (Pin no-ops, isPinned answers true); an address-take is NOT a
/// Pinner pin; the leak finalizer fires through the real SetFinalizer bridge only with pins
/// outstanding; and the box itself gains no instance state — the byte-cost rule as an assertion.
/// </para>
/// </remarks>
[TestClass]
public class RuntimePinnerTests
{
    private struct Leaf
    {
        public long x;
    }

    private struct Mid
    {
        public ж<Leaf> o;
    }

    private struct Top
    {
        public ж<Mid> o;
    }

    private struct Outer
    {
        public ж<Top> o;
    }

    private struct WithUnsafePointer
    {
        public long pad;
        public @unsafe.Pointer o;
    }

    private struct WithNative
    {
        public ж<long> o;
    }

    private static @unsafe.Pointer AddrOf<T>(ж<T> box)
    {
        return @unsafe.Pointer.FromPinnedBox(box);
    }

    private static void AssertCheckPanics(object ptr, string because)
    {
        try
        {
            Δruntime.GoCgoCheckPointer(ptr, true);
        }
        catch (PanicException)
        {
            return;
        }

        Assert.Fail($"cgoCheckPointer did not panic: {because}");
    }

    private static void AssertCheckPasses(object ptr, string because)
    {
        try
        {
            Δruntime.GoCgoCheckPointer(ptr, true);
        }
        catch (PanicException ex)
        {
            Assert.Fail($"cgoCheckPointer panicked: {because}: {ex.Message}");
        }
    }

    [TestMethod]
    public void PinMarksTheObject_RepeatsCount_UnpinClears()
    {
        Δruntime.Pinner pinner = new(nil);
        var p = new StandardBox<Leaf>(new Leaf { x = 7 });
        var addr = AddrOf(p);

        // TestPinnerSimple's own first assertion: taking the address (which pins the storage and
        // registers the provenance record) is NOT a Pinner pin.
        Assert.IsFalse(Δruntime.GoIsPinned(addr), "an address-take must not read as a Pinner pin");

        pinner.Pin(p);
        Assert.IsTrue(Δruntime.GoIsPinned(addr), "Pin must mark the object");
        Assert.IsNull(Δruntime.GoPinCounter(addr), "a single pin has no counter (Go: nil)");

        pinner.Pin(p);
        pinner.Pin(p);
        var counter = Δruntime.GoPinCounter(addr);
        Assert.IsNotNull(counter, "repeat pins create the counter");
        Assert.AreEqual(2UL, (ulong)counter.Value.Value, "the counter records the ADDITIONAL pins (TestPinnerMultiplePinsSame: N-1)");

        pinner.Unpin();
        Assert.IsFalse(Δruntime.GoIsPinned(addr), "Unpin must clear the mark");
        Assert.IsNull(Δruntime.GoPinCounter(addr), "Unpin must delete the counter");

        // TestPinnerEmptyUnpin: Unpin on an already-unpinned Pinner and on one that never pinned.
        pinner.Unpin();
        Δruntime.Pinner never = new(nil);
        never.Unpin();
        never.Unpin();
    }

    [TestMethod]
    public void PinIsPerAllocation_AnElementPinPinsTheBacking_TwoPinnersShareOneCount()
    {
        Δruntime.Pinner p1 = new(nil);
        Δruntime.Pinner p2 = new(nil);
        var sl = new long[] { 1, 2, 3 }.slice();
        var e0 = Ꮡ(sl, 0);
        var e1 = Ꮡ(sl, 1);

        Assert.IsFalse(Δruntime.GoIsPinned(AddrOf(e1)));
        p1.Pin(e0);
        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(e1)), "pinning &sl[0] pins the whole backing array (Go's object index)");

        // The slice header in a box (`&sl`): its array word is pinned, so the check passes...
        var header = new StandardBox<slice<long>>(sl);
        AssertCheckPasses(header, "&sl with its backing pinned (TestPinnerCgoCheckSlice)");

        p2.Pin(e1);
        var counter = Δruntime.GoPinCounter(AddrOf(e0));
        Assert.IsNotNull(counter);
        Assert.AreEqual(1UL, (ulong)counter.Value.Value, "two Pinners on one allocation: one additional pin (TestPinnerTwoPinner)");

        p1.Unpin();
        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(e0)), "still pinned by the other Pinner");
        Assert.IsNull(Δruntime.GoPinCounter(AddrOf(e0)), "the counter is gone once a single pin remains");

        p2.Unpin();
        Assert.IsFalse(Δruntime.GoIsPinned(AddrOf(e0)));

        // ...and the NEGATIVE arm: with the pin gone the very same check must go red.
        AssertCheckPanics(header, "an unpinned backing must fail the cgo check");
    }

    [TestMethod]
    public void CgoCheckIsATwoLevelWalk_LevelThreeIsNotInspected()
    {
        Δruntime.Pinner pinner = new(nil);
        var p = new StandardBox<Leaf>(new Leaf { x = 1 });
        var p2 = new StandardBox<Mid>(new Mid { o = p });
        var p3 = new StandardBox<Top>(new Top { o = p2 });
        var p4 = new StandardBox<Outer>(new Outer { o = p3 });

        AssertCheckPanics(p2, "p2 -> p: p unpinned at level 1 (TestPinnerCgoCheckPtr2Ptr)");
        AssertCheckPanics(p3, "p3 -> p2: p2 unpinned at level 1");

        pinner.Pin(p2);
        AssertCheckPanics(p2, "p2 pinned, but p2 -> p: p still unpinned (TestPinnerCgoCheckPinned2UnpinnedPanics)");
        AssertCheckPanics(p3, "p3 -> p2 (pinned) -> p: p unpinned at level 2");

        pinner.Pin(p);
        AssertCheckPasses(p2, "p2 -> p with p pinned");
        AssertCheckPasses(p3, "p3 -> p2 -> p with both pinned (TestPinnerCgoCheckPtr2Pinned2Unpinned)");

        // Level 3 is NOT inspected: with p3 (level 1) and p2 (level 2) pinned and p NOT pinned, the
        // check of p4 passes — Go's cgoCheckUnknownPointer reads an object's words and never descends.
        pinner.Unpin();
        pinner.Pin(p3);
        pinner.Pin(p2);
        AssertCheckPasses(p4, "p4 -> p3 (L1 pinned) -> p2 (L2 pinned) -> p (L3, not inspected)");
        AssertCheckPanics(p3, "but p3 itself: p3 -> p2 (L1 pinned) -> p (L2 unpinned)");

        // NEGATIVE arm: nothing pinned, the pass above must go red.
        pinner.Unpin();
        AssertCheckPanics(p4, "with nothing pinned the level-1 pointer is unpinned");
    }

    [TestMethod]
    public void CgoCheckSeesUnsafePointerFields_AndTheUnknownPointerShape()
    {
        Δruntime.Pinner pinner = new(nil);
        var leaf = new StandardBox<Leaf>(new Leaf { x = 3 });
        var up = @unsafe.Pointer.FromPinnedBox(leaf);
        var holder = new StandardBox<WithUnsafePointer>(new WithUnsafePointer { pad = 1, o = up });

        AssertCheckPanics(holder, "an unsafe.Pointer field to an unpinned object (TestPinnerCgoCheckPtr2UnsafePtr)");
        pinner.Pin(up);
        AssertCheckPasses(holder, "the unsafe.Pointer's referent pinned through Pin(unsafe.Pointer)");
        pinner.Unpin();
        AssertCheckPanics(holder, "negative arm: unpinned again");

        // arg == nil (TestPinnerCgoCheckPtr2UnknownPtr): p2 = &p for an unsafe.Pointer variable p.
        // The pointee is walked as an unknown object, and its one word must be pinned.
        var cell = new StandardBox<@unsafe.Pointer>(up);

        try
        {
            Δruntime.GoCgoCheckPointer(cell, nil);
            Assert.Fail("the cell's word is an unpinned Go pointer");
        }
        catch (PanicException)
        {
        }

        pinner.Pin(up);
        Δruntime.GoCgoCheckPointer(cell, nil);
        pinner.Unpin();
    }

    [TestMethod]
    public void PinningAnInterfaceCellDoesNotPinItsPointee_AndTheCheckReadsTheDataWord()
    {
        Δruntime.Pinner pinner = new(nil);
        var o = new StandardBox<Leaf>(new Leaf { x = 5 });

        // Go: `var ifc any = o; pinner.Pin(&ifc)` pins the interface CELL, not what it holds.
        var cell = new StandardBox<object>(o);
        pinner.Pin(cell);
        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(cell)), "the cell is pinned");
        Assert.IsFalse(Δruntime.GoIsPinned(AddrOf(o)), "its pointee is not (TestPinnerInterface)");
        pinner.Unpin();

        // TestPinnerCgoCheckInterface: `&ifc` with an unpinned pointer in the interface panics; pin
        // the pointee and it passes — the check reads the DATA WORD.
        AssertCheckPanics(cell, "the interface's data word is an unpinned Go pointer");
        pinner.Pin(o);
        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(o)));
        Assert.IsFalse(Δruntime.GoIsPinned(AddrOf(cell)), "Pin(ifc) pins the pointee, not the cell");
        AssertCheckPasses(cell, "the data word pinned");
        pinner.Unpin();
        AssertCheckPanics(cell, "negative arm: unpinned again");
    }

    [TestMethod]
    public void ANativePointerIsNotAGoPointer_PinNoOps_IsPinnedAnswersTrue()
    {
        Δruntime.Pinner pinner = new(nil);

        // A number nothing resolves converts to a native alias (a NativeBox).
        ж<long> native = (ж<long>)(uintptr)(nuint)0x1000;
        Assert.AreNotEqual((nuint)0, native.NativeAddress, "the arm needs a native alias");

        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(native)), "Go: a pointer outside any heap span reads pinned");
        pinner.Pin(native);
        Assert.IsNull(Δruntime.GoPinCounter(AddrOf(native)), "Pin on a non-Go pointer is a no-op");

        var holder = new StandardBox<WithNative>(new WithNative { o = native });
        AssertCheckPasses(holder, "a native pointer word is not a Go pointer");
        pinner.Unpin();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UseAndDropAPinner(bool unpinFirst)
    {
        Δruntime.Pinner pinner = new(nil);
        var p = new StandardBox<Leaf>(new Leaf { x = 9 });
        pinner.Pin(p);

        if (unpinFirst)
            pinner.Unpin();
    }

    [TestMethod]
    public void ALeakedPinnerReportsThroughPinnerLeakPanic_AnUnpinnedOneDoesNot()
    {
        Action old = Δruntime.GoPinnerLeakPanic;
        using var fired = new ManualResetEventSlim(false);
        Δruntime.GoPinnerLeakPanic = () => fired.Set();

        try
        {
            UseAndDropAPinner(unpinFirst: false);
            Δruntime.GC();
            Δruntime.GC();
            Assert.IsTrue(fired.Wait(TimeSpan.FromSeconds(10)), "TestPinnerLeakPanics: the pinner's finalizer must report pins outstanding");

            fired.Reset();
            UseAndDropAPinner(unpinFirst: true);
            Δruntime.GC();
            Δruntime.GC();
            Assert.IsFalse(fired.Wait(TimeSpan.FromSeconds(1)), "negative arm: an unpinned pinner leaks nothing");
        }
        finally
        {
            Δruntime.GoPinnerLeakPanic = old;
        }
    }

    [TestMethod]
    public void PinValidatesItsArgument_AndToleratesAProjectedNumber()
    {
        Δruntime.Pinner pinner = new(nil);

        try
        {
            pinner.Pin(nil);
            Assert.Fail("Pin(nil) must panic");
        }
        catch (PanicException ex)
        {
            StringAssert.Contains(ex.Message, "Pinner");
        }

        try
        {
            pinner.Pin(42);
            Assert.Fail("Pin(int) must panic (TestPinnerPinNonPtrPanics)");
        }
        catch (PanicException ex)
        {
            StringAssert.Contains(ex.Message, "Pinner");
        }

        // The Q49 accommodation (pinner_impl.cs header): a pointer's projected number — the shape
        // internal/fmtsort's emitted `pin.Pin((uintptr)…UnsafePointer())` hands Pin — resolves
        // through the provenance/token record to its box...
        var box = new StandardBox<Leaf>(new Leaf { x = 2 });
        uintptr number = box;
        pinner.Pin(number);
        Assert.IsTrue(Δruntime.GoIsPinned(AddrOf(box)), "a registered number resolves to its box and pins it");

        // ...and a number nothing resolves is a non-Go pointer: a no-op, no panic.
        pinner.Pin((uintptr)(nuint)0x2000);

        pinner.Unpin();
        Assert.IsFalse(Δruntime.GoIsPinned(AddrOf(box)));
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void TheBoxGainsNoInstanceState()
    {
        // The byte-cost rule as an assertion: a golib change adding instance state to ж<T> (or any
        // per-box base) is a corpus-wide byte cost that has to be STATED. This arm pins the field
        // set the pin table was designed against; a legitimate future field updates it in the same
        // commit that states its cost.
        static string[] Fields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Select(f => f.Name)
                .Where(n => n.StartsWith("m_", StringComparison.Ordinal))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();
        }

        // m_id (option 3, ruling 2026-09-30): a heap box's unique allocation id. Its cost, measured: 0 bytes
        // (72 -> 72 per ж of a 24-byte struct), since an int beside m_isNull fills the base block's padding.
        // m_identityFacts (C51, 2026-11-11): a field view's cached identity facts, one byte read by FieldRefBox alone. Its
        // cost, measured: 0 bytes (a converted *T over one embedded hop allocates 224 B and over five hops 792 B, before and
        // after), since the byte joins m_id in the same padding.
        CollectionAssert.AreEqual(new[] { "m_id", "m_identityFacts", "m_isNull", "m_pin", "m_publishedArrayBacking" }, Fields(typeof(ж<>)),
            "ж<T> gained instance state — the Pinner keeps its bit in a side table, not on the box");
        CollectionAssert.AreEqual(new[] { "m_slot", "m_val" }, Fields(typeof(StandardBox<>)),
            "StandardBox<T> gained instance state");
    }

    // The pins a cost row measures after its warm-up. Odd, so the median is one of them.
    private const int PinsMeasured = 31;

    [TestMethod]
    public void TheCostPerPinIsMeasured()
    {
        Δruntime.Pinner pinner = new(nil);
        var boxes = Enumerable.Range(0, 10 + PinsMeasured).Select(i => new StandardBox<Leaf>(new Leaf { x = i })).ToArray();

        // The first pin allocates the pinner box, its list and the finalizer registration.
        long before = GC.GetAllocatedBytesForCurrentThread();
        pinner.Pin(boxes[0]);
        long firstPin = GC.GetAllocatedBytesForCurrentThread() - before;

        // Warm with nine more pins, then measure a run of distinct pins (DistinctPinCost says how one cost is read off
        // the run) and one repeat pin.
        for (int i = 1; i < 10; i++)
            pinner.Pin(boxes[i]);

        long[] costs = PinCosts(ref pinner, boxes[10..]);
        long distinctPin = DistinctPinCost(costs);

        before = GC.GetAllocatedBytesForCurrentThread();
        pinner.Pin(boxes[10]);
        long repeatPin = GC.GetAllocatedBytesForCurrentThread() - before;

        Console.WriteLine($"runtime.Pinner cost: first pin {firstPin} B (pinner box + list + finalizer registration); distinct object {distinctPin} B (table entry + record + pinned buffer + storage count + list slot; of {costs.Length} pins, least {costs.Min()} B, most {costs.Max()} B); repeat pin {repeatPin} B");

        // cgo S6 (DESIGN-cgo-phase3-marshaling 21): a distinct pin now HOLDS its allocation still, so it adds a PinnedBuffer
        // (a finalizable GCHandle owner), the record's one-element array and a storage count beside the table entry.
        // Measured at Debug: 24 B before S6 (8a7a6d3690), 216 B after, for a reference-free value (one allocation, the fast
        // path). Of the 216 B, 96 B are the pin index's (8 B more of pinned buffer, a weak reference and a node), so a
        // process that links no cgo package reads 120 B. The GCHandle's table slot is not on the GC heap, so this figure
        // does not include it (golib-gen.md).
        Assert.AreEqual(0L, repeatPin, "a repeat pin is a counter increment and allocates nothing");
        Assert.IsTrue(distinctPin > 0 && distinctPin <= 256, $"a distinct pin costs one table entry, one record, one pinned buffer and one storage count, measured {distinctPin} B");

        pinner.Unpin();
        GC.KeepAlive(boxes);
    }

    // The cost above must not depend on where the allocator puts a box. This row puts the first of its measured boxes on
    // a page that holds no live pin, which is the case that read 304 B, on every run: the boxes come after four pages of
    // bytes, and the index itself is asked whether their first page is empty.
    [TestMethod]
    public void TheCostPerPinIsTheSameWhenAPinIsTheFirstOnItsPage()
    {
        Assert.IsNotNull(PinExtents.Current, "GolibTests is a cgo process: the pin index is on");

        const int attempts = 8;

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            Δruntime.Pinner pinner = new(nil);
            var warm = Enumerable.Range(0, 10).Select(i => new StandardBox<Leaf>(new Leaf { x = i })).ToArray();

            foreach (StandardBox<Leaf> box in warm)
                pinner.Pin(box);

            byte[] spacer = new byte[4 * 4096];
            var measured = Enumerable.Range(0, PinsMeasured).Select(i => new StandardBox<Leaf>(new Leaf { x = i })).ToArray();

            // The page is read before the pin and again after it. A box that a collection moved in between, or a page
            // that already held a live pin (another test's), is not the premise: go round with new boxes.
            nuint page = PageOf(measured[0]);
            int liveBefore = PinExtents.EntriesOver(page << 12, (page + 1) << 12);
            long[] costs = PinCosts(ref pinner, measured);
            bool firstOnItsPage = liveBefore == 0 && PageOf(measured[0]) == page;
            long distinctPin = DistinctPinCost(costs);

            pinner.Unpin();
            GC.KeepAlive(warm);
            GC.KeepAlive(spacer);
            GC.KeepAlive(measured);

            if (!firstOnItsPage)
                continue;

            Console.WriteLine($"runtime.Pinner cost, first pin on a page: that pin {costs[0]} B; distinct object {distinctPin} B (of {costs.Length} pins, least {costs.Min()} B, most {costs.Max()} B); the pin index's page entry {costs[0] - distinctPin} B; attempt {attempt}");

            Assert.IsTrue(distinctPin > 0 && distinctPin <= 256, $"a distinct pin costs the same when one of the pins is the first on its page, measured {distinctPin} B");
            Assert.IsTrue(costs[0] > distinctPin, $"the first live pin on a page pays the pin index that page's entry: {costs[0]} B against {distinctPin} B");
            return;
        }

        Assert.Fail($"the premise: in {attempts} attempts no box after four pages of bytes was the first live pin on its page");
    }

    // Pins each box once and answers what each pin allocated on this thread.
    private static long[] PinCosts(ref Δruntime.Pinner pinner, StandardBox<Leaf>[] boxes)
    {
        long[] costs = new long[boxes.Length];

        for (int i = 0; i < boxes.Length; i++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            pinner.Pin(boxes[i]);
            costs[i] = GC.GetAllocatedBytesForCurrentThread() - before;
        }

        return costs;
    }

    // What one distinct pin costs, off the costs of a run of them: the median. A single pin can carry a cost that is not
    // every pin's. The one that was measured: with the pin index on (PinExtents, which every cgo process has, GolibTests
    // included), the first live pin on a 4 KiB page pays the index that page's entry, a Page and its dictionary node,
    // 88 B, so the single pin this file used to measure read 304 B against its 256 B bound whenever the allocator had put
    // its box on a page none of the pins before it was on (linux/amd64 Debug and osx-arm64, 2026-11-28). The others, seen
    // in one run of 31 pins: the Pinner's list doubles at its 17th and 33rd pin (280 B and 536 B more), and a pin now and
    // then reads 1,488 B or 2,768 B more (by its size a table behind the pins growing; the table is not named). Each of
    // those lands on a few pins of a run, and the median does not move until more than half of the pins are hit.
    private static long DistinctPinCost(long[] costs)
    {
        long[] sorted = (long[])costs.Clone();
        Array.Sort(sorted);
        return sorted[sorted.Length / 2];
    }

    // The 4 KiB page a box's value slot lies on, which is the page the pin index files its pin under. It holds still only
    // while the box is pinned.
    private static unsafe nuint PageOf(StandardBox<Leaf> box)
    {
        Array slot = (Array)((INilPointer)box).PinnableStorage!;
        return (nuint)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(slot)) >> 12;
    }
}
