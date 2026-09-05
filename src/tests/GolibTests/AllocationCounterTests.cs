// AllocationCounterTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Pins the go2cs runtime allocation counter's census — the per-site object counts
/// <see cref="AllocationCounter"/> claims, and the invariant that makes reporting them safe.
/// </summary>
/// <remarks>
/// <para>
/// The counter exists because Go's <c>testing.AllocsPerRun</c> reports a malloc COUNT and the CLR
/// publishes none; golib is the runtime here, so it counts at its own allocation sites exactly as
/// Go's runtime does. The census is documented in <c>docs/phase4/DESIGN-allocation-counting.md</c>,
/// and a documented census that nothing checks is a census that drifts — every charge below is a
/// claim that document makes, asserted as an exact number rather than a range.
/// </para>
/// <para>
/// <b>The invariant in <see cref="CountedObjectsNeverExceedTheirByteCost"/> is the load-bearing
/// one.</b> `AllocsPerRun` reports the count in place of the byte figure, and that substitution is
/// only safe while it is MONOTONE — a counted object costs at least the CLR's 24-byte object header
/// minimum, so `count ≤ bytes / 24` must hold and the reported value can only ever fall. A site that
/// charged more objects than it allocated would break that and could turn a passing assert into a
/// failing one corpus-wide.
/// </para>
/// <para>
/// Counting is process-global and one-way by design (<see cref="AllocationCounter.Enable"/> has no
/// inverse: a run that begins counting counts for its lifetime), so these tests enable it and leave
/// it on. That is harmless here and matches how the test host uses it.
/// </para>
/// </remarks>
[TestClass]
public class AllocationCounterTests
{
    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    // Runs `action` and returns the objects golib charged to this thread for it, with the byte cost
    // alongside so a charge can be audited against what actually reached the heap.
    private static (long objects, long bytes) Charge(Action action)
    {
        // Warm up so JIT and first-call lazy initialization land outside the window, exactly as
        // AllocsPerRun's own warm-up run does.
        action();

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long beforeBytes = GC.GetAllocatedBytesForCurrentThread();

        action();

        long bytes = GC.GetAllocatedBytesForCurrentThread() - beforeBytes;

        return (AllocationCounter.CurrentThreadCount - beforeCount, bytes);
    }

    private static void AssertCharge(long expected, Action action, string what)
    {
        (long objects, long bytes) = Charge(action);

        Assert.AreEqual(expected, objects,
            $"{what} charged {objects} object(s), expected {expected} ({bytes} B measured). The " +
            "census in docs/phase4/DESIGN-allocation-counting.md §4 and this assertion have to " +
            "agree — update both, or neither.");
    }

    // ---- the r57c result the census depends on ----

    [TestMethod]
    public void SlicingAStringChargesNothing()
    {
        @string s = new("the quick brown fox jumps over the lazy dog");

        // Go's `s[a:b]` is an O(1) window over shared immutable storage and allocates nothing. Since
        // r57c @string carries backing+offset+length, so ours does too — this is the one census row
        // whose correct charge is ZERO, and it is a result rather than an omission.
        AssertCharge(0L, () =>
        {
            @string sub = s[4..9];

            if (sub.Length != 5)
                throw new InvalidOperationException("slice window is wrong");
        }, "s[a:b] (string window)");
    }

    // ---- @string materialization ----

    [TestMethod]
    public void StringMaterializationChargesItsBacking()
    {
        AssertCharge(1L, () => { @string _ = new("a literal reaching @string"); },
            "@string(string) — the UTF-8 backing every C# literal encodes into");

        @string source = new("materialize me");

        AssertCharge(1L, () => { string _ = source.ToString(); },
            "@string.ToString() — the materialized System.String");

        AssertCharge(1L, () => { slice<byte> _ = source; },
            "[]byte(s) — Go's copying conversion");

        AssertCharge(1L, () => { @string _ = source + source; },
            "s + t — the single result buffer");

        // char[] costs two: the intermediate UTF-16 string, then the UTF-8 backing it encodes into.
        char[] chars = ['g', 'o', '2', 'c', 's'];

        AssertCharge(2L, () => { @string _ = new(chars); },
            "@string(char[]) — intermediate string plus its UTF-8 backing");
    }

    [TestMethod]
    public void TmpStringMapProbeChargesNothing()
    {
        // Go's `m[string(b)]` map-READ special case (runtime.slicebytetostringtmp): the compiler
        // proves the key does not outlive the lookup and skips the copy, so the probe allocates
        // NOTHING. The converter emits golib's tmpstring(b) for exactly that shape, and this is the
        // path net/textproto's canonicalMIMEHeaderKey common-header probe takes 200 times inside a
        // want-ZERO AllocsPerRun assert (L11) — the charge must be zero in BOTH units, since
        // AllocsPerRun's zero-bytes rule is what makes the reported figure exactly 0.
        map<@string, @string> m = new();
        @string interned = new("Content-Length");
        m[interned] = interned;

        slice<byte> key = new(new byte[] { (byte)'C', (byte)'o', (byte)'n', (byte)'t', (byte)'e', (byte)'n', (byte)'t', (byte)'-', (byte)'L', (byte)'e', (byte)'n', (byte)'g', (byte)'t', (byte)'h' });

        (long objects, long bytes) = Charge(() =>
        {
            @string v = m[tmpstring(key)];

            if (v != interned)
                throw new InvalidOperationException("probe missed");
        });

        Assert.AreEqual(0L, objects,
            $"m[tmpstring(b)] charged {objects} object(s), expected 0 — the transient alias must not " +
            "materialize the key (the census in docs/phase4/DESIGN-allocation-counting.md §4 and this " +
            "assertion have to agree — update both, or neither).");

        Assert.AreEqual(0L, bytes,
            $"m[tmpstring(b)] allocated {bytes} B, expected 0 — testing.AllocsPerRun reports exactly 0 " +
            "only when zero BYTES reach the heap, so the want-zero stdlib asserts depend on this in " +
            "both units.");

        // The alias WINDOWS the slice's live storage — the aliasing is the mechanism, so pin it:
        // canonicalizing the slice in place must be visible through a fresh transient.
        key[0] = (byte)'c';

        Assert.IsTrue(tmpstring(key) == new @string("content-Length"),
            "tmpstring must alias the slice's LIVE bytes, not a snapshot");

        key[0] = (byte)'C';
    }

    [TestMethod]
    public void RangingAStringChargesItsEnumerator()
    {
        @string s = new("räng");

        // Go's `for range s` allocates nothing; ours hands out an enumerator object, and the count
        // is precisely where that difference is supposed to become visible.
        AssertCharge(1L, () =>
        {
            foreach ((nint _, rune _) in s)
            {
            }
        }, "for range s — the enumerator");
    }

    // ---- the shapes r56d decomposed, which the rsa and nistec verdicts rest on ----

    [TestMethod]
    public void HeapBoxOverAnUnmanagedValueChargesBoxAndSlot()
    {
        // The canonical divergence: Go's `&x` is ONE malloc; the CLR box additionally allocates the
        // one-element pinnable slot its address stability requires. Two objects, and the count says
        // two — this is the shape r56d decomposed nistec's per-run bill into.
        AssertCharge(2L, () => { ж<long> _ = new StandardBox<long>(42L); }, "ж<long> — box plus eager pinnable slot");
    }

    // ---- the B1 per-kind split's leaf-ctor census (DESIGN-zh-box-b1.md §5) ----
    // One exact number per kind ctor: the base charges nothing, each leaf charges what the
    // pre-split unified ctor charged for its shape. These are the numbers the ratified design
    // binds; a kind that starts charging differently is a census change, not a refactor.

    [TestMethod]
    public void HeapBoxOverAManagedValueChargesBoxAlone()
    {
        // With a managed T there is no eager pinnable slot (pinning managed storage is
        // meaningless), so the standard kind charges exactly the box — the split's std-managed 1.
        @string held = "abc";

        AssertCharge(1L, () => { ж<@string> _ = new StandardBox<@string>(held); },
            "ж<@string> — box only, no slot for managed T");
    }

    [TestMethod]
    public void NilBoxChargesBoxAlone()
    {
        // The nil ctor never materializes a slot, unmanaged T included — nothing dereferences a
        // nil box without panicking first, so there is no address to stabilize.
        AssertCharge(1L, () => { ж<long> _ = new StandardBox<long>(nil); },
            "ж<long> nil — box only");
    }

    [TestMethod]
    public void FieldReferenceMintChargesBoxAlone()
    {
        // Go's `&x.field` — the accessor delegate is compiler-cached at real call sites, so the
        // mint itself is one object: the FieldRefBox. This pins the CONSTRUCTOR row of the census
        // (as the elem-ref, native and nil arms beside it do), constructed directly: since the
        // field-view cache (ж.Views.cs) `of()` reaches this ctor once per (box, field) and answers
        // the cached view afterwards, so an `of()` inside Charge's window — which follows Charge's
        // own warm-up call on the same box — measures the cache, not the mint (the arm below does).
        ж<FieldHost> host = new StandardBox<FieldHost>(default(FieldHost));

        AssertCharge(1L, () => { ж<long> _ = new FieldRefBox<long>(host, s_hostField); },
            "FieldRefBox ctor — the field-ref box only");
    }

    [TestMethod]
    public void FieldReferenceRepeatOfChargesNothing()
    {
        // The field-view cache's contract in the counting suite: a repeat `of()` on the same box for
        // the same field returns the cached view, charging no object and allocating no bytes. Charge's
        // warm-up call is the first `of()` (the mint, charged 1 by the ctor row above); the window sees
        // the hit. Both numbers are asserted, because the cut's whole yield is this row reading 0 / 0.
        ж<FieldHost> host = new StandardBox<FieldHost>(default(FieldHost));

        (long objects, long bytes) = Charge(() => { ж<long> _ = host.of(s_hostField); });

        Assert.AreEqual(0L, objects,
            $"a repeat of(...) charged {objects} object(s) — the field-view cache must answer it ({bytes} B measured)");
        Assert.AreEqual(0L, bytes,
            $"a repeat of(...) allocated {bytes} B — the cached view must cost nothing per call");
    }

    private struct FieldHost
    {
        public long Field;
    }

    private static readonly FieldRefFunc<long> s_hostField = static p => ref ((ж<FieldHost>)p).Value.Field;

    [TestMethod]
    public void ElementReferenceMintChargesBoxAlone()
    {
        // Go's `&s[i]` — the element kind charges its box; the backing array was charged where it
        // was made. (builtin's public Ꮡ path documents one extra charge for the caller's IArray
        // boxing temp; that is the call site's census, not this ctor's.)
        array<byte> backing = new(4);

        AssertCharge(1L, () => { ж<byte> _ = new ElemRefBox<byte>(backing, 1); },
            "element reference — the elem-ref box only");
    }

    [TestMethod]
    public void NativeAddressBoxChargesBoxAlone()
    {
        // A uintptr round-trip mints the address-identity kind: one box, no slot, no referent.
        AssertCharge(1L, () => { ж<nint> _ = new NativeBox<nint>(0x4000u); },
            "native address box — the box only");
    }

    [TestMethod]
    public void MakeAndAppendChargeTheirBackingArrays()
    {
        // Non-const so .NET 9's stack-allocation optimization cannot keep the backing off the heap;
        // with a literal length this measures zero objects because nothing is allocated at all.
        nint length = s_sliceLength;

        AssertCharge(1L, () => { slice<byte> _ = new(length); }, "make([]byte, n) — the backing array");

        // Made OUTSIDE the window: appending to an at-capacity slice regrows without mutating the
        // source header, so each measured run allocates exactly the one new backing array.
        slice<byte> full = new(length, length);

        AssertCharge(1L, () => { _ = append(full, (byte)1); },
            "append past capacity — the regrown backing array");
    }

    private static readonly nint s_sliceLength = 16;

    // ---- the invariant that makes the substitution safe ----

    [TestMethod]
    public void CountedObjectsNeverExceedTheirByteCost()
    {
        // 24 bytes is the CLR's minimum heap object size on x64 (header + method table + a field
        // slot, rounded to the allocation granularity). No charged object can cost less, so a census
        // that charges more objects than bytes/24 is over-counting somewhere -- and over-counting is
        // the one direction that could turn a passing assert into a failing one.
        const long MinimumObjectBytes = 24L;

        // Every shape hands its result to AllocationProbe.Escape rather than discarding it, and at
        // the configuration validation runs under that is what keeps this a measurement rather than
        // a false alarm. Release with tiering OFF is fully optimized from the first call, so .NET's
        // escape analysis removes an allocation whose object never leaves the lambda — and the
        // BYTES side of this invariant is the side that then under-reports while golib's CHARGE
        // stays exactly what it was. Measured at TC0 before the escapes: `ж<long>` charged its two
        // objects (box + eager pinnable slot) against only 32 B reaching the heap — the slot array
        // alone, the box elided — and this assertion reported an over-charge that does not exist.
        // The direction matters: a stack-allocated object can only ever make `objects * 24 > bytes`
        // MORE likely, so an unescaped table produces phantom violations, never hides real ones.
        // See AllocationProbe for the isolated measurement (0 B vs 56 B on one build).
        (string what, Action action)[] shapes =
        [
            ("@string(string)",   () => { @string v = new("a literal reaching @string"); AllocationProbe.Escape(v); }),
            ("@string(char[])",   () => { @string v = new(new[] { 'g', 'o' }); AllocationProbe.Escape(v); }),
            ("s + t",             () => { @string v = new @string("left") + new @string("right"); AllocationProbe.Escape(v); }),
            ("[]byte(s)",         () => { slice<byte> v = new @string("convert me"); AllocationProbe.Escape(v); }),
            ("string(b)",         () => { @string v = new(new slice<byte>(4)); AllocationProbe.Escape(v); }),
            ("ToString()",        () => { string v = new @string("materialize").ToString(); AllocationProbe.Escape(v); }),
            ("ToRunes()",         () => { rune[] v = new @string("räng").ToRunes(); AllocationProbe.Escape(v); }),
            ("for range s",       () => { @string v = new("räng"); foreach ((nint _, rune _) in v) { } AllocationProbe.Escape(v); }),
            ("ж<long>",           () => { ж<long> v = new StandardBox<long>(42L); AllocationProbe.Escape(v); }),
            ("make([]byte, n)",   () => { slice<byte> v = new(s_sliceLength); AllocationProbe.Escape(v); }),
            ("make(map)",         () => { map<@string, nint> v = new(); AllocationProbe.Escape(v); }),
            ("make(chan, 4)",     () => { channel<nint> v = new(4); AllocationProbe.Escape(v); })
        ];

        List<string> violations = [];

        foreach ((string what, Action action) in shapes)
        {
            (long objects, long bytes) = Charge(action);

            if (objects * MinimumObjectBytes > bytes)
                violations.Add($"{what}: charged {objects} object(s) = at least " +
                               $"{objects * MinimumObjectBytes} B, but only {bytes} B reached the heap");

            Console.WriteLine($"{what,-18} {objects,3} object(s) {bytes,6} B");
        }

        Assert.AreEqual(0, violations.Count,
            "AllocsPerRun reports the COUNT in place of the byte figure, which is only safe while " +
            "count <= bytes/24 holds — the substitution has to be monotone downward so that no test " +
            "passing on bytes can fail on the count. Over-charged site(s):" +
            Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    // ---- the gate itself ----

    [TestMethod]
    public void CountingIsOffUntilAHostEnablesIt()
    {
        // Not a behavioral assertion about the current process (ClassInitialize has already enabled
        // counting, and there is deliberately no way back). It pins the SHAPE: the gate is readable,
        // so a converted application can be shown to pay nothing, and TestHost.Run is the only
        // caller of Enable in the tree.
        Assert.IsTrue(AllocationCounter.Enabled,
            "counting must be observable through the public gate — AllocsPerRun's fallback arm " +
            "reads it to decide whether a zero count means 'nothing allocated' or 'not counting'");
    }
}
