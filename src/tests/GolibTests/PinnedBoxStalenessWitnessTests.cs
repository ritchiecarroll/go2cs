// PinnedBoxStalenessWitnessTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;
using static go.builtin;

namespace GolibTests;

// THE PINNED-BOX STALENESS WITNESS — a `ж<T>` whose T carries a managed reference, handed across a
// seam as `unsafe.Pointer.FromPinnedBox(box)` and recovered through its NUMBER, must come back
// aliasing the same storage. Landed 2026-09-04 as a REPRODUCER (5 of 5 RED under its gate at both
// configurations — the history kept below) and CLOSED by Q44, the managed pointer TOKEN
// (docs/phase4/DESIGN-managed-pointer-token.md): the address-take operators in ж.cs hand a
// reference-bearing box its own order token, registered in ManagedPointerTokens, instead of the
// address of a movable field — so arms 3 and 4 are this file's ACCEPTANCE, ungated, and arm 1's last
// two steps and arm 5 state the token's properties rather than the address's defect.
//
// WHERE THE WITNESS CAME FROM. Measured in one instrumented run of runtime/pprof's
// TestGoroutineCounts: 192 goroutines, 91 labelled through `runtime_setProfLabel` storing
// `FromPinnedBox`'s number, 90 read back correctly at profile time, and ONE — the finalizer
// goroutine's labelMap, set inside a finalizer body by pprof.Do — read `len == 1` at the instant it
// was stored and `len == 1885431144` when the profile read it back after two collections.
// printCountProfile sizes a slice from that length, so the host died with OutOfMemoryException and
// the row was classified `infrastructure-error` — a host defect, not a verdict. The label half of
// runtime/pprof's goroutine profile is withheld because of it
// (src/core/runtime/pprof/pprof_impl.cs); filling `labels[i]` from `entry.Labels` in
// `pprof_goroutineProfileWithLabels` is the ONE LINE that re-enters when these arms go green.
//
// THE TWO SIDES, VERBATIM. The set side is runtime/pprof/runtime.cs:36
//
//     runtime_setProfLabel(@unsafe.Pointer.FromPinnedBox(ctxLabels));
//
// and the read side is runtime/pprof/pprof.cs:919
//
//     return (ж<labelMap>)(uintptr)(p.labels[i]);
//
// The `(uintptr)` bridge is what discards the Pointer's retained box (unsafe.cs:337 returns the
// bare value), so the recovery has only the number, and `(ж<T>)(uintptr)` consults
// ManagedPointerTokens.Resolve and otherwise mints a NativeBox over the raw address (ж.cs:612-622).
//
// THE MECHANISM BEFORE Q44, and it was NOT the one the pin-release class has. For a reference-BEARING
// T, StandardBox allocates no `m_slot` (ж.StandardBox.cs), so `PinnableStorage` is null, so
// `EnsureStableAddress` never called `PinnedBuffer.PinOnly` and `m_pin` stayed null. No PinnedBuffer
// was ever CONSTRUCTED for such a box — so "the pin was released by its finalizer" (the mechanism
// AliasOverlapRaceTests and PinLifetimeAtTheNativeBoundaryTests measure, where a pin IS taken and
// dies with its box) was never this defect's mechanism, and there was nothing for a finalizer
// counter to count. The `fixed` address was nonetheless REGISTERED
// (`ManagedPointerTokens.RegisterPinned`), validate-on-read then refused it by design (`IsPinnedAt`
// returns false the moment `m_pin` is null), the recovery MISSED, and the consumer was handed a
// native alias of an address the collector was never asked to hold still. Arm 1 still measures
// steps 1–4 of that — they are true under the token too: no slot, no pin, not pinned at the number —
// and its steps 5–6 flipped with the fix.
//
// THE FIX (Q44). Both address-take operators (`ж<T> → uintptr` and `ж<T> → void*`, ж.cs) take a new
// arm BEFORE EnsureStableAddress: when `PinnableStorage` is null the number handed out is the box's
// `PointerOrderToken` (for a StandardBox the identity hash's allocation base — high 32 bits, never a
// heap address), registered weakly in ManagedPointerTokens, so `(ж<T>)(uintptr)` recovers the very
// box through Resolve's order-token arm for as long as something else keeps it alive (the record's
// weak-lifetime rule; the goroutine registry keeps pprof's alive through the Pointer's retained
// source). Nothing on the read side changed, and nothing about a box that DOES pin changed.
//
// THE COUPLED GUARDS, and which way each moved. DarwinKeystoneArgsRecoveryTests' reference-bearing
// arm — banked stating the MISS as the keystone design's bound — takes its other branch under the
// token and is rewritten in the same cut as ReferenceBearingArgsStructResolvesThroughItsToken_TheBoundQ44Narrowed;
// NativeAddressStabilityTests.ReferenceBearingPointeeIsLeftAlone is predicted NOT to move (the fix
// adds no storage, it mints a token, so the arm's two assertions stay true) and the cut's run of it
// is the measurement rather than an edit.
//
// HOW IT WAS GATED, and why the gate is gone. While the hole was open arms 3 and 4 reported
// INCONCLUSIVE by default (GolibTests' idiom for "this is not a green") and went RED under
// `GO2CS_PIN_STALENESS_STRICT=1`, the flag the fixing arc would run and then delete. Q44 is that
// arc: the flag, the Disclosure text and the Strict property are deleted with it, and the two arms
// are plain assertions — a miss here is a failure now, never a disclosure.
//
// ⚠ ONE ARM PER PROCESS does not apply here and it is worth saying why: what the frame holds decides
// what COLLECTS, and nothing in these arms is measured by collection — the box is deliberately kept
// alive for every reading (the goroutine registry keeps the real one alive through the Pointer's
// retained source), so the arms measure RELOCATION and RESOLUTION, both of which survive being run
// in one host. The configuration-dependence AliasOverlapRaceTests records is predicted NOT to reach
// these arms either, because what they assert is structural — under the token, a property of the
// box's identity hash rather than of any frame — which is a prediction to be measured at both
// configurations, not a claim.
//
// THE PREDICTION, ON RECORD BEFORE ANY RUN — posted to the fleet mailbox at `ce89582f9`, which
// predates the first execution of this file. Arms 1, 2 and 5 PASS; arms 3 and 4 FAIL under the flag,
// 5 of 5, at Debug and at Release with tiering off; the mechanism is (1) no PinnedBuffer is ever
// CONSTRUCTED for a reference-bearing box, (2) there is no `m_slot` to be re-allocated, (3) the
// address is a stale copy the collector was never asked to hold still. The measured result lands as
// a FOLLOW-UP commit rather than by editing this block.
//
// THE RESULT, MEASURED 2026-09-04 on the i7 coordinator class (net10.0, windows flavour), and it
// held exactly. Under `GO2CS_PIN_STALENESS_STRICT=1`, filtered to this class: **5 of 5 at Debug and
// 5 of 5 at Release with `DOTNET_TieredCompilation=0`** — `Failed: 2, Passed: 3, Skipped: 0,
// Total: 5` on all ten runs, the SAME two arms (3 and 4) every time, zero aborts. Skipped: 0 is
// load-bearing: no arm reported "the control array did not move", so every reading is a measurement
// rather than a vacuous pass. Ungated, both configurations: `Failed: 0, Passed: 3, Skipped: 2,
// Total: 5`, exit 0. Full GolibTests, ungated, both configurations: **598 of 598 declared**
// (593 at master + these 5, derived from the COMPILE set — the three linux-flavour files the csproj
// removes under the default `$(GoTargetOS)` are subtracted), 0 failures, 0 aborts, exit 0; the only
// new skips are arms 3 and 4, the other four (Debug) / one (Release) being pre-existing and
// documented.
//
// So the configuration prediction held: the gated arms' assertion is STRUCTURAL and reads the same
// at Debug and at Release, unlike AliasOverlapRaceTests' four-take race, which a non-optimizing
// frame masks. And the mechanism prediction held on every step — arm 1 passes each of its six
// assertions, which is the bisect: `PinnableStorage` null, `m_pin` null BOTH before and after the
// address take (so no PinnedBuffer is ever constructed and there is nothing for a finalizer to
// release), `IsPinnedAt` false, `Resolve` null, and the recovery `IsNative`.
//
// ⚠ One claim in this header was written as intended and MEASURED to be narrower, corrected here
// rather than left: at `dotnet test`'s DEFAULT console verbosity a skip prints its NAME and not its
// reason, so the disclosure text reaches `--logger "console;verbosity=detailed"` and the .trx, not
// the summary line.
//
// CLOSED BY Q44 (cut 2026-09-05). The prediction on record for THIS cut
// (docs/phase4/DESIGN-managed-pointer-token.md §8, posted before any run of it): all five arms PASS
// ungated, 10 of 10 across Debug and Release+TC0; arm 1's steps 5–6 flip to "Resolve returns the box
// and the recovery IS the box"; arm 5 flips from "the address goes stale when the box moves" to "the
// token is the same number after the move and still resolves". The measured result lands as a
// FOLLOW-UP commit, as it did the first time.
[TestClass]
public class PinnedBoxStalenessWitnessTests
{
    // A reference-BEARING pointee, the shape of runtime/pprof's `labelMap`
    // (`[GoType("map[@string, @string]")] partial struct labelMap`, label.cs:54). golib's map is a
    // readonly struct over a dictionary, so the type carries a managed reference and StandardBox
    // gives it no pinnable slot — which is the whole premise, asserted rather than assumed below.
    private struct LabelMapShape
    {
        internal map<@string, @string> Entries;
    }

    // The reference-FREE control shape: the class that DOES pin, so the same round trip works.
    private struct PinnableShape
    {
        internal ulong A;
        internal ulong B;
    }

    // ---- the instrument (independent of golib, per the control doctrine) ------------------------

    private static object? s_sink;

    // Enough gen0 garbage to fragment the heap, then two FORCED COMPACTING collections with a
    // finalizer drain between them — the same churn PinLifetimeAtTheNativeBoundaryTests uses, for
    // the same reason (a dropped box's pin is released by ~PinnedBuffer, so the drain is what lets
    // the second collection relocate anything).
    private static void Churn()
    {
        for (int i = 0; i < 50_000; i++)
        {
            s_sink = new byte[64];
        }

        s_sink = null;

        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    // Reads an ordinary array's current address WITHOUT leaving it pinned — the control instrument,
    // owing nothing to the machinery under test.
    private static nuint CurrentAddress(byte[] array)
    {
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);

        try
        {
            return (nuint)handle.AddrOfPinnedObject();
        }
        finally
        {
            handle.Free();
        }
    }

    // The box's OWN pin field. `m_pin` is `private protected` on ж<T>, so InternalsVisibleTo does
    // not reach it and reflection is the only read — and it is the read that answers the bisect's
    // first question outright: null means no PinnedBuffer was ever constructed FOR THIS BOX, so
    // there is nothing a finalizer could have released.
    private static object? PinOf(object box)
    {
        for (Type? type = box.GetType(); type is not null; type = type.BaseType)
        {
            FieldInfo? field = type.GetField("m_pin", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (field is not null)
                return field.GetValue(box);
        }

        throw new InvalidOperationException("ж<T>.m_pin was not found: the bisect instrument is reading a shape golib no longer has");
    }

    // The pinnable slot's IDENTITY — the second bisect question ("is the box's m_slot
    // re-allocated?"). Null for a reference-bearing T by construction; a stable object for the
    // control, which is what makes "not re-allocated" a measurement rather than a claim.
    private static object? SlotOf<T>(ж<T> box) => ((INilPointer)box).PinnableStorage;

    // Once a plain gate: the arms this reports for are the ACCEPTANCE of Q44, so a miss here is a
    // failure, not a disclosure — the GO2CS_PIN_STALENESS_STRICT gate that made them Inconclusive
    // while the hole was open is deleted with the hole.
    private static void Report(bool ok, string message)
    {
        Assert.IsTrue(ok, message);
    }

    // ---- ARM 1: THE BISECT ----------------------------------------------------------------------

    [TestMethod]
    public void TheReferenceBearingSetSideNeverEntersThePinPath()
    {
        // Named as the mechanism rather than the symptom, and UNCONDITIONAL: it is true today, and
        // the day the pin arc changes it this arm goes red naming exactly which step moved.
        Assert.IsTrue(RuntimeHelpers.IsReferenceOrContainsReferences<LabelMapShape>(),
            "the premise: a labelMap-shaped pointee carries a managed reference");

        ref LabelMapShape held = ref heap<LabelMapShape>(out ж<LabelMapShape> box);
        held.Entries = new map<@string, @string>();
        held.Entries[(@string)"k"] = (@string)"v";

        Assert.IsNull(SlotOf(box),
            "step 1: a reference-bearing StandardBox allocates no pinnable slot (m_slot)");

        Assert.IsNull(PinOf(box),
            "step 2 (precondition): no pin exists before the address is taken");

        @unsafe.Pointer pointer = @unsafe.Pointer.FromPinnedBox(box);
        nuint number = (nuint)(uintptr)pointer;

        Assert.AreNotEqual((nuint)0, number,
            "the address take must still report a number — the mint is not the thing that is broken");

        // THE BISECT'S ANSWER. Not "the PinnedBuffer was released" — no PinnedBuffer was ever
        // constructed, so a finalizer counter has nothing to count on this path; and not "m_slot was
        // re-allocated" — there is no m_slot. The pin path DECLINES, and everything downstream
        // follows from that one fact.
        Assert.IsNull(PinOf(box),
            "step 3, THE MECHANISM: EnsureStableAddress declined (PinnableStorage is null), so no " +
            "PinnedBuffer was constructed for this box — 'the pin was released by its finalizer' is " +
            "not this defect's mechanism");

        Assert.IsFalse(((INilPointer)box).IsPinnedAt(number),
            "step 4: validate-on-read must refuse a number the box is not pinned at");

        Assert.AreSame(box, ManagedPointerTokens.Resolve(number),
            "step 5 (FLIPPED by Q44): the number is the box's order TOKEN, registered at the address take, " +
            "so the provenance record resolves it to this very box — no pin, no slot, and no miss");

        // And the consumer's emission (pprof.cs:919) is therefore handed the box itself.
        ж<LabelMapShape> recovered = (ж<LabelMapShape>)(uintptr)pointer;

        Assert.AreSame(box, recovered,
            "step 6 (FLIPPED by Q44): the recovery is the box itself, aliasing its storage — never a " +
            "NativeBox over a number that was not an address");
        Assert.IsFalse(recovered.IsNative, "and it is not a native alias");

        GC.KeepAlive(pointer);
    }

    // ---- ARM 2: THE POSITIVE CONTROL --------------------------------------------------------------

    // The control's set side, in a NON-INLINED frame for the same reason arm 3's is: the two arms
    // must differ on ONE axis (the pointee's reference-bearing-ness) and not also on whether the
    // caller's frame roots the box. Boxing inline here was the first cut and it was a two-axis
    // comparison — corrected before the guard banked.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @unsafe.Pointer SetControlTheSameWay(out ж<PinnableShape> box)
    {
        ref PinnableShape held = ref heap<PinnableShape>(out box);
        held.A = 0xFEEDFACE;

        return @unsafe.Pointer.FromPinnedBox(box);
    }

    [TestMethod]
    public void AReferenceFreeBoxRoundTripsThroughFromPinnedBoxAcrossACollection()
    {
        // What makes arms 3 and 4 measurements rather than tautologies: the SAME seam, the SAME
        // churn, the SAME non-inlined set frame, a pointee that differs only in carrying no managed
        // reference — and it comes back aliasing its own storage. This arm also answers the bisect's
        // second question directly: the pinnable slot's identity is unchanged across the collection,
        // so nothing is re-allocated.
        Assert.IsFalse(RuntimeHelpers.IsReferenceOrContainsReferences<PinnableShape>(),
            "the control's premise: this shape carries no managed reference");

        byte[] control = new byte[64];
        nuint controlBefore = CurrentAddress(control);

        @unsafe.Pointer pointer = SetControlTheSameWay(out ж<PinnableShape> box);
        nuint number = (nuint)(uintptr)pointer;

        object? slotBefore = SlotOf(box);
        object? pinBefore = PinOf(box);

        Assert.IsNotNull(slotBefore, "the control's premise: a reference-free StandardBox has a pinnable slot");
        Assert.IsNotNull(pinBefore, "the control's premise: taking the address pinned that slot");

        Churn();

        if (controlBefore == CurrentAddress(control))
        {
            Assert.Inconclusive("the control array did not move; this arm measured nothing");
            return;
        }

        Assert.AreSame(slotBefore, SlotOf(box), "the pinnable slot was re-allocated under the collection");
        Assert.AreSame(pinBefore, PinOf(box), "the pin was replaced under the collection");

        Assert.AreSame(box, ManagedPointerTokens.Resolve(number),
            "a pinned box's number must resolve back to that box after a collection");

        ж<PinnableShape> recovered = (ж<PinnableShape>)(uintptr)pointer;

        Assert.AreSame(box, recovered,
            "the emitted recovery form must hand back the same box for a pinned pointee");

        // The alias itself, read through the recovered pointer: a write on one side is visible on
        // the other because they are one storage location — and the value written BEFORE the
        // collection is still there, so this is a move, never a copy.
        Assert.AreEqual(0xFEEDFACEUL, recovered.Value.A, "the value written before the collection did not survive");

        box.Value.B = 0x5A5A5A5A;
        Assert.AreEqual(0x5A5A5A5AUL, recovered.Value.B, "the recovered box did not alias the original storage");

        GC.KeepAlive(control);
        GC.KeepAlive(pointer);
    }

    // ---- ARM 3: THE WITNESS, MAIN THREAD ---------------------------------------------------------

    // The set side, in its own NON-INLINED frame so the caller's frame roots no box — exactly what
    // SetGoroutineLabels does: the registry keeps the POINTER, and the box lives only because the
    // Pointer retains it.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @unsafe.Pointer SetLabelTheWayPprofDoes()
    {
        ref LabelMapShape held = ref heap<LabelMapShape>(out ж<LabelMapShape> box);
        held.Entries = new map<@string, @string>();
        held.Entries[(@string)"pin"] = (@string)"witness";

        return @unsafe.Pointer.FromPinnedBox(box);
    }

    [TestMethod]
    public void APinnedBoxNumberMustStillAliasItsStorageAfterACollection()
    {
        byte[] control = new byte[64];
        nuint controlBefore = CurrentAddress(control);

        @unsafe.Pointer pointer = SetLabelTheWayPprofDoes();
        nuint number = (nuint)(uintptr)pointer;

        // The box is alive throughout — the Pointer retains it, which is precisely what the
        // goroutine registry holds. This is a RELOCATION question, never a use-after-free one, and
        // the distinction is what keeps the arm safe to run: nothing below dereferences the number.
        object? original = pointer.RetainedSource;
        Assert.IsNotNull(original, "the mint must retain its box, or this arm is measuring the wrong defect");

        Churn();

        if (controlBefore == CurrentAddress(control))
        {
            Assert.Inconclusive("the control array did not move; this arm measured nothing");
            return;
        }

        // The read side, verbatim (pprof.cs:919).
        ж<LabelMapShape> recovered = (ж<LabelMapShape>)(uintptr)pointer;

        bool aliases = ReferenceEquals(recovered, original);

        Report(aliases,
            $"the number a reference-bearing box reported ({number:X}) did not recover that box after " +
            $"two collections: the token arm (Q44) registers the box's order token at the address take " +
            $"and Resolve must find it, yet the consumer holds something else (IsNative={recovered.IsNative}). " +
            "This is the pprof label round trip, and reading a labelMap's length through a missed " +
            "recovery is what read 1885431144 and killed the host");

        // Only reachable once the recovery aliases — never dereference a number that did not.
        ж<LabelMapShape> aliased = (ж<LabelMapShape>)original!;
        aliased.Value.Entries[(@string)"written-after"] = (@string)"1";

        Assert.AreEqual(2, len(recovered.Value.Entries),
            "the recovered box must alias the original storage, not a copy of it");

        GC.KeepAlive(control);
        GC.KeepAlive(pointer);
    }

    // ---- ARM 4: THE WITNESS, FROM INSIDE A FINALIZER BODY ----------------------------------------

    private static @unsafe.Pointer? s_finalizerPointer;

    private static bool s_finalizerRan;

    // pprof.Do called from inside a finalizer body — the ONE goroutine of the 91 that failed. The
    // labelMap is allocated on the finalizer thread, during finalization, which is what puts it in
    // gen0 at the moment two collections are about to run.
    private sealed class LabelSettingFinalizer
    {
        ~LabelSettingFinalizer()
        {
            ref LabelMapShape held = ref heap<LabelMapShape>(out ж<LabelMapShape> box);
            held.Entries = new map<@string, @string>();
            held.Entries[(@string)"finalizer"] = (@string)"1";

            s_finalizerPointer = @unsafe.Pointer.FromPinnedBox(box);
            s_finalizerRan = true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateAndDropTheFinalizer()
    {
        _ = new LabelSettingFinalizer();
    }

    [TestMethod]
    public void ALabelSetFromInsideAFinalizerBodyMustAliasItsStorageToo()
    {
        s_finalizerPointer = null;
        s_finalizerRan = false;

        byte[] control = new byte[64];
        nuint controlBefore = CurrentAddress(control);

        AllocateAndDropTheFinalizer();

        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        if (!s_finalizerRan || s_finalizerPointer is null)
        {
            Assert.Inconclusive("the finalizer did not run; this arm measured nothing");
            return;
        }

        @unsafe.Pointer pointer = s_finalizerPointer;
        nuint number = (nuint)(uintptr)pointer;
        object? original = pointer.RetainedSource;

        Assert.IsNotNull(original, "the mint must retain its box, or this arm is measuring the wrong defect");

        Churn();

        if (controlBefore == CurrentAddress(control))
        {
            Assert.Inconclusive("the control array did not move; this arm measured nothing");
            return;
        }

        ж<LabelMapShape> recovered = (ж<LabelMapShape>)(uintptr)pointer;

        Report(ReferenceEquals(recovered, original),
            $"a label set from inside a FINALIZER body did not recover its box from the number " +
            $"({number:X}) after two collections (IsNative={recovered.IsNative}) — the token arm must " +
            "hold from the finalizer thread too; this is the exact arm that failed in " +
            "TestGoroutineCounts, the finalizer goroutine's labelMap");

        ж<LabelMapShape> aliased = (ж<LabelMapShape>)original!;
        aliased.Value.Entries[(@string)"written-after"] = (@string)"1";

        Assert.AreEqual(2, len(recovered.Value.Entries),
            "the recovered box must alias the original storage, not a copy of it");

        GC.KeepAlive(control);
        GC.KeepAlive(pointer);
    }

    // ---- ARM 5: THE TOKEN IS STABLE ACROSS A MOVE -------------------------------------------------

    [TestMethod]
    public void TheNumberAReferenceBearingBoxReportsIsStableWhenTheBoxMoves()
    {
        // Before Q44 this arm measured the defect's CONSEQUENCE: the number was a real address of the
        // box's value field at the instant it was taken, nothing held that address still, and
        // re-taking it after a collection reported a different one — so a consumer's stored number
        // named other memory, and `before == after` was the Inconclusive branch ("the box did not
        // relocate under this churn"). Under the token the number is the box's ORDER TOKEN, a
        // property of the object's identity hash and not of where the collector currently keeps
        // it, so the equality is the ASSERTED case now. The structural assertion below (the number
        // IS the token) is the load-bearing one; the post-churn equality, with the control array
        // demonstrably moved, is that property observed rather than an unmoved coincidence.
        byte[] control = new byte[64];
        nuint controlBefore = CurrentAddress(control);

        ref LabelMapShape held = ref heap<LabelMapShape>(out ж<LabelMapShape> box);
        held.Entries = new map<@string, @string>();

        @unsafe.Pointer pointer = @unsafe.Pointer.FromPinnedBox(box);
        nuint before = (nuint)(uintptr)pointer;

        Assert.AreEqual(box.PointerOrderToken, before,
            "the number a reference-bearing box hands across the seam is its order token, not an address");

        Churn();

        if (controlBefore == CurrentAddress(control))
        {
            Assert.Inconclusive("the control array did not move; this arm measured nothing");
            return;
        }

        nuint after = (nuint)(uintptr)box;

        Assert.AreEqual(before, after,
            "the token handed across the seam changed under a compacting collection — it is a " +
            "property of the box's identity, never of its address");

        Assert.AreSame(box, ManagedPointerTokens.Resolve(after),
            "the token must still resolve to the box after the collection");

        // And the storage is intact.
        Assert.IsNotNull(held.Entries);

        GC.KeepAlive(control);
        GC.KeepAlive(pointer);
    }
}
