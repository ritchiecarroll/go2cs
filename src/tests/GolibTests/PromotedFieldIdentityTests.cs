using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A promoted field's pointer is ONE Go pointer however it was spelled (COORD 2026-10-12): converted code's flat
/// <c>&amp;o.X</c> (<c>Ꮡo.of(Outer.ᏑX)</c>, a view over <c>o</c>), the nested <c>&amp;o.Inner.X</c>
/// (<c>Ꮡo.of(Outer.ᏑInner).of(Inner.ᏑX)</c>, a view over <c>o</c>'s view of Inner), the implicit chain through
/// <c>p := &amp;o.Inner</c>, and reflect's <c>FieldByName("X").Addr()</c>, which builds the nested chain. They compared
/// unequal: the sources differ, so no arm reached them, though they name one allocation at one Go offset.
/// </summary>
/// <remarks>
/// The rule: two managed-rooted field views of one root allocation, with the same order token (allocation base plus Go
/// offset, exact at every hop) and the same T, are equal. Pinned beside it: distinct allocations stay distinct (C2's Z2,
/// ZeroSizeFieldIdentityTests), and a pointer of another type at the same offset is another pointer.
/// </remarks>
[TestClass]
public class PromotedFieldIdentityTests
{
    // The shapes go2cs-gen emits: an embedded value struct (its ʗ backing field and its Ꮡ accessor), and the outer
    // struct's FLAT promoted accessors, which reach through the embed.
    private struct Inner
    {
        internal nint W;
        internal nint X;

        internal static ref nint ᏑW(ref Inner instance) => ref instance.W;
        internal static ref nint ᏑX(ref Inner instance) => ref instance.X;
    }

    private struct Outer
    {
        internal nint Y;
        private Inner ʗInner;

        [System.Diagnostics.CodeAnalysis.UnscopedRef] internal ref Inner Inner => ref ʗInner;

        internal static ref nint ᏑY(ref Outer instance) => ref instance.Y;
        internal static ref Inner ᏑInner(ref Outer instance) => ref instance.ʗInner;
        internal static ref nint ᏑX(ref Outer instance) => ref instance.ʗInner.X;
    }

    // Two fields at ONE Go offset: a leading zero-size field and the field after it, and a second zero-size field.
    private struct Empty;

    private struct Overlap
    {
        internal Empty A;
        internal Empty B;
        internal nint C;

        internal static ref Empty ᏑA(ref Overlap instance) => ref instance.A;
        internal static ref Empty ᏑB(ref Overlap instance) => ref instance.B;
        internal static ref nint ᏑC(ref Overlap instance) => ref instance.C;
    }

    private static object ReflectField(object box, Type structType, string name) =>
        GoReflect.FieldAliasBox(box, Array.Find(GoReflect.GoFields(structType), field => field.Name == name));

    // reflect's FieldByName("X").Addr() for a promoted X: Field(Inner), then Field(X) on it.
    private static ж<nint> ReflectPromotedX(ж<Outer> o) =>
        (ж<nint>)ReflectField(ReflectField(o, typeof(Outer), "Inner"), typeof(Inner), "X");

    [TestMethod]
    public void TheFlatPromotedPointerEqualsTheNestedOneBothWays()
    {
        ж<Outer> o = Ꮡ(new Outer());

        ж<nint> flat = o.of(Outer.ᏑX);
        ж<nint> nested = o.of(Outer.ᏑInner).of(Inner.ᏑX);

        Assert.IsTrue(flat == nested, "&o.X == &o.Inner.X, as in Go");
        Assert.IsTrue(nested == flat, "and symmetrically");
        Assert.AreEqual(flat.GetHashCode(), nested.GetHashCode(), "equal pointers hash alike");
    }

    [TestMethod]
    public void AMapKeyIsFoundUnderEverySpelling()
    {
        ж<Outer> o = Ꮡ(new Outer());

        Dictionary<ж<nint>, string> byFlat = new() { [o.of(Outer.ᏑX)] = "X" };
        Assert.IsTrue(byFlat.ContainsKey(o.of(Outer.ᏑInner).of(Inner.ᏑX)), "a key stored flat is found nested");
        Assert.IsTrue(byFlat.ContainsKey(ReflectPromotedX(o)), "and through reflect's Addr()");

        Dictionary<ж<nint>, string> byNested = new() { [o.of(Outer.ᏑInner).of(Inner.ᏑX)] = "X" };
        Assert.IsTrue(byNested.ContainsKey(o.of(Outer.ᏑX)), "a key stored nested is found flat");
    }

    [TestMethod]
    public void ReflectsAddrEqualsBothSpellings()
    {
        ж<Outer> o = Ꮡ(new Outer());
        ж<nint> viaReflect = ReflectPromotedX(o);

        Assert.IsTrue(viaReflect == o.of(Outer.ᏑX), "reflect's FieldByName(\"X\").Addr() == &o.X");
        Assert.IsTrue(viaReflect == o.of(Outer.ᏑInner).of(Inner.ᏑX), "== &o.Inner.X");
    }

    [TestMethod]
    public void TheImplicitChainEqualsTheFlatPointer()
    {
        ж<Outer> o = Ꮡ(new Outer());

        ж<Inner> p = o.of(Outer.ᏑInner); // p := &o.Inner
        Assert.IsTrue(p.of(Inner.ᏑX) == o.of(Outer.ᏑX), "&p.X == &o.X");
    }

    [TestMethod]
    public void DifferentFieldsAndDifferentAllocationsStayDifferentPointers()
    {
        ж<Outer> o = Ꮡ(new Outer());
        ж<Outer> other = Ꮡ(new Outer());

        Assert.IsFalse(o.of(Outer.ᏑX) == o.of(Outer.ᏑInner).of(Inner.ᏑW), "&o.X != &o.Inner.W");
        Assert.IsFalse(o.of(Outer.ᏑX) == o.of(Outer.ᏑY), "&o.X != &o.Y");
        Assert.IsFalse(o.of(Outer.ᏑX) == other.of(Outer.ᏑInner).of(Inner.ᏑX), "the same field of another struct is another pointer");
    }

    // The Go Programming Language Specification, Comparison operators: "Pointers to distinct zero-size variables may or
    // may not be equal." Two zero-size fields at one Go offset name one address, so go2cs answers EQUAL, as gc does.
    [TestMethod]
    public void TwoZeroSizeFieldsAtOneOffsetCompareEqual()
    {
        ж<Overlap> s = Ꮡ(new Overlap());

        Assert.IsTrue(s.of(Overlap.ᏑA) == s.of(Overlap.ᏑB), "&s.A == &s.B, both zero-size at offset 0");
    }

    // The same-T condition is what keeps a zero-size field and the int after it apart: one Go offset, two Go types.
    [TestMethod]
    public void FieldsOfDifferentTypesAtOneOffsetStayUnequal()
    {
        ж<Overlap> s = Ꮡ(new Overlap());

        object zero = s.of(Overlap.ᏑA);
        object value = s.of(Overlap.ᏑC);

        Assert.IsFalse(zero.Equals(value), "*Empty at offset 0 is not *int at offset 0");
        Assert.IsFalse(value.Equals(zero), "and symmetrically");
    }
}
