using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards seat #46: the field-view cache's hit path (golib <c>ж.Views.cs</c>) is a separate, inlined method in front of
/// the full lookup. #32 made every <c>&amp;p.f</c> site pass the field's ONE token (<c>T.Ꮡf‿</c>), so a hit is a
/// reference match on the box's own view slot, small enough to inline at every site. It must not depend on dynamic
/// PGO, which gives no profile to a method of one basic block: PerfFieldView's <c>step</c> became one under #32, its
/// tier-1 code stopped inlining the lookup, and the JIT row read +4.7% (R's JitDisasm attribution, 2026-10-30). The
/// structure guard is the red arm (it fails with one viewOf); the behaviour arms pin what the split must keep: a
/// token hit allocates nothing and answers the cached view, a miss publishes once, a weak-table box and an older
/// site's own delegate still find their view through the full lookup, and a disabled cache bypasses a view that was
/// cached before it was disabled.
/// </summary>
[TestClass]
public class ViewCacheHitPathTests
{
    private struct leaf
    {
        public nint n;
    }

    // A consumer type with #32's shape: the generator's accessor and the field's one token per field.
    [GoBoxViews]
    private struct point
    {
        public leaf X;
        public leaf Y;
        public leaf Z;
        internal static ref leaf ᏑX(ref point p) => ref p.X;
        internal static ref leaf ᏑY(ref point p) => ref p.Y;
        internal static ref leaf ᏑZ(ref point p) => ref p.Z;
        internal static readonly FieldRefFunc<point, leaf> ᏑX‿ = ᏑX;
        internal static readonly FieldRefFunc<point, leaf> ᏑY‿ = ᏑY;
        internal static readonly FieldRefFunc<point, leaf> ᏑZ‿ = ᏑZ;
    }

    [TestMethod]
    public void TheTypedViewOfIsAnInlinedHitPathInFrontOfANoInliningFullLookup()
    {
        MethodInfo[] methods = typeof(ж<point>).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        static bool typed(MethodInfo method) =>
            method.IsGenericMethodDefinition && method.GetParameters() is [{ ParameterType: { IsGenericType: true } type }] &&
            type.GetGenericTypeDefinition() == typeof(FieldRefFunc<,>);

        MethodInfo? hit = methods.SingleOrDefault(method => method.Name == "viewOf" && typed(method));
        MethodInfo? slow = methods.SingleOrDefault(method => method.Name == "viewOfSlow" && typed(method));

        Assert.IsNotNull(hit, "the typed viewOf exists");
        Assert.IsTrue(hit!.MethodImplementationFlags.HasFlag(MethodImplAttributes.AggressiveInlining),
            "the hit path is inlined at every site, with or without a PGO profile");
        Assert.IsNotNull(slow, "the full lookup is its own method, viewOfSlow");
        Assert.IsTrue(slow!.MethodImplementationFlags.HasFlag(MethodImplAttributes.NoInlining),
            "the full lookup (the list walk by sameAccessor, the publish, the wrapper table) is never inlined");
    }

    [TestMethod]
    public void ATokenHitAnswersTheCachedViewAndAllocatesNothing()
    {
        FieldViewCache.Disabled = false;

        ж<point> box = Ꮡ(new point());
        Assert.IsInstanceOfType(box, typeof(SlottedStandardBox<point>), "a consumer type's box carries the view slot");

        ж<leaf> x = box.of(point.ᏑX‿);
        ж<leaf> y = box.of(point.ᏑY‿);
        ж<leaf> z = box.of(point.ᏑZ‿);

        // warm every call before measuring
        for (int i = 0; i < 4; i++)
        {
            box.of(point.ᏑX‿);
            box.of(point.ᏑY‿);
            box.of(point.ᏑZ‿);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 1000; i++)
        {
            Assert.IsTrue(ReferenceEquals(x, box.of(point.ᏑX‿)));
            Assert.IsTrue(ReferenceEquals(y, box.of(point.ᏑY‿)));
            Assert.IsTrue(ReferenceEquals(z, box.of(point.ᏑZ‿)), "the last-published view and the first are both hits");
        }

        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.AreEqual(0L, after - before, "a hit allocates nothing");

        x.Value.n = 7;
        Assert.AreEqual(7, box.Value.X.n, "the cached view aliases the field");
    }

    [TestMethod]
    public void AMissPublishesOnceAndTheNextCallHits()
    {
        FieldViewCache.Disabled = false;

        ж<point> box = Ꮡ(new point());
        ж<leaf> first = box.of(point.ᏑY‿);

        Assert.IsTrue(ReferenceEquals(first, box.of(point.ᏑY‿)), "the miss published the view the next call hits");
        Assert.IsFalse(ReferenceEquals(first, Ꮡ(new point()).of(point.ᏑY‿)), "another box's miss mints its own view");
    }

    [TestMethod]
    public void AnOlderSitesOwnDelegateFindsTheTokensViewThroughTheFullLookup()
    {
        FieldViewCache.Disabled = false;

        ж<point> box = Ꮡ(new point());
        ж<leaf> view = box.of(point.ᏑX‿);

        // not the token object, so not a reference hit: the full lookup's sameAccessor matches it by its method
        Assert.IsTrue(ReferenceEquals(view, box.of(new FieldRefFunc<point, leaf>(point.ᏑX))));
    }

    [TestMethod]
    public void ASlotlessBoxAnswersOneStableViewThroughTheWeakTable()
    {
        FieldViewCache.Disabled = false;

        StandardBox<point> slotless = new(new point());
        Assert.IsNotInstanceOfType(slotless, typeof(SlottedStandardBox<point>));

        ж<leaf> a = slotless.of(point.ᏑX‿);
        Assert.IsTrue(ReferenceEquals(a, slotless.of(point.ᏑX‿)), "no slot to read, so the full lookup answers, the same view");
    }

    [TestMethod]
    public void DisablingTheCacheBypassesAViewCachedBeforeIt()
    {
        FieldViewCache.Disabled = false;

        ж<point> box = Ꮡ(new point());
        ж<leaf> cached = box.of(point.ᏑX‿);

        FieldViewCache.Disabled = true;

        try
        {
            ж<leaf> first = box.of(point.ᏑX‿);
            ж<leaf> second = box.of(point.ᏑX‿);

            Assert.IsFalse(ReferenceEquals(cached, first), "the hit path does not answer from the slot while the cache is disabled");
            Assert.IsFalse(ReferenceEquals(first, second), "with the cache disabled two calls mint two views");
            Assert.IsTrue(first.Equals(cached), "which compare equal to the cached one (the pre-cache contract)");
        }
        finally
        {
            FieldViewCache.Disabled = false;
        }
    }
}
