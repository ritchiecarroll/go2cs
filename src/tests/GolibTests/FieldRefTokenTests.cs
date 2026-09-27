// FieldRefTokenTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// A pointer to a FIELD whose type holds managed references answers an order token, never a raw
/// address, exactly as an element reference of such a type already did (ElemRefBox, ruling 2026-09-22).
/// </summary>
/// <remarks>
/// Before, <c>unsafe.Pointer(&amp;fs.sys)</c> over os's fileStat handed internal/syscall/unix.Fstatat the
/// raw, UNPINNED interior address of a reference-bearing syscall.Stat_t. The kernel wrote 144 bytes of
/// kernel-layout <c>struct stat</c> over CLR memory holding object references: every os.Root Stat/Lstat
/// read size 1000 (the uid) and mode p---------, and the heap was exposed besides. A token is refused
/// by the kernel (EFAULT) and carried by the linux keystone's NativeStructMarshal. The criterion is the
/// field's OWN type, never the container's: a reference-free field inside a reference-bearing root keeps
/// its real address (the windows TCP dial's repair).
/// </remarks>
[TestClass]
public class FieldRefTokenTests
{
    public struct Inner
    {
        public string S;
        public long N;
    }

    public struct Outer
    {
        public long A;
        public Inner F;
        public long B;
    }

    private static readonly FieldRefFunc<Inner> s_f = FieldRef<Outer>.Create<Inner>("F");
    private static readonly FieldRefFunc<long> s_b = FieldRef<Outer>.Create<long>("B");

    [TestMethod]
    public void AReferenceBearingFieldAnswersATokenThatResolvesToItsView()
    {
        builtin.heap(new Outer { F = new Inner { S = "x" } }, out ж<Outer> box);
        ж<Inner> field = box.of(s_f);

        Assert.AreEqual(PointerStorage.None, field.StorageKind);

        uintptr number = field;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken((nuint)number), "a reference-bearing field must never hand out a raw address");
        Assert.AreSame(field, ManagedPointerTokens.Resolve((nuint)number), "the token resolves back to the very view");

        ж<Inner> back = (ж<Inner>)number;
        Assert.AreEqual("x", back.Value.S, "the uintptr -> ж round-trip reaches the same storage");
    }

    [TestMethod]
    public void AReferenceFreeFieldInAReferenceBearingRootKeepsItsAddress()
    {
        builtin.heap(new Outer { F = new Inner { S = "x" } }, out ж<Outer> box);
        ж<long> field = box.of(s_b);

        Assert.AreNotEqual(PointerStorage.None, field.StorageKind);
        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken((nuint)(uintptr)field), "the criterion is the field's type, not the container's");
    }

    [TestMethod]
    public void RepeatedConversionOfOneViewAllocatesNothing()
    {
        builtin.heap(new Outer(), out ж<Outer> box);
        ж<Inner> field = box.of(s_f);
        nuint first = (nuint)(uintptr)field;

        long before = GC.GetAllocatedBytesForCurrentThread();
        nuint sink = 0;

        for (int i = 0; i < 1000; i++)
            sink ^= (nuint)(uintptr)field;

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before, "the token is computed once per view");

        // An even number of XORs of one value cancels: every conversion answered the same number.
        Assert.AreEqual((nuint)0, sink, "a view's number is stable across conversions");
        Assert.AreNotEqual((nuint)0, first);
    }

    // ---- the displacement cache: a FRESH view's first token, keyed by the accessor ----

    // The shape go2cs-gen emits for a struct field and converted code reaches through the typed of().
    public struct Holder
    {
        public long A;
        public Inner F;

        public static ref Inner ᏑF(ref Holder instance) => ref instance.F;
    }

    // Two structs whose same-named field sits at different Go offsets (0 and 8).
    public struct NAtZero
    {
        public long N;
    }

    public struct NAtEight
    {
        public long X;
        public long N;
    }

    // One accessor serving both. Its name resolves (`ᏑN` -> field N), so each source's displacement is
    // that struct's real Go offset of N -- which makes the answer depend on the SOURCE, the one case a
    // cache keyed by the accessor alone would get wrong.
    private static ref long ᏑN(object source)
    {
        if (source is ж<NAtZero> atZero)
            return ref atZero.Value.N;

        return ref ((ж<NAtEight>)source).Value.N;
    }

    private static readonly FieldRefFunc<long> s_n = ᏑN;

    private static nuint Displacement(nuint token) => token & 0xFFFFFFFFu;

    [TestMethod]
    public void AFreshViewsFirstTokenAllocatesNothingOnceItsAccessorIsKnown()
    {
        const int views = 1000;
        ж<Inner>[] fields = new ж<Inner>[views];

        for (int i = 0; i < views; i++)
        {
            builtin.heap(new Holder(), out ж<Holder> box);
            fields[i] = box.of(Holder.ᏑF);
        }

        // The accessor's first view resolves the field's Go offset: a long before it, so 8.
        Assert.AreEqual((nuint)8, Displacement(fields[0].PointerOrderToken), "the token carries the field's real Go offset, not a fallback");

        // The accessor's FIRST cache hit is left outside the window: it allocated 344 B once (measured,
        // Debug, unrooted -- the lookup path's first execution, not a per-view cost), and every hit after
        // it reads 0.
        _ = fields[1].PointerOrderToken;
        long before = GC.GetAllocatedBytesForCurrentThread();
        nuint sink = 0;

        for (int i = 2; i < views; i++)
            sink |= Displacement(fields[i].PointerOrderToken) ^ 8;

        // Before the cache every fresh view re-resolved by name: a Type[] and a sliced name string, 56 B.
        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before, "a known accessor's displacement is read back, not re-resolved");
        Assert.AreEqual((nuint)0, sink, "every fresh view of the field carries the same displacement");
    }

    [TestMethod]
    public void OneAccessorOverTwoSourceTypesKeepsEachSourcesDisplacement()
    {
        builtin.heap(new NAtZero(), out ж<NAtZero> zero);
        builtin.heap(new NAtEight(), out ж<NAtEight> eight);

        Assert.AreEqual((nuint)0, Displacement(zero.of(s_n).PointerOrderToken), "N is NAtZero's first field");
        Assert.AreEqual((nuint)8, Displacement(eight.of(s_n).PointerOrderToken), "the cached entry is for another source type: re-resolve, never reuse");
        builtin.heap(new NAtZero(), out ж<NAtZero> zeroAgain);
        Assert.AreEqual((nuint)0, Displacement(zeroAgain.of(s_n).PointerOrderToken), "and back again, on a fresh view");
    }
}
