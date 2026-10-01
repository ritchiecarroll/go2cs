using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// OPTION 3 of the hash-token sizing (COORD ruling 2026-09-30): a ж token names ONE live allocation.
// ж tokens were minted from the CLR identity hash (AllocationBase), which collides between live objects
// (the first collision among live ж<object> boxes came after 4622 boxes). A reference-bearing box
// REGISTERS its token on every uintptr conversion and ManagedPointerTokens.Register keeps the LAST
// writer, so `(ж<T>)(uintptr)p` of the FIRST box's number handed back the SECOND box: a silent wrong
// object, since both answer Resolve's verify.
[TestClass]
public class PointerTokenUniquenessTests
{
    [TestMethod]
    public void APointerRoundTripReturnsItsOwnBoxAmongCollidingIdentityHashes()
    {
        Dictionary<int, ж<object>> byHash = new();
        List<ж<object>> live = new();
        ж<object>? first = null, second = null;

        for (int made = 0; second is null && made < 2_000_000; made++)
        {
            ref object value = ref heap((object)new object(), out ж<object> box);
            _ = value;
            live.Add(box);

            if (byHash.TryGetValue(RuntimeHelpers.GetHashCode(box), out ж<object>? prior))
            {
                first = prior;
                second = box;
            }
            else
            {
                byHash[RuntimeHelpers.GetHashCode(box)] = box;
            }
        }

        Assert.IsNotNull(second, "no two live boxes shared an identity hash in 2M; the arm measured nothing");

        uintptr firstNumber = first!;
        uintptr secondNumber = second;
        ж<object> back = (ж<object>)firstNumber;

        Assert.AreNotEqual((ulong)(nuint)firstNumber, (ulong)(nuint)secondNumber, "two live allocations must have two numbers");
        Assert.IsTrue(ReferenceEquals(back, first), "the first box's number resolved to another live box");

        System.GC.KeepAlive(live);
    }

    // A FIELD of an ELEMENT: &s[i].P. The field view's base is its source's allocation, and an element
    // box's allocation is its backing array, so the element's own position must survive into the field's
    // token or every element's P shares one number. P sits at Go offset 8 and holds a reference, so it
    // converts to a token (a reference-free field keeps its real address), and &s[8] probes an element
    // index against a field displacement of the same size.
    public struct Elem
    {
        public long A;
        public string P;
    }

    // Accessors named as go2cs-gen names them (Ꮡ + the Go field name), so each view resolves its field's
    // REAL Go offset; an unnamed accessor falls back to a delegate hash, and two of those can agree.
    private static ref string ᏑP(object source) => ref ((ж<Elem>)source).Value.P;
    private static ref Elem ᏑX(object source) => ref ((ж<Pair>)source).Value.X;
    private static ref Elem ᏑY(object source) => ref ((ж<Pair>)source).Value.Y;

    private static readonly FieldRefFunc<string> s_p = ᏑP;

    [TestMethod]
    public void FieldOfElementPointersOfDistinctSliceElementsAreDistinct()
    {
        slice<Elem> s = make<slice<Elem>>(16);

        ж<string> p0 = Ꮡ(s, 0).of(s_p);
        ж<string> p1 = Ꮡ(s, 1).of(s_p);
        ж<Elem> e8 = Ꮡ(s, 8);

        AssertDistinctGoPointers(p0, p1, Ꮡ(s, 1).of(s_p), e8);
    }

    [TestMethod]
    public void FieldOfElementPointersOfDistinctArrayElementsAreDistinct()
    {
        array<Elem> a = new(16);

        ж<string> p0 = Ꮡ(a, 0).of(s_p);
        ж<string> p1 = Ꮡ(a, 1).of(s_p);
        ж<Elem> e8 = Ꮡ(a, 8);

        AssertDistinctGoPointers(p0, p1, Ꮡ(a, 1).of(s_p), e8);
    }

    // The same base loss one level up: a field of a FIELD. &o.X.P and &o.Y.P are 24 bytes apart in Go
    // (Pair's X and Y are each an Elem), and the inner view's base must carry X's or Y's own offset.
    public struct Pair
    {
        public Elem X;
        public Elem Y;
    }

    private static readonly FieldRefFunc<Elem> s_x = ᏑX;
    private static readonly FieldRefFunc<Elem> s_y = ᏑY;

    [TestMethod]
    public void FieldOfFieldPointersOfDistinctOuterFieldsAreDistinct()
    {
        heap(new Pair(), out ж<Pair> o);

        ж<string> xp = o.of(s_x).of(s_p);
        ж<string> yp = o.of(s_y).of(s_p);
        nuint tx = (nuint)(uintptr)xp, ty = (nuint)(uintptr)yp;

        Assert.IsFalse(xp == yp, "&o.X.P == &o.Y.P, where Go compares two different addresses");
        Assert.AreNotEqual(tx, ty, "&o.X.P and &o.Y.P converted to ONE number: the outer field's offset was lost");
        Assert.AreEqual(24ul, (ulong)(ty - tx), "&o.Y.P - &o.X.P is Go's 24 bytes");

        ((ж<string>)(uintptr)tx).Value = "x";
        ((ж<string>)(uintptr)ty).Value = "y";

        Assert.AreEqual("x", o.Value.X.P, "&o.X.P's number resolved to another field");
        Assert.AreEqual("y", o.Value.Y.P, "&o.Y.P's number resolved to another field");
    }

    // p0 = &x[0].P, p1 = &x[1].P, p1Again = a second &x[1].P, e8 = &x[8]: Go gives p0, p1 and e8 three
    // addresses and p1 == p1Again. Each token must round-trip to a box that writes ITS OWN element.
    private static void AssertDistinctGoPointers(ж<string> p0, ж<string> p1, ж<string> p1Again, ж<Elem> e8)
    {
        nuint t0 = (nuint)(uintptr)p0, t1 = (nuint)(uintptr)p1, t1Again = (nuint)(uintptr)p1Again, t8 = (nuint)(uintptr)e8;

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(t0) && ManagedPointerTokens.IsTaggedToken(t8),
            "the probe must read TOKENS: P and Elem hold references, so neither may convert to a raw address");
        Assert.IsFalse(p0 == p1, "&x[0].P == &x[1].P, where Go compares two different addresses");
        Assert.IsTrue(p1 == p1Again, "two &x[1].P must be equal, as in Go");
        Assert.AreEqual(t1, t1Again, "equal pointers must convert to equal numbers");
        Assert.AreNotEqual(t0, t1, "&x[0].P and &x[1].P converted to ONE number: the element position was lost from the field's token");
        Assert.AreNotEqual(t0, t8, "&x[0].P (Go offset 8) and &x[8] converted to ONE number");
        Assert.AreEqual(24ul, (ulong)(t1 - t0), "&x[1].P - &x[0].P is Go's 24 bytes, one Elem");

        ((ж<string>)(uintptr)t0).Value = "zero";
        ((ж<string>)(uintptr)t1).Value = "one";

        Assert.AreEqual("zero", p0.Value, "&x[0].P's number resolved to another element's field");
        Assert.AreEqual("one", p1.Value, "&x[1].P's number resolved to another element's field");
    }
}
