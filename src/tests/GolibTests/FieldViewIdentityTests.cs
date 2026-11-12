// FieldViewIdentityTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Go's <c>==</c> on two pointers to struct fields, over every shape <see cref="FieldRefBox{T}"/>'s equality has an arm for.
/// </summary>
/// <remarks>
/// TypeScript's checker spells every <c>*Type</c> as <c>&amp;data.Type</c>, a field promoted through five embedded structs,
/// so each of its pointer compares is a compare of two field-view chains (C51). A MANAGED-EXACT pair (every hop's Go offset
/// resolved, rooted at a plain heap box) answers from its root and Go offset without walking either chain. Every answer
/// below is the one the full arms gave before that shortcut existed: the shortcut must not move one.
/// </remarks>
[TestClass]
public class FieldViewIdentityTests
{
    // The shape go2cs-gen emits for Go's `type Mid struct { Base; c int }` and `type Top struct { Mid; r int }`.
    public struct Base
    {
        public uint flags;
        public uint id;

        internal static ref uint Ꮡflags(ref Base b) => ref b.flags;
        internal static ref uint Ꮡid(ref Base b) => ref b.id;
    }

    public struct Mid
    {
        [GoEmbedded] public Base Base;
        public nint c;

        internal static ref Base ᏑBase(ref Mid m) => ref m.Base;
        internal static ref nint Ꮡc(ref Mid m) => ref m.c;
    }

    public struct Top
    {
        [GoEmbedded] public Mid Mid;
        public nint r;

        internal static ref Mid ᏑMid(ref Top t) => ref t.Mid;
        internal static ref nint Ꮡr(ref Top t) => ref t.r;

        // Converted code's FLAT promoted `&t.Base`: one view over t, named for the promoted field.
        internal static ref Base ᏑBase(ref Top t) => ref t.Mid.Base;
    }

    // `&t.Base`, nested: t's view of Mid, then Mid's view of Base.
    private static ж<Base> Nested(ж<Top> t) => t.of(Top.ᏑMid).of(Mid.ᏑBase);

    // The same pointer as a FRESH chain of boxes, so ReferenceEquals cannot answer for it.
    private static ж<Base> FreshNested(ж<Top> t) =>
        new FieldRefBox<Base>(new FieldRefBox<Mid>(t, s => ref ((ж<Top>)s).Value.Mid, (FieldRefFunc<Top, Mid>)Top.ᏑMid),
            s => ref ((ж<Mid>)s).Value.Base, (FieldRefFunc<Mid, Base>)Mid.ᏑBase);

    private static void AssertSame<T>(ж<T> a, ж<T> b, string what)
    {
        Assert.IsTrue(a == b, $"{what}: a == b");
        Assert.IsTrue(b == a, $"{what}: b == a");
        Assert.IsFalse(a != b, $"{what}: a != b is false");
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), $"{what}: equal pointers hash alike");
    }

    private static void AssertDistinct<T>(ж<T>? a, ж<T>? b, string what)
    {
        Assert.IsFalse(a == b, $"{what}: a == b is false");
        Assert.IsFalse(b == a, $"{what}: b == a is false");
        Assert.IsTrue(a != b, $"{what}: a != b");
    }

    [TestMethod]
    public void TheSameFieldOfDistinctStructsIsDistinctAtEveryDepth()
    {
        ж<Top> t = Ꮡ(new Top());
        ж<Top> u = Ꮡ(new Top());

        AssertDistinct(t.of(Top.Ꮡr), u.of(Top.Ꮡr), "one hop");
        AssertDistinct(Nested(t), Nested(u), "two hops");
        AssertDistinct(Nested(t).of(Base.Ꮡid), Nested(u).of(Base.Ꮡid), "three hops");
        AssertDistinct(FreshNested(t), Nested(u), "a fresh chain against another root");
    }

    [TestMethod]
    public void OneFieldReachedThroughTwoChainsIsOnePointer()
    {
        ж<Top> t = Ꮡ(new Top());

        AssertSame(Nested(t), Nested(t), "the cached view");
        AssertSame(FreshNested(t), Nested(t), "a fresh chain of boxes");
        AssertSame(FreshNested(t).of(Base.Ꮡid), Nested(t).of(Base.Ꮡid), "one hop further, over two chains");
        AssertSame(t.of(Top.ᏑBase), Nested(t), "flat promoted &t.Base against the nested chain");

        Dictionary<ж<Base>, int> keys = new() { [Nested(t)] = 1 };
        Assert.IsTrue(keys.ContainsKey(FreshNested(t)), "a fresh chain finds the key the cached view set");
        Assert.IsTrue(keys.ContainsKey(t.of(Top.ᏑBase)), "and so does the flat promoted view");
    }

    [TestMethod]
    public void FieldsOfDistinctRootsSharingAnIdAreDistinct()
    {
        // The one answer the MANAGED-EXACT compare changed. Allocation ids are 29 bits and wrap, so two live roots can carry
        // one id, and their views' order tokens then agree. The full arms compared ids and tokens and answered equal; the
        // compare confirms an equal token by the root itself, so these stay two pointers, as Go's are.
        ж<Top> t = Ꮡ(new Top());
        ж<Top> u = Ꮡ(new Top());
        ulong id = t.AllocationId;

        typeof(ж<Top>).GetField("m_id", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(u, (int)id);

        Assert.AreEqual(id, u.AllocationId, "the two roots carry one id");
        Assert.AreEqual(Nested(t).PointerOrderToken, Nested(u).PointerOrderToken, "so their views' order tokens agree");
        AssertDistinct(Nested(t), Nested(u), "&t.Base and &u.Base, whose roots share an id");
        AssertDistinct(Nested(t).of(Base.Ꮡid), FreshNested(u).of(Base.Ꮡid), "and one hop further, over a fresh chain");
    }

    [TestMethod]
    public void TwoFieldsOfOneStructAreDistinct()
    {
        ж<Top> t = Ꮡ(new Top());

        AssertDistinct(t.of(Top.ᏑMid).of(Mid.Ꮡc), t.of(Top.Ꮡr), "&t.c and &t.r");
        AssertDistinct(Nested(t).of(Base.Ꮡflags), Nested(t).of(Base.Ꮡid), "&t.flags and &t.id");
    }

    [TestMethod]
    public void AFirstFieldAndAReinterpretOfItsRootAreOnePointer()
    {
        ж<Top> t = Ꮡ(new Top());

        // (*uint32)(unsafe.Pointer(t)) and &t.Base.flags: one address, one type.
        ж<uint> viaReinterpret = t.Reinterpret<Top, uint>();
        ж<uint> viaFields = Nested(t).of(Base.Ꮡflags);

        AssertSame(viaReinterpret, viaFields, "reinterpret of the root against its first field");
        AssertDistinct(viaReinterpret, Nested(t).of(Base.Ꮡid), "reinterpret of the root against its second field");
    }

    [TestMethod]
    public void AFieldViewEqualsNoOtherKindAndNoNil()
    {
        ж<Top> t = Ꮡ(new Top());
        ж<Base> field = Nested(t);

        AssertDistinct(field, Ꮡ(new Base()), "a field view and a heap box of the field's type");
        AssertDistinct(field, null, "a field view and the C# null nil");
        AssertDistinct(field, go.ж<Base>.NilBox, "a field view and the nil box");
        Assert.IsFalse(field == nil, "a field view is never nil");
    }

    [TestMethod]
    public void ElementRootedViewsCompareByElement()
    {
        slice<Top> s = new(3);

        AssertSame(Ꮡ(s, 1).of(Top.Ꮡr), Ꮡ(s, 1).of(Top.Ꮡr), "&s[1].r twice");
        AssertDistinct(Ꮡ(s, 1).of(Top.Ꮡr), Ꮡ(s, 2).of(Top.Ꮡr), "&s[1].r and &s[2].r");
        AssertSame(Ꮡ(s, 2).of(Top.ᏑMid).of(Mid.ᏑBase), Ꮡ(s, 2).of(Top.ᏑBase), "&s[2].Base, nested and flat");
    }

    public struct Empty
    {
    }

    public struct HoldsEmpty
    {
        [GoEmbedded] public Empty Empty;

        internal static ref Empty ᏑEmpty(ref HoldsEmpty h) => ref h.Empty;
    }

    [TestMethod]
    public void FieldsOfZeroSizeHeapStructsKeepTheirAnswer()
    {
        // Every zero-size heap allocation is Go's zerobase, so two such boxes are one pointer, and so are their fields.
        // That rests on the root's equality, not its identity: these views must keep the full arms.
        ж<HoldsEmpty> a = Ꮡ(new HoldsEmpty());
        ж<HoldsEmpty> b = Ꮡ(new HoldsEmpty());

        Assert.IsTrue(a == b, "two zero-size heap boxes are the zerobase");
        AssertSame(a.of(HoldsEmpty.ᏑEmpty), b.of(HoldsEmpty.ᏑEmpty), "their fields are one pointer too");
    }

    [TestMethod]
    public unsafe void NativeRootedViewsCompareByAddress()
    {
        void* memory = NativeMemory.AllocZeroed((nuint)sizeof(Base) * 2);

        try
        {
            ж<Base> first = new NativeBox<Base>((nuint)memory);
            ж<Base> again = new NativeBox<Base>((nuint)memory);
            ж<Base> second = new NativeBox<Base>((nuint)memory + (nuint)sizeof(Base));

            AssertSame(first.of(Base.Ꮡid), again.of(Base.Ꮡid), "one native address through two boxes");
            AssertDistinct(first.of(Base.Ꮡid), second.of(Base.Ꮡid), "two native structs");
            AssertDistinct(first.of(Base.Ꮡid), Nested(Ꮡ(new Top())).of(Base.Ꮡid), "a native field and a managed one");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
}
