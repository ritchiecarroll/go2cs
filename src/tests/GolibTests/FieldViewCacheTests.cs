using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the field-view cache (golib <c>ж.Views.cs</c>, docs/phase4/DESIGN-field-view-cache.md): a
/// box's <c>of()</c> returns ONE cached <c>FieldRefBox</c> per (box, accessor) for the box's life,
/// the slot on consumer-type boxes (<c>SlottedStandardBox&lt;T&gt;</c>) and on views, the weak table
/// for every other box, the nil box never caching. The arms are the design's §5: identity across
/// calls (the contract the address-keyed semaphores key on — equal views are now the SAME object),
/// the +8/+0 byte split by type, a chain's inner hop caching on the outer view's slot, the fallback
/// for a box minted before its type flipped, the nil box, a negative arm that disables the cache
/// through <c>FieldViewCache.Disabled</c> and must see identity FAIL, and concurrent publication.
/// The acceptance row is measured outside this class: the os row under SUB-Q32's protocol on B2's
/// base, predicted 488.25 B / 6 objects.
/// </summary>
[TestClass]
public class FieldViewCacheTests
{
    // Hand-written shapes with the generator's accessor form (`static ref F ᏑF(ref T t) => ref t.F`):
    // a consumer type, a chain (outer -> inner -> leaf), a type that never sees an of(), a type that
    // flips lazily, and an attributed type whose boxes are slotted from their first mint.
    private struct leaf
    {
        public nint n;
    }

    private struct inner
    {
        public leaf L;
        public nint pad;
        internal static ref leaf ᏑL(ref inner i) => ref i.L;
    }

    private struct outer
    {
        public inner I;
        public leaf K;
        internal static ref inner ᏑI(ref outer o) => ref o.I;
        internal static ref leaf ᏑK(ref outer o) => ref o.K;
    }

    private struct never
    {
        public nint n;
    }

    private struct late
    {
        public leaf L;
        internal static ref leaf ᏑL(ref late l) => ref l.L;
    }

    [GoBoxViews]
    private struct marked
    {
        public leaf L;
        internal static ref leaf ᏑL(ref marked m) => ref m.L;
    }

    [TestMethod]
    public void TwoCallsWithOneAccessorReturnTheSameViewAndDifferentFieldsOrBoxesDoNot()
    {
        FieldViewCache.Disabled = false;
        ж<outer> box = Ꮡ(new outer());
        ж<inner> first = box.of(outer.ᏑI);
        ж<inner> second = box.of(outer.ᏑI);

        Assert.IsTrue(ReferenceEquals(first, second), "two of() calls with one accessor on one box must return the SAME view");
        Assert.IsTrue(first.Equals(second), "and the views must still compare equal (the pre-cache contract)");

        ж<leaf> other = box.of(outer.ᏑK);
        Assert.IsFalse(ReferenceEquals(first, other), "a different field is a different view");

        ж<outer> box2 = Ꮡ(new outer());
        Assert.IsFalse(ReferenceEquals(first, box2.of(outer.ᏑI)), "a different box is a different view of the same field");
        Assert.IsFalse(box.of(outer.ᏑI).Equals(box2.of(outer.ᏑI)), "and it must not compare equal either (different source)");

        // the view aliases the box's storage: a write through it is read through the box
        first.Value.pad = 41;
        Assert.AreEqual(41, box.Value.I.pad);
        box.Value.I.pad = 42;
        Assert.AreEqual(42, second.Value.pad);
    }

    [TestMethod]
    public void AConsumerTypesBoxGrowsByExactlyEightBytesAndANonConsumersDoesNot()
    {
        FieldViewCache.Disabled = false;

        // a type that never sees an of(): its box size is the baseline
        long neverBytes = BytesOfMint(() => Ꮡ(new never()));
        long neverAgain = BytesOfMint(() => Ꮡ(new never()));
        Assert.AreEqual(neverBytes, neverAgain, "the baseline mint must be repeatable");

        // the attributed type is slotted from its first mint: exactly +8 over the same-shaped baseline
        // (never and marked's leaf are both one nint wide; marked's box is a SlottedStandardBox)
        ж<marked> m = Ꮡ(new marked());
        Assert.IsInstanceOfType(m, typeof(SlottedStandardBox<marked>), "[GoBoxViews] mints the slotted box from the first mint");
        long markedBytes = BytesOfMint(() => Ꮡ(new marked()));
        Assert.AreEqual(neverBytes + 8, markedBytes, "a consumer-type box costs exactly +8 B — the slot — over a non-consumer box of the same shape");

        // after the flip a lazily-marked type's boxes cost the same +8; before it, +0
        Assert.IsFalse(BoxShape<late>.Slotted, "late has not flipped yet in this process");
        long lateBefore = BytesOfMint(() => Ꮡ(new late()));
        Assert.AreEqual(neverBytes, lateBefore, "before the flip a box of a not-yet-consumer type costs +0");
        Ꮡ(new late()).of(late.ᏑL);
        Assert.IsTrue(BoxShape<late>.Slotted, "the first of() on any box of the type flips it");
        long lateAfter = BytesOfMint(() => Ꮡ(new late()));
        Assert.AreEqual(neverBytes + 8, lateAfter, "after the flip a box of the type carries the slot: +8");
    }

    [TestMethod]
    public void AChainsInnerHopCachesOnTheOuterViewsOwnSlot()
    {
        FieldViewCache.Disabled = false;
        ж<outer> box = Ꮡ(new outer());
        ж<inner> hop = box.of(outer.ᏑI);
        ж<leaf> first = hop.of(inner.ᏑL);
        ж<leaf> second = box.of(outer.ᏑI).of(inner.ᏑL);

        Assert.IsTrue(ReferenceEquals(first, second), "the inner hop of a chain is one cached view");
        Assert.IsInstanceOfType(hop, typeof(FieldRefBox<inner>));
        Assert.IsNotNull(((FieldRefBox<inner>)hop).m_views, "the inner view is cached on the outer view's own slot, not in the weak table");

        first.Value.n = 7;
        Assert.AreEqual(7, box.Value.I.L.n, "the chained view aliases the leaf");
    }

    [TestMethod]
    public void ABoxMintedBeforeItsTypeFlippedStillReturnsOneStableViewThroughTheFallback()
    {
        FieldViewCache.Disabled = false;

        // `outer` has flipped by now (other arms ran) or not — either way, force the pre-flip shape
        // by minting a plain StandardBox explicitly and asking it for views
        StandardBox<outer> plain = new(new outer());
        Assert.IsNotInstanceOfType(plain, typeof(SlottedStandardBox<outer>));
        ж<inner> a = plain.of(outer.ᏑI);
        ж<inner> b = plain.of(outer.ᏑI);
        Assert.IsTrue(ReferenceEquals(a, b), "a slotless box caches through the weak table: one stable view");

        // and a box minted after the flip carries the slot
        Assert.IsTrue(BoxShape<outer>.Slotted);
        Assert.IsInstanceOfType(Ꮡ(new outer()), typeof(SlottedStandardBox<outer>));
    }

    [TestMethod]
    public void TheNilBoxNeverCaches()
    {
        FieldViewCache.Disabled = false;
        // `go.ж<outer>` spelled out: in MEMBER-ACCESS position the bare `ж<outer>` binds to golib's
        // dereference method group `builtin.ж<T>(in ж<T>)` (the static using), not the pointer type
        ж<outer> nilBox = go.ж<outer>.NilBox;
        Assert.IsTrue(nilBox.IsNilPointer);
        ж<inner> a = nilBox.of(outer.ᏑI);
        ж<inner> b = nilBox.of(outer.ᏑI);
        Assert.IsFalse(ReferenceEquals(a, b), "the nil box mints fresh views and retains nothing");
        Assert.IsTrue(a.Equals(b), "which still compare equal, as before");
    }

    [TestMethod]
    public void WithTheCacheDisabledIdentityAcrossCallsFails_TheNegativeArm()
    {
        // The neuter: FieldViewCache.Disabled makes of() mint on every call exactly as it did before
        // the cache. The identity assertion of the first arm must then FAIL — which is what proves
        // that assertion is load-bearing. Restored in finally.
        FieldViewCache.Disabled = true;
        try
        {
            ж<outer> box = Ꮡ(new outer());
            ж<inner> first = box.of(outer.ᏑI);
            ж<inner> second = box.of(outer.ᏑI);
            Assert.IsFalse(ReferenceEquals(first, second), "with the cache disabled two calls mint two views");
            Assert.IsTrue(first.Equals(second), "which compare equal (the pre-cache contract)");
        }
        finally
        {
            FieldViewCache.Disabled = false;
        }
    }

    [TestMethod]
    public void ConcurrentFirstCallsOnOneBoxAgreeOnOneView()
    {
        FieldViewCache.Disabled = false;
        const int callers = 16;

        for (int round = 0; round < 50; round++)
        {
            ж<outer> box = Ꮡ(new outer());
            ж<inner>[] seen = new ж<inner>[callers];
            using Barrier gate = new(callers);

            Parallel.For(0, callers, i =>
            {
                gate.SignalAndWait();
                seen[i] = box.of(outer.ᏑI);
            });

            for (int i = 1; i < callers; i++)
                Assert.IsTrue(ReferenceEquals(seen[0], seen[i]), $"round {round}: caller {i} saw a different view — the compare-exchange publish lost a race");
        }
    }

    // Bytes allocated by ONE mint, read on this thread with the counter's own primitive; the box is
    // kept alive across the read so the allocation is real.
    // Bytes allocated by ONE mint, read with the runtime's own per-thread counter.
    private static long BytesOfMint<T>(Func<go.ж<T>> mint)
    {
        // warm the generic instantiation and the JIT before measuring
        go.ж<T> warm = mint();
        GC.KeepAlive(warm);

        long before = GC.GetAllocatedBytesForCurrentThread();
        go.ж<T> box = mint();
        long after = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(box);
        return after - before;
    }
}
