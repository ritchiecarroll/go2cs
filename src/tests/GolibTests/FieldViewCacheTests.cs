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

    // Two fields of ONE type: two accessors with the same delegate type and the same (null) target that
    // differ only in their method, and a picker whose one instance method reads either field by its target.
    private struct pair
    {
        public inner A;
        public inner B;
        internal static ref inner ᏑA(ref pair p) => ref p.A;
        internal static ref inner ᏑB(ref pair p) => ref p.B;
    }

    private sealed class picker(bool second)
    {
        internal ref inner Pick(ref pair p) => ref second ? ref p.B : ref p.A;
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
    public void DistinctDelegateInstancesOfOneAccessorShareOneViewAndDistinctAccessorsDoNot()
    {
        // Each `&x.f` site converts the accessor method group to its OWN delegate, so one field reaches a
        // box as several instances. The lookup matches them by type, target and method (ж.Views.cs
        // sameAccessor), which is Delegate.Equals' answer for a single-cast delegate.
        FieldViewCache.Disabled = false;
        ж<pair> box = Ꮡ(new pair());

        FieldRefFunc<pair, inner> a1 = new(pair.ᏑA);
        FieldRefFunc<pair, inner> a2 = new(pair.ᏑA);
        Assert.IsFalse(ReferenceEquals(a1, a2), "precondition: two delegate instances");
        ж<inner> viewA = box.of(a1);
        Assert.IsTrue(ReferenceEquals(viewA, box.of(a2)), "two instances of one accessor must return the SAME view");

        // same delegate type, same null target, a different method: a different field
        ж<inner> viewB = box.of(new FieldRefFunc<pair, inner>(pair.ᏑB));
        Assert.IsFalse(ReferenceEquals(viewA, viewB), "a different accessor method is a different view");
        viewB.Value.pad = 5;
        Assert.AreEqual(5, box.Value.B.pad);
        Assert.AreEqual(0, box.Value.A.pad, "and it must alias its own field, not the first entry's");

        // one instance method on two targets: the target decides the field, so it decides the view
        picker first = new(false), second = new(true);
        ж<inner> pickA = box.of(new FieldRefFunc<pair, inner>(first.Pick));
        ж<inner> pickB = box.of(new FieldRefFunc<pair, inner>(second.Pick));
        Assert.IsFalse(ReferenceEquals(pickA, pickB), "one method on two targets is two accessors");
        Assert.IsTrue(ReferenceEquals(pickA, box.of(new FieldRefFunc<pair, inner>(first.Pick))), "one method on one target is one accessor");
        pickB.Value.pad = 9;
        Assert.AreEqual(9, box.Value.B.pad, "the second target's view aliases B");
        pickA.Value.pad = 3;
        Assert.AreEqual(3, box.Value.A.pad, "the first target's view aliases A");
    }

    [TestMethod]
    public void TheNativeAotFormOfTheAccessorMatchGivesTheSameAnswers()
    {
        // Native AOT takes Delegate.Equals in sameAccessor (RuntimeFeature.IsDynamicCodeSupported is false there),
        // and tests run under the JIT. The switch makes the JIT take that form, so the same identity assertions
        // run against it: one field's distinct instances share a view, distinct fields and targets do not, and
        // a multicast token keeps its own view.
        FieldViewCache.CompareByDelegateEquals = true;

        try
        {
            DistinctDelegateInstancesOfOneAccessorShareOneViewAndDistinctAccessorsDoNot();
            AMulticastTokenComparesByDelegateEqualsNotByItsLastTarget();
        }
        finally
        {
            FieldViewCache.CompareByDelegateEquals = false;
        }
    }

    [TestMethod]
    public void AMulticastTokenComparesByDelegateEqualsNotByItsLastTarget()
    {
        // No converted site builds a multicast accessor, but should one reach the cache it must keep
        // Delegate.Equals' answer. A multicast delegate's Target and Method name its LAST entry only, so a
        // type/target/method match would take `ᏑA + ᏑB` for `ᏑB` and hand back the view cached for it.
        FieldViewCache.Disabled = false;
        ж<pair> box = Ꮡ(new pair());

        ж<inner> viewB = box.of(new FieldRefFunc<pair, inner>(pair.ᏑB));
        FieldRefFunc<pair, inner> both = new FieldRefFunc<pair, inner>(pair.ᏑA) + new FieldRefFunc<pair, inner>(pair.ᏑB);
        Assert.AreEqual(nameof(pair.ᏑB), both.Method.Name, "precondition: the multicast's Method is its last entry's");
        ж<inner> viewBoth = box.of(both);
        Assert.IsFalse(ReferenceEquals(viewB, viewBoth), "a multicast token is not its last entry: Delegate.Equals says they differ");

        FieldRefFunc<pair, inner> bothAgain = new FieldRefFunc<pair, inner>(pair.ᏑA) + new FieldRefFunc<pair, inner>(pair.ᏑB);
        Assert.IsTrue(ReferenceEquals(viewBoth, box.of(bothAgain)), "an equal multicast token, by Delegate.Equals, returns the same view");
        Assert.IsTrue(ReferenceEquals(viewB, box.of(new FieldRefFunc<pair, inner>(pair.ᏑB))), "and the single-cast entry still finds its own view");
    }

    [TestMethod]
    public void AnElementBoxCachesItsViewsOnItsOwnSlot()
    {
        // `&a[i].f` asks of() of an element box, often a fresh one per expression: its views live on the
        // box's own slot, never in the weak table, whose insert takes a lock.
        FieldViewCache.Disabled = false;
        array<outer> backing = new(3);
        ElemRefBox<outer> element = new(backing, 1);
        Assert.IsNull(element.m_views, "precondition: no view asked yet");

        ж<inner> first = element.of(outer.ᏑI);
        ж<inner> second = element.of(outer.ᏑI);
        Assert.IsTrue(ReferenceEquals(first, second), "two of() calls on one element box return the SAME view");
        Assert.IsNotNull(element.m_views, "the view is cached on the element box's own slot");

        first.Value.pad = 11;
        Assert.AreEqual(11, backing[1].I.pad, "the view aliases the array element");
        Assert.AreEqual(0, backing[0].I.pad);

        ElemRefBox<outer> again = new(backing, 1);
        ж<inner> fresh = again.of(outer.ᏑI);
        Assert.IsFalse(ReferenceEquals(first, fresh), "a fresh element box caches its own view");
        Assert.AreEqual(11, fresh.Value.pad, "of the same element");
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
