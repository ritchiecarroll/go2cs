using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// The interior unsafe.Pointer root (docs/phase4/DESIGN-interior-pointer-root.md, sections 4.1 and 10.1): a typed
// pointer converted from an unsafe.Pointer VALUE that holds an interior address of Go memory, with no cgo anywhere.
//
// Every conversion in this file goes through ONE helper, Converted<T>, which spells what the converter emits for
// `(*T)(p)`. Each row then asks two things after a collection that COMPACTS:
//
//   the ADDRESS: the typed pointer names the storage where it is NOW (a native pointer whose storage moved reads and
//   writes somebody else's bytes, and a value assertion alone can read green over them);
//
//   the VALUE: a write through the typed pointer is read back through the Go variable.
//
// The collection is the recipe of GoCgoArgsTests.Review2 (CollectAndCompact), copied verbatim because that class
// keeps it private. A row whose storage did not move reports Inconclusive: it cannot tell the two cases apart.
//
// The C-memory rows at the end must pass before and after the fix. A refused or re-homed C-memory twin is the
// design's stop line S1.
[TestClass]
public unsafe class InteriorPointerRootTests
{
    // struct{ i1, i2 int64 }: reference-free, so its box has storage with an address.
    private struct Pair
    {
        internal long i1;
        internal long i2;
    }

    // struct{ first, second [4]Pair }: the array fields make it reference-bearing in the managed layout (array<T>
    // holds a T[]), so its pointer answers an order token, never an address. Go offset 24 is first[1].i2.
    private struct TwoArrays
    {
        public TwoArrays() { }

        internal array<Pair> first = new(4);
        internal array<Pair> second = new(4);
    }

    // A Go struct viewed over a byte array: pad at 0, n at 4.
    private struct viewHdr
    {
        public int32 Pad;
        public int32 N;

        // Converter-named, so a field view can read the Go field name off the accessor.
        internal static ref int32 ᏑN(ref viewHdr instance) => ref instance.N;
    }

    // The runtime's notInHeapSlice shape over int32 and over byte: a typed array pointer first, then len and cap.
    private struct Int32Header
    {
        public ж<int32> array;
        public nint len;
        public nint cap;
    }

    private struct ByteHeader
    {
        public ж<byte> array;
        public nint len;
        public nint cap;
    }

    // `(*T)(p)` for an unsafe.Pointer value p, as the converter emits it (design 4.1). Until the typed door this line
    // read `(ж<T>)(uintptr)(p)`, and the cast through uintptr dropped whatever the pointer carried: every red row of
    // this file was red through that one spelling.
    private static ж<T> Converted<T>(@unsafe.Pointer p) => @unsafe.Pointer.To<T>(p);

    // GoCgoArgsTests.Review2's recipe: the generations collected and finalized three times, then a blocking, compacting
    // gen2 collection with the large object heap compacted too, then the freed space reused.
    private static void CollectAndCompact()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);

        var reuse = new List<byte[]>();

        for (int i = 0; i < 256; i++)
        {
            byte[] fill = new byte[64];
            Array.Fill(fill, (byte)0xEE);
            reuse.Add(fill);
        }

        GC.KeepAlive(reuse);
    }

    // Where a box's storage is at this instant. A read, not a pin: the answer is stale after the next collection.
    private static nuint SlotAddress<T>(ж<T> box) => (nuint)Unsafe.AsPointer(ref box.ValueSlot);

    private static nuint DataAddress(byte[] storage) => (nuint)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(storage));

    // Two boxes allocated behind garbage that dies on return, so a compacting collection has room to slide them into.
    // The control is never converted to a pointer of any kind: nothing pins it, so it shows whether the heap moved.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<Pair> subject, ж<Pair> control) SubjectAndControlBehindGarbage()
    {
        var garbage = new List<byte[]>();

        for (int i = 0; i < 2048; i++)
            garbage.Add(new byte[512]);

        ж<Pair> subject = new StandardBox<Pair>(new Pair { i1 = 1, i2 = 2 });
        ж<Pair> control = new StandardBox<Pair>(new Pair { i1 = 3, i2 = 4 });

        GC.KeepAlive(garbage);

        return (subject, control);
    }

    private static void AssertNamesItsStorageNow<T>(ж<T> typed, nuint expected, string what)
    {
        // A managed alias has no number to be stale; the write below is its whole check.
        if (typed.NativeAddress != 0)
            Assert.AreEqual(expected, typed.NativeAddress, what + ": the typed pointer's address is not where the storage is now");
    }

    // ---- Review must-fix 1: unsafe.Pointer.FromBox reads its number under `fixed` for one statement and pins nothing.

    private enum CollectionAt
    {
        BetweenTheMintAndTheAdd,
        BetweenTheAddAndTheConversion,
        BetweenTheConversionAndTheWrite
    }

    // p := unsafe.Pointer(q) for a pointer VALUE q (FromBox); t := (*int64)(unsafe.Add(p, 8)); *t = 77. The box is held
    // by the frame the whole time: this is a rooted, MOVABLE base, the review's shape 8 with the root alive.
    private static void AFromBoxDerivedPointerNamesItsStorageAcrossACompactingCollection(CollectionAt at)
    {
        (ж<Pair> box, ж<Pair> control) = SubjectAndControlBehindGarbage();
        nuint controlBefore = SlotAddress(control);

        @unsafe.Pointer p = @unsafe.Pointer.FromBox(box);

        if (at == CollectionAt.BetweenTheMintAndTheAdd)
            CollectAndCompact();

        @unsafe.Pointer q = @unsafe.Add(p, 8);

        if (at == CollectionAt.BetweenTheAddAndTheConversion)
            CollectAndCompact();

        ж<long> typed = Converted<long>(q);

        if (at == CollectionAt.BetweenTheConversionAndTheWrite)
            CollectAndCompact();

        if (SlotAddress(control) == controlBefore)
            Assert.Inconclusive("the control box beside the subject did not move, so this run cannot tell a pointer that follows its storage from one that does not");

        // The address FIRST: a write through a stale number lands in memory this test does not own.
        AssertNamesItsStorageNow(typed, SlotAddress(box) + 8, "(*int64)(unsafe.Add(unsafe.Pointer(q), 8)), collection " + at);

        typed.Value = 77;
        Assert.AreEqual(77L, box.Value.i2, "a write through the typed pointer reaches the field it was derived from");
        Assert.AreEqual(1L, box.Value.i1, "and leaves its neighbour alone");

        GC.KeepAlive(box);
        GC.KeepAlive(control);
        GC.KeepAlive(typed);
    }

    [TestMethod]
    public void AFromBoxDerivedPointerNamesItsStorageWhenTheHeapCompactsBeforeTheAdd()
    {
        AFromBoxDerivedPointerNamesItsStorageAcrossACompactingCollection(CollectionAt.BetweenTheMintAndTheAdd);
    }

    [TestMethod]
    public void AFromBoxDerivedPointerNamesItsStorageWhenTheHeapCompactsBeforeTheConversion()
    {
        AFromBoxDerivedPointerNamesItsStorageAcrossACompactingCollection(CollectionAt.BetweenTheAddAndTheConversion);
    }

    [TestMethod]
    public void AFromBoxDerivedPointerNamesItsStorageWhenTheHeapCompactsBeforeTheWrite()
    {
        AFromBoxDerivedPointerNamesItsStorageAcrossACompactingCollection(CollectionAt.BetweenTheConversionAndTheWrite);
    }

    // Offset 0 of the same mint: (*Pair)(unsafe.Pointer(q)) after the heap compacted names the box q names.
    [TestMethod]
    public void AFromBoxPointerConvertedAtOffsetZeroNamesItsStorageAfterACompactingCollection()
    {
        (ж<Pair> box, ж<Pair> control) = SubjectAndControlBehindGarbage();
        nuint controlBefore = SlotAddress(control);

        @unsafe.Pointer p = @unsafe.Pointer.FromBox(box);

        CollectAndCompact();

        ж<Pair> typed = Converted<Pair>(p);

        if (SlotAddress(control) == controlBefore)
            Assert.Inconclusive("the control box beside the subject did not move, so this run cannot tell a pointer that follows its storage from one that does not");

        AssertNamesItsStorageNow(typed, SlotAddress(box), "(*Pair)(unsafe.Pointer(q)) after a compacting collection");

        typed.Value = new Pair { i1 = 5, i2 = 6 };
        Assert.AreEqual(5L, box.Value.i1);
        Assert.AreEqual(6L, box.Value.i2);

        GC.KeepAlive(box);
        GC.KeepAlive(control);
    }

    // The twin over an address TAKEN (FromPinnedBox): the mint pins, so the storage must not move at all while the box
    // is held, and the typed pointer names it. Passed before the typed door too; it is here so the base rule cannot
    // un-pin or re-home what was already right.
    [TestMethod]
    public void AFromPinnedBoxDerivedPointerNamesItsStorageAcrossACompactingCollection()
    {
        (ж<Pair> box, ж<Pair> control) = SubjectAndControlBehindGarbage();
        nuint controlBefore = SlotAddress(control);

        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(box);
        nuint pinnedAt = SlotAddress(box);

        CollectAndCompact();

        @unsafe.Pointer q = @unsafe.Add(p, 8);

        CollectAndCompact();

        ж<long> typed = Converted<long>(q);

        if (SlotAddress(control) == controlBefore)
            Assert.Inconclusive("the control box beside the subject did not move, so this run cannot tell a pinned box from a heap that stood still");

        Assert.AreEqual(pinnedAt, SlotAddress(box), "storage whose address was taken moved while its box was held");
        AssertNamesItsStorageNow(typed, pinnedAt + 8, "(*int64)(unsafe.Add(unsafe.Pointer(&v), 8))");
        Assert.AreNotEqual((nuint)0, typed.NativeAddress, "a field of pinned storage at a different type is an address, as before");

        typed.Value = 77;
        Assert.AreEqual(77L, box.Value.i2);
        Assert.AreSame(box, Converted<Pair>(p), "offset 0 at the box's own type is the box, as the registry answered before");

        GC.KeepAlive(box);
        GC.KeepAlive(control);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<long> typed, WeakReference root) TypedFromAFromBoxPointerOfADroppedBox()
    {
        ж<Pair> box = new StandardBox<Pair>(new Pair { i1 = 1, i2 = 9 });
        @unsafe.Pointer p = @unsafe.Add(@unsafe.Pointer.FromBox(box), 8);

        return (Converted<long>(p), new WeakReference(box));
    }

    // The same derivation with the box dropped: the typed pointer is the only thing left that names the storage, so it
    // must keep the box, and the box's storage must be held still for as long as the pointer carries a number.
    [TestMethod]
    public void AFromBoxDerivedPointerKeepsItsBoxAlive()
    {
        (ж<long> typed, WeakReference root) = TypedFromAFromBoxPointerOfADroppedBox();

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the box a typed pointer was derived from is collected while the pointer is alive");

        AssertNamesItsStorageNow(typed, SlotAddress((ж<Pair>)root.Target!) + 8, "(*int64)(unsafe.Add(unsafe.Pointer(q), 8)) with q dropped");
        Assert.AreEqual(9L, typed.Value, "the typed pointer still reads the field it was derived from");
        GC.KeepAlive(typed);
    }

    // ---- A root that holds a reference: its number is an order token, and token arithmetic goes by the alias walk.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (@unsafe.Pointer moved, WeakReference root) AnInteriorPointerOfADroppedReferenceBearingBox()
    {
        ж<TwoArrays> box = new StandardBox<TwoArrays>(new TwoArrays());
        box.Value.first[1] = new Pair { i1 = 10, i2 = 11 };

        // unsafe.Add(unsafe.Pointer(&t), 24): &t.first[1].i2
        return (@unsafe.Add(@unsafe.Pointer.FromPinnedBox(box), 24), new WeakReference(box));
    }

    // The unsafe.Pointer VALUE is all that is left of the struct. Go keeps the struct alive through it; the conversion
    // after a collection must answer the field, by name, and not a refusal for a base that is gone.
    [TestMethod]
    public void AnInteriorPointerOfAReferenceBearingBoxKeepsItAndConvertsToTheField()
    {
        (@unsafe.Pointer moved, WeakReference root) = AnInteriorPointerOfADroppedReferenceBearingBox();

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the struct an unsafe.Pointer was derived from is collected while the pointer is alive");

        ж<long> typed = Converted<long>(moved);
        Assert.AreEqual(11L, typed.Value, "(*int64)(unsafe.Add(unsafe.Pointer(&t), 24)) reads t.first[1].i2");

        typed.Value = 12;
        Assert.AreEqual(12L, ((ж<TwoArrays>)root.Target!).Value.first[1].i2, "and a write through it reaches the field");
        GC.KeepAlive(moved);
    }

    // ---- The slice header whose first field is a typed pointer (HeaderSliceBox, the rebase at its native arm).

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<int32> rebased, WeakReference root) HeaderRebasedOverAFieldOfADroppedView(byte[] storage)
    {
        ж<viewHdr> view = new ElemRefBox<byte>(new array<byte>(storage), 0).Reinterpret<byte, viewHdr>();

        // hdr := notInHeapSlice{array: &h.n, len: 2, cap: 2}; s := *(*[]int32)(unsafe.Pointer(&hdr))
        Int32Header header = new() { array = view.of(viewHdr.ᏑN), len = 2, cap = 2 };
        ref Int32Header sl = ref heap(header, out ж<Int32Header> Ꮡsl);

        return (~Ꮡsl.Reinterpret<Int32Header, slice<int32>>(), new WeakReference(view));
    }

    // The field of a view keeps the view and its pin (GoCgoArgsTests.AFieldOfAViewKeepsTheViewAndItsPinAlive). A slice
    // re-based over that field through a pointer-first header must keep them too: the slice is all that is left.
    [TestMethod]
    public void ASliceRebasedThroughAPointerFirstHeaderOverAViewKeepsTheViewAlive()
    {
        Assert.IsTrue(HeaderSliceBox<Int32Header, slice<int32>>.Applies, "the pointer-first header over slice<int32> is the box's shape");

        byte[] storage = new byte[32];
        storage[4] = 7;
        (slice<int32> rebased, WeakReference root) = HeaderRebasedOverAFieldOfADroppedView(storage);

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the view a slice header was re-based over is collected while the slice is alive, and its pin with it");
        Assert.AreEqual(DataAddress(storage) + 4, (nuint)(uintptr)Ꮡ(rebased, 0), "the slice's first element is not where the storage is now");
        Assert.AreEqual(7, (int)rebased[0], "the slice still reads the storage it was re-based over");
        GC.KeepAlive(rebased);
        GC.KeepAlive(storage);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<byte> rebased, WeakReference root) HeaderRebasedOverATypedPointerOfADroppedBox(byte[] storage)
    {
        ж<byte> first = new ElemRefBox<byte>(new array<byte>(storage), 0);

        // p := (*byte)(unsafe.Add(unsafe.Pointer(&b[0]), 4)); hdr := notInHeapSlice{array: p, len: 4, cap: 8}
        ByteHeader header = new() { array = Converted<byte>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4)), len = 4, cap = 8 };
        ref ByteHeader sl = ref heap(header, out ж<ByteHeader> Ꮡsl);

        return (~Ꮡsl.Reinterpret<ByteHeader, slice<byte>>(), new WeakReference(first));
    }

    // The same header over a typed pointer that came from an interior unsafe.Pointer of a Go array, with no view at
    // all. Red for two reasons before the fix (the conversion drops the box, and the rebase would drop it again).
    [TestMethod]
    public void ASliceRebasedThroughAPointerFirstHeaderOverAConvertedPointerKeepsItsBoxAlive()
    {
        byte[] storage = new byte[32];
        storage[4] = 9;
        (slice<byte> rebased, WeakReference root) = HeaderRebasedOverATypedPointerOfADroppedBox(storage);

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the box a slice header was re-based over is collected while the slice is alive, and its pin with it");
        Assert.AreEqual(DataAddress(storage) + 4, (nuint)(uintptr)Ꮡ(rebased, 0), "the slice's first element is not where the storage is now");
        Assert.AreEqual((byte)9, rebased[0], "the slice still reads the storage it was re-based over");
        GC.KeepAlive(rebased);
        GC.KeepAlive(storage);
    }

    // ---- unsafe.Add over a nil pointer (COORD's ruling of 2026-11-14): Go answers the offset.

    [TestMethod]
    public void AddOverANilPointerAnswersTheOffset()
    {
        @unsafe.Pointer nilPointer = (@unsafe.Pointer)(uintptr)0;

        // unsafe.Add(unsafe.Pointer(nil), 16), and unsafe.Pointer(uintptr(nil) + 16) emitted as the same Add.
        Assert.AreEqual((nuint)16, (nuint)(uintptr)@unsafe.Add(nilPointer, 16), "unsafe.Add(nil, 16) is 16, as in Go");
        Assert.AreEqual((nuint)16, (nuint)(uintptr)@unsafe.Add(nilPointer, (uintptr)16), "the uintptr overload answers the same");
        Assert.AreEqual((nuint)0, (nuint)(uintptr)@unsafe.Add(nilPointer, 0), "and offset 0 over nil stays nil");
        Assert.IsTrue(@unsafe.Add(nilPointer, 0) == nil, "unsafe.Add(nil, 0) == nil");
    }

    // ---- Equality: two pointers at one address are one Go pointer, however each of them was derived.
    //
    // A pointer moved by unsafe.Add carries a native box at the new address (it keeps what the pointer carried), and a
    // pointer made by unsafe.Pointer(&x) carries x's own box. Compared by what they carry, the two were different
    // pointers: `unsafe.Add(p, 0) == p` and `unsafe.Pointer(uintptr(p) + 0) == p` answered false where Go answers
    // true (the behavioral program UnsafePointerKeywordParam, once the arithmetic form is emitted as unsafe.Add).

    [TestMethod]
    public void AMovedPointerEqualsAPointerAtTheSameAddress()
    {
        ж<Pair> box = new StandardBox<Pair>(new Pair { i1 = 1, i2 = 2 });
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(box);

        Assert.IsTrue(@unsafe.Add(p, 0) == p, "unsafe.Add(p, 0) == p");
        Assert.IsTrue(p == @unsafe.Add(p, 0), "p == unsafe.Add(p, 0)");
        Assert.IsTrue(@unsafe.Add(@unsafe.Add(p, 8), -8) == p, "there and back is the same pointer");
        Assert.IsFalse(@unsafe.Add(p, 8) == p, "a pointer 8 bytes on is another pointer");

        // A FromBox pointer holds a number read before anything held the storage still.
        ж<Pair> second = new StandardBox<Pair>(new Pair { i1 = 3, i2 = 4 });
        @unsafe.Pointer loose = @unsafe.Pointer.FromBox(second);

        Assert.IsTrue(@unsafe.Add(loose, 0) == loose, "unsafe.Add(p, 0) == p for a pointer whose box was not pinned when it was made");
        Assert.IsFalse(@unsafe.Add(loose, 0) == p, "two variables are two pointers");

        // &a[1] reached two ways: by index, and by arithmetic from &a[0].
        ж<array<long>> elements = new StandardBox<array<long>>(new array<long>(4));
        @unsafe.Pointer first = @unsafe.Pointer.FromPinnedBox(elements.at<long>(0));
        @unsafe.Pointer next = @unsafe.Pointer.FromPinnedBox(elements.at<long>(1));

        Assert.IsTrue(@unsafe.Add(first, 8) == next, "unsafe.Add(unsafe.Pointer(&a[0]), 8) == unsafe.Pointer(&a[1])");
        Assert.IsTrue(next == @unsafe.Add(first, 8), "and the other way round");
        Assert.IsFalse(@unsafe.Add(first, 16) == next, "&a[2] is not &a[1]");

        // A root that holds a reference has no address: its arithmetic is token arithmetic, and offset 0 is the root.
        ж<TwoArrays> bearing = new StandardBox<TwoArrays>(new TwoArrays());
        @unsafe.Pointer t = @unsafe.Pointer.FromPinnedBox(bearing);

        Assert.IsTrue(@unsafe.Add(t, 0) == t, "unsafe.Add(p, 0) == p over a struct that holds references");

        GC.KeepAlive(box);
        GC.KeepAlive(second);
        GC.KeepAlive(elements);
        GC.KeepAlive(bearing);
    }

    // ---- C memory: the same derivations over memory Go does not own. These pass before and after the fix.

    [TestMethod]
    public void TheSameDerivationsOverCMemoryAnswerTheAddress()
    {
        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            for (int i = 0; i < 64; i++)
                Marshal.WriteByte(buffer, i, 0);

            nuint address = (nuint)(nint)buffer;

            // p := unsafe.Pointer(uintptr(c)); (*int64)(unsafe.Add(p, 8))
            ж<long> fromNumber = Converted<long>(@unsafe.Add((@unsafe.Pointer)(uintptr)address, 8));
            Assert.AreEqual(address + 8, fromNumber.NativeAddress, "a typed pointer derived from a C address is that address plus the offset");

            fromNumber.Value = 0x1122334455667788;
            Assert.AreEqual(0x1122334455667788, Marshal.ReadInt64(buffer, 8), "and a write through it reaches C memory");

            // q := (*byte)(c); p := unsafe.Pointer(q) (FromBox over a native box); (*int64)(unsafe.Add(p, 16))
            ж<byte> q = (void*)buffer;
            ж<long> fromBox = Converted<long>(@unsafe.Add(@unsafe.Pointer.FromBox(q), 16));
            Assert.AreEqual(address + 16, fromBox.NativeAddress, "the same through a typed pointer to C memory");

            fromBox.Value = 42;
            Assert.AreEqual(42L, Marshal.ReadInt64(buffer, 16));

            // Offset 0: (*int64)(unsafe.Pointer(q)) is C memory at q's address.
            ж<long> atZero = Converted<long>(@unsafe.Pointer.FromBox(q));
            Assert.AreEqual(address, atZero.NativeAddress, "offset 0 of a typed pointer to C memory is its address");

            CollectAndCompact();

            Assert.AreEqual(address + 8, fromNumber.NativeAddress, "C memory does not move");
            Assert.AreEqual(0x1122334455667788, fromNumber.Value);
            Assert.AreEqual(42L, fromBox.Value);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // A pointer-first header over C memory still re-bases as it does today (HeaderSliceReinterpretTests holds the
    // detail); here it is the twin of the two Go-memory rows above, across the same collection.
    [TestMethod]
    public void ASliceRebasedThroughAPointerFirstHeaderOverCMemoryStillReadsIt()
    {
        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            for (int i = 0; i < 64; i++)
                Marshal.WriteByte(buffer, i, (byte)i);

            ж<byte> array = (void*)buffer;
            ByteHeader header = new() { array = array, len = 4, cap = 16 };
            ref ByteHeader sl = ref heap(header, out ж<ByteHeader> Ꮡsl);
            slice<byte> s = ~Ꮡsl.Reinterpret<ByteHeader, slice<byte>>();

            CollectAndCompact();

            Assert.IsTrue(s.IsNativeBacked, "re-based over the address, not copied");
            Assert.AreEqual((nuint)(nint)buffer, (nuint)(uintptr)Ꮡ(s, 0), "the slice's first element is the C address");
            Assert.AreEqual((byte)2, s[2]);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
