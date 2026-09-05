using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using @unsafe = go.unsafe_package;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the flat hash-stub family's hand-own (runtime/hash_impl.cs): <c>memhash</c>,
/// <c>memhash32</c>, <c>memhash64</c> and <c>strhash</c> bodied over Go's own fallback arithmetic
/// (hash64.go) applied to the bytes the managed referent holds. Go's contract, stated as arms: equal
/// inputs hash equal and one byte moves the hash; the SEED parameter changes the hash; the 32- and
/// 64-bit variants agree with the byte form (Go's own TestMemHash32Equality/TestMemHash64Equality);
/// every pointer SHAPE the emitted code mints is recovered to the same bytes; what cannot be
/// recovered — a raw address, a slice/string header read through a reinterpretation golib cannot
/// alias — is refused BY NAME as a panic, never dereferenced (the header class is a native SIGSEGV
/// otherwise); and the hash key is seeded per process. The generated stubs throw
/// NotImplementedException, so every arm goes RED against them (the cut's negative control); the
/// hand-own is flat, so the file compiles for every GoTargetOS.
/// </summary>
[TestClass]
public class RuntimeHashFamilyTests
{
    // The runtime's own slice-header shape (runtime.slice: array unsafe.Pointer; len, cap int),
    // declared here so the bytesHash reinterpretation can be reproduced with public golib API alone.
    private struct SliceHeaderShape
    {
        public @unsafe.Pointer array;
        public nint len;
        public nint cap;
    }

    // The runtime's own string header (runtime.stringStruct: str unsafe.Pointer; len int).
    private struct StringHeaderShape
    {
        public @unsafe.Pointer str;
        public nint len;
    }

    private static byte[] RandomBytes(int length, int seed)
    {
        Random random = new(seed);
        byte[] bytes = new byte[length];
        random.NextBytes(bytes);
        return bytes;
    }

    [TestMethod]
    public void EqualInputsHashEqualAndOneByteMovesTheHash()
    {
        foreach (int length in new[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 16, 17, 47, 48, 49, 96, 97, 100, 1000 })
        {
            byte[] a = RandomBytes(length, 100 + length);
            byte[] b = (byte[])a.Clone();

            Assert.AreEqual(GoMemhash(a, 7), GoMemhash(b, 7), $"length {length}: equal inputs must hash equal");

            if (length == 0)
                continue;

            b[length / 2] ^= 1;
            Assert.AreNotEqual(GoMemhash(a, 7), GoMemhash(b, 7), $"length {length}: one flipped bit must move the hash");
        }
    }

    [TestMethod]
    public void TheSeedParameterChangesTheHash()
    {
        byte[] data = RandomBytes(33, 2);
        ulong[] seeds = [0, 1, 2, 3, 0xdeadbeef, ulong.MaxValue];

        HashSet<ulong> hashes = new(seeds.Select(seed => GoMemhash(data, seed)));

        Assert.AreEqual(seeds.Length, hashes.Count, "distinct seeds must give distinct hashes of the same bytes");

        // A seed that Go's schedule folds through hashkey[0] is still the caller's seed: the same
        // seed twice is the same hash (determinism within the process).
        Assert.AreEqual(GoMemhash(data, 0xdeadbeef), GoMemhash(data, 0xdeadbeef));
    }

    [TestMethod]
    public void The32And64BitVariantsAgreeWithTheByteForm()
    {
        // Go's TestMemHash32Equality / TestMemHash64Equality, over the emitted POINTER shape:
        // MemHash32(unsafe.Pointer(&b), seed) == MemHash(unsafe.Pointer(&b), seed, 4) for b [4]byte.
        Random random = new(1234);
        ulong seed = (ulong)random.NextInt64();

        for (int i = 0; i < 100; i++)
        {
            byte[] four = new byte[4];
            random.NextBytes(four);
            array<byte> arr4 = new(four);
            ref array<byte> a4 = ref heap(arr4, out ж<array<byte>> Ꮡa4);
            @unsafe.Pointer p4 = @unsafe.Pointer.FromPinnedBox(Ꮡa4);

            Assert.AreEqual(GoMemhashPointer(p4, seed, 4), GoMemhash32Pointer(p4, seed), $"memhash32 vs memhash over {Convert.ToHexString(four)}");
            Assert.AreEqual(GoMemhash(four, seed), GoMemhash32(four, seed), "span forms agree too");

            byte[] eight = new byte[8];
            random.NextBytes(eight);
            array<byte> arr8 = new(eight);
            ref array<byte> a8 = ref heap(arr8, out ж<array<byte>> Ꮡa8);
            @unsafe.Pointer p8 = @unsafe.Pointer.FromPinnedBox(Ꮡa8);

            Assert.AreEqual(GoMemhashPointer(p8, seed, 8), GoMemhash64Pointer(p8, seed), $"memhash64 vs memhash over {Convert.ToHexString(eight)}");
            Assert.AreEqual(GoMemhash(eight, seed), GoMemhash64(eight, seed), "span forms agree too");
        }
    }

    [TestMethod]
    public void RecoversEveryEmittedPointerShapeToTheSameBytes()
    {
        const ulong seed = 5;
        byte[] backing = RandomBytes(40, 3);

        // (a) A byte ELEMENT box at an offset — &b[16] of a slice, unsafe.StringData(s) — retained by
        //     the Pointer; the bytes are [16, 24) of the backing, exactly as Go reads them.
        slice<byte> b = new(backing);
        @unsafe.Pointer elem = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 16));
        Assert.AreEqual(GoMemhash(backing.AsSpan(16, 8), seed), GoMemhashPointer(elem, seed, 8), "element box at an offset");

        // (b) A [N]byte box — unsafe.Pointer(&arr) — recovered by the provenance record.
        array<byte> arr = new(backing.AsSpan(0, 12).ToArray());
        ref array<byte> a = ref heap(arr, out ж<array<byte>> Ꮡarr);
        Assert.AreEqual(GoMemhash(backing.AsSpan(0, 12), seed), GoMemhashPointer(@unsafe.Pointer.FromPinnedBox(Ꮡarr), seed, 12), "array box");

        // (c) An unmanaged scalar box through the `(uintptr)` bridge that STRIPS retention (the
        //     emitted int64Hash: `memhash64((uintptr)noescape(FromPinnedBox(Ꮡi)), seed)`): the number
        //     alone resolves back to the box WHILE THE BOX LIVES — the provenance record validates on
        //     read (alive and still pinned there), and the pin lives on the box. Held alive here.
        ulong value = 0x1122334455667788UL;
        ref ulong v = ref heap(value, out ж<ulong> Ꮡv);
        @unsafe.Pointer stripped = (uintptr)@unsafe.Pointer.FromPinnedBox(Ꮡv);
        Assert.IsNull(stripped.RetainedSource, "the bridge strips the retained box — this arm tests the token route");
        Assert.AreEqual(GoMemhash(BitConverter.GetBytes(value), seed), GoMemhash64Pointer(stripped, seed), "scalar box through the token route");
        System.GC.KeepAlive(Ꮡv);

        // (d) A string box's CONTENT for strhash.
        @string str = "hello, world"u8;
        ref @string s = ref heap(str, out ж<@string> Ꮡs);
        Assert.AreEqual(GoMemhash("hello, world"u8, seed), GoStrhashPointer(@unsafe.Pointer.FromPinnedBox(Ꮡs), seed), "string content");

        // (f) The emitted bytesHash shape end to end — `(*slice)(unsafe.Pointer(&b))` over a SUBSLICE, its
        //     `array` word handed to memhash with its `len`: served by SliceHeaderBox since increment 3 (the
        //     pointer retains the subslice's element-0 box), so the hash is the span form's.
        slice<byte> sub = new slice<byte>(backing)[16..24];
        ref slice<byte> sbox = ref heap(sub, out ж<slice<byte>> Ꮡsub);
        ж<SliceHeaderShape> header = Ꮡsub.Reinterpret<slice<byte>, SliceHeaderShape>();
        Assert.AreEqual(GoMemhash(backing.AsSpan(16, 8), seed), GoMemhashPointer((~header).array, seed, (ulong)(~header).len), "the bytesHash route through the slice header");

        // (e) The empty case: memhash(nil, seed, 0) is seed ^ hashkey[0], as Go's s == 0 arm says.
        Assert.AreEqual(GoMemhash(ReadOnlySpan<byte>.Empty, seed), GoMemhashPointer(new @unsafe.Pointer(nil), seed, 0), "nil pointer, zero size");
    }

    [TestMethod]
    public void RefusesWhatItCannotRecoverByName()
    {
        // A raw address nothing minted: no referent, so a panic naming memhash — never a number.
        PanicException raw = Assert.ThrowsException<PanicException>(() => GoMemhashPointer(new @unsafe.Pointer((nuint)0x1000), 0, 8));
        StringAssert.Contains(raw.Message, "memhash");
        StringAssert.Contains(raw.Message, "no recoverable managed referent");

        // Reading past an element box's backing.
        slice<byte> b = new(RandomBytes(40, 4));
        PanicException past = Assert.ThrowsException<PanicException>(() => GoMemhashPointer(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 36)), 0, 8));
        StringAssert.Contains(past.Message, "past its end");

        // The HEADER class as rule (0) met the STRING header here until Q44. (The SLICE header was this
        // witness until increment 3's SliceHeaderBox began serving it — its case now lives in the recovery
        // arm above as a positive property.) Before the token, the reinterpret of a @string box onto the
        // runtime's stringStruct shape took the address route: a NativeBox over the PINNED managed string
        // whose `str` field read back the byte[] reference as a Pointer (measured 2026-09-04: runtime type
        // System.Byte[], a field read through it a native SIGSEGV), which the seam refused by name. That
        // route no longer exists to be refused — the witness moved with Q44 (2026-09-05), as it said it
        // would: a reference-bearing box hands out its ORDER TOKEN, the reinterpret to another pointee
        // type is a native box OVER THE TOKEN (the design's loud form; its fields are not touched here — a
        // dereference is the row-level fault the design chose, never a number), and the STRING HALF of
        // the seam is admitted through the token: the string box's own number resolves to the box, memhash
        // over it is refused as a HEADER by name, strhash over it hashes the content.
        @string text = "hello, header"u8;
        ref @string sv = ref heap(text, out ж<@string> Ꮡtext);
        ж<StringHeaderShape> stringHeader = Ꮡtext.Reinterpret<@string, StringHeaderShape>();
        Assert.IsTrue(stringHeader.IsNative, "the string-header reinterpret is the loud form: a native box over the token, never a pun through the pinned string");
        Assert.AreEqual(Ꮡtext.PointerOrderToken, stringHeader.NativeAddress, "whose address IS the string box's order token");

        @unsafe.Pointer stringBox = new @unsafe.Pointer((uintptr)Ꮡtext);
        PanicException headerPanic = Assert.ThrowsException<PanicException>(() => GoMemhashPointer(stringBox, 0, 8));
        StringAssert.Contains(headerPanic.Message, "string HEADER");
        Assert.AreEqual(GoMemhash("hello, header"u8, 0), GoStrhashPointer(stringBox, 0), "and strhash over the same number hashes the string's CONTENT -- the string half of the seam, admitted by the token");

        ref slice<byte> sb = ref heap(b, out ж<slice<byte>> Ꮡb);

        // A slice HEADER box itself (unsafe.Pointer(&b) of a slice): refused, not hashed as garbage.
        PanicException headerBox = Assert.ThrowsException<PanicException>(() => GoMemhashPointer(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, 8));
        StringAssert.Contains(headerBox.Message, "memhash");

        // strhash over something that is not a string box.
        ulong value = 1;
        ref ulong v = ref heap(value, out ж<ulong> Ꮡv);
        PanicException notString = Assert.ThrowsException<PanicException>(() => GoStrhashPointer(@unsafe.Pointer.FromPinnedBox(Ꮡv), 0));
        StringAssert.Contains(notString.Message, "not a string box");
    }

    // The emitted int32Hash/int64Hash mint the box in their OWN frame and hand memhash32/64 only the
    // number (`(uintptr)noescape(FromPinnedBox(Ꮡi))`), so once FromPinnedBox returns the box has no
    // reference left and a collection between the mint and the callee's resolve retires the pin and
    // the provenance entry with it. Measured on runtime's TestSmhasherWindowed (2026-09-05): two
    // million Int32Hash calls, and the first one interrupted by a GC panicked by name. This arm makes
    // that deterministic — and pins what the body owes in that state: a REFUSAL naming memhash64,
    // never a number over whatever the address now holds. The fix is upstream (retention through the
    // bridge, Q44's neighbourhood), not here.
    [TestMethod]
    public void AStrippedPointerWhoseBoxDiedIsRefusedByNameNeverHashed()
    {
        @unsafe.Pointer stripped = MintAndDrop(0x0123456789abcdefUL);
        Assert.IsNull(stripped.RetainedSource);

        // One collection: the provenance record holds a SHORT weak reference, cleared the moment the
        // box is only finalizer-reachable, so no finalizer drain is needed (and a process-wide drain
        // runs every Go finalizer inline on the finalizer thread, a documented hazard).
        System.GC.Collect();

        PanicException dead = Assert.ThrowsException<PanicException>(() => GoMemhash64Pointer(stripped, 5));
        StringAssert.Contains(dead.Message, "memhash64");
        StringAssert.Contains(dead.Message, "no recoverable managed referent");
    }

    // A separate, non-inlined frame so the box is unreachable the moment it returns.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static @unsafe.Pointer MintAndDrop(ulong value)
    {
        ref ulong v = ref heap(value, out ж<ulong> Ꮡv);
        return (uintptr)@unsafe.Pointer.FromPinnedBox(Ꮡv);
    }

    [TestMethod]
    public void TheHashKeyIsSeededPerProcessAndStable()
    {
        ulong[] key = GoHashKey();

        Assert.AreEqual(4, key.Length);
        Assert.IsTrue(key.Any(word => word != 0), "alginit's non-AES branch must have seeded hashkey — four zero words would be the unseeded master state");
        CollectionAssert.AreEqual(key, GoHashKey(), "the key is fixed for the life of the process");
    }
}
