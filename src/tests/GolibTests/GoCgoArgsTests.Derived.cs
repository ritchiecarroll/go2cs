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

// cgo Phase 3 increment 3.1, the review's findings (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16.11): a box DERIVED
// from a pointer over Go memory. The first arms of the doors enumerated the boxes golib mints first. These enumerate
// what golib mints FROM one: a field, a second reinterpret, an element or SliceData of an unsafe.Slice, and
// unsafe.Add. Each is built as the converter emits it, handed to every door it can reach, and must be refused. Beside
// each, the same derivation over C memory still crosses, so a refusal is the classification and not the door.
public unsafe partial class GoCgoArgsTests
{
    private struct cgoHdr
    {
        public int32 Pad;
        public int32 N;
        public cgoPoint P;

        internal static ref int32 ᏑN(ref cgoHdr instance) => ref instance.N;
        internal static ref cgoPoint ᏑP(ref cgoHdr instance) => ref instance.P;
    }

    // The doors a typed pointer can reach, by the mints the converter emits for it. Each door that lets it through is
    // named.
    private static List<string> DoorsCrossedBy<T>(ж<T> box)
    {
        var crossed = new List<string>();

        void Try(string door, Action call)
        {
            try
            {
                call();
                crossed.Add(door);
            }
            catch (PanicException)
            {
            }
        }

        Try("StructPointer", () => GoCgoArgs.StructPointer(box, "f", out _));
        Try("HelperPointer", () => GoCgoArgs.HelperPointer(box, "GoString"));
        Try("VoidPointer of the pinning mint", () => GoCgoArgs.VoidPointer(@unsafe.Pointer.FromPinnedBox(box), "f"));
        Try("VoidPointer of the retaining mint", () => GoCgoArgs.VoidPointer(@unsafe.Pointer.FromBox(box), "f"));
        Try("HelperVoidPointer of the pinning mint", () => GoCgoArgs.HelperVoidPointer(@unsafe.Pointer.FromPinnedBox(box), "GoBytes"));

        return crossed;
    }

    private static List<string> DoorsCrossedBy(@unsafe.Pointer pointer)
    {
        var crossed = new List<string>();

        void Try(string door, Action call)
        {
            try
            {
                call();
                crossed.Add(door);
            }
            catch (PanicException)
            {
            }
        }

        Try("VoidPointer", () => GoCgoArgs.VoidPointer(pointer, "f"));
        Try("HelperVoidPointer", () => GoCgoArgs.HelperVoidPointer(pointer, "GoBytes"));

        return crossed;
    }

    private const int TypedDoors = 5, UntypedDoors = 2;

    // A user function that returns the pointer it was handed: `func id(p unsafe.Pointer) unsafe.Pointer { return p }`.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static @unsafe.Pointer Returned(@unsafe.Pointer pointer) => pointer;

    [TestMethod]
    public void ABoxDerivedFromAPointerOverGoMemoryIsRefusedAtEveryDoor()
    {
        array<byte> bytes = new array<byte>(32);
        ж<byte> first = new ElemRefBox<byte>(bytes, 0);
        ж<array<byte>> goArray = new StandardBox<array<byte>>(new array<byte>(32));
        ж<int64> local = new StandardBox<int64>(7);

        // h := (*hdr)(unsafe.Pointer(&b[0])): a view pinned over a slice's element.
        ж<cgoHdr> view = first.Reinterpret<byte, cgoHdr>();
        // h2 := (*hdr)(unsafe.Pointer(&array)): through the Go array's number.
        ж<cgoHdr> overArray = (ж<cgoHdr>)(uintptr)goArray;
        // sl := unsafe.Slice((*int32)(unsafe.Pointer(&b[0])), 4)
        slice<int32> sliced = @unsafe.Slice(first.Reinterpret<byte, int32>(), 4);

        var crossings = new List<string>();

        void Typed<T>(string form, ж<T> box)
        {
            foreach (string door in DoorsCrossedBy(box))
                crossings.Add(form + ": " + door);
        }

        void Untyped(string form, @unsafe.Pointer pointer)
        {
            foreach (string door in DoorsCrossedBy(pointer))
                crossings.Add(form + ": " + door);
        }

        // Finding 1: a field of a view.
        Typed("&h.n, a field of a view over a slice", view.of(cgoHdr.ᏑN));
        Typed("&h.p, a struct field of a view over a slice", view.of(cgoHdr.ᏑP));
        Typed("&h2.n, a field of a view over a Go array", overArray.of(cgoHdr.ᏑN));
        // Finding 2: a second reinterpret, of the view and of its field.
        Typed("(*C.char)(unsafe.Pointer(h)), a second reinterpret", view.Reinterpret<cgoHdr, sbyte>());
        Typed("(*C.char)(unsafe.Pointer(&h.n)), a reinterpret of a field", view.of(cgoHdr.ᏑN).Reinterpret<int32, sbyte>());
        Typed("(*C.char)(unsafe.Pointer(h2)), a reinterpret of a view over a Go array", overArray.Reinterpret<cgoHdr, sbyte>());
        // Finding 3: unsafe.Slice over a view.
        Typed("&sl[1], an element of unsafe.Slice over a view", Ꮡ(sliced, 1));
        Typed("unsafe.SliceData(sl) over a view", @unsafe.SliceData(sliced));
        Typed("&sl[1:][1], an element of a re-slice", Ꮡ(sliced[1..], 1));
        // Finding 4: unsafe.Add, typed and untyped.
        Typed("unsafe.Add over a typed view", @unsafe.Add(view, 4));
        Untyped("unsafe.Add(unsafe.Pointer(&b[0]), 4)", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4));
        Untyped("unsafe.Add(unsafe.Pointer(&array), 4)", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(goArray), 4));
        Untyped("unsafe.Add(unsafe.Pointer(&local), 4)", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(local), 4));
        Untyped("unsafe.Add(unsafe.Pointer(h), 4) over a view", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(view), 4));

        // A CALL RESULT, as the converter emits one: the call itself, with no cast between it and its consumer
        // (docs/phase4/DESIGN-unsafe-pointer-call-result.md). The four arms above hand unsafe.Add's result to the door
        // as the argument. These take it through a variable and through a user function, for unsafe.Add and for a
        // derived pointer.
        @unsafe.Pointer added = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4);
        Untyped("p := unsafe.Add(unsafe.Pointer(&b[0]), 4), through a variable", added);
        Untyped("id(unsafe.Add(unsafe.Pointer(&b[0]), 4)), through a user function", Returned(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4)));
        @unsafe.Pointer field = @unsafe.Pointer.FromPinnedBox(view.of(cgoHdr.ᏑN));
        Untyped("p := unsafe.Pointer(&h.n), a derived pointer through a variable", field);
        Untyped("id(unsafe.Pointer(&h.n)), a derived pointer through a user function", Returned(@unsafe.Pointer.FromPinnedBox(view.of(cgoHdr.ᏑN))));
        Untyped("id(unsafe.Pointer(&sl[1])), an element of unsafe.Slice through a user function", Returned(@unsafe.Pointer.FromPinnedBox(Ꮡ(sliced, 1))));

        Assert.AreEqual(0, crossings.Count, "derived pointers over Go memory that crossed as C memory:\n" + string.Join("\n", crossings));

        GC.KeepAlive(first);
        GC.KeepAlive(goArray);
        GC.KeepAlive(local);
        GC.KeepAlive(view);
        GC.KeepAlive(overArray);
    }

    // The control of the arm above: every derivation, over C memory, crosses every door it reaches.
    [TestMethod]
    public void ABoxDerivedFromAPointerOverCMemoryStillCrosses()
    {
        void* memory = NativeMemory.AllocZeroed(64);

        try
        {
            ж<cgoHdr> native = new NativeBox<cgoHdr>((nuint)memory);
            slice<int32> sliced = @unsafe.Slice(new NativeBox<int32>((nuint)memory), 4);

            void Typed<T>(string form, ж<T> box) =>
                Assert.AreEqual(TypedDoors, DoorsCrossedBy(box).Count, form + " crosses every door over C memory");

            void Untyped(string form, @unsafe.Pointer pointer) =>
                Assert.AreEqual(UntypedDoors, DoorsCrossedBy(pointer).Count, form + " crosses every door over C memory");

            Typed("a field", native.of(cgoHdr.ᏑN));
            Typed("a struct field", native.of(cgoHdr.ᏑP));
            Typed("a reinterpret", native.Reinterpret<cgoHdr, sbyte>());
            Typed("a reinterpret of a field", native.of(cgoHdr.ᏑN).Reinterpret<int32, sbyte>());
            Typed("an element of unsafe.Slice", Ꮡ(sliced, 1));
            Typed("unsafe.SliceData", @unsafe.SliceData(sliced));
            Typed("an element of a re-slice", Ꮡ(sliced[1..], 1));
            Typed("typed unsafe.Add", @unsafe.Add(native, 4));
            Untyped("unsafe.Add over the pinning mint", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(native), 4));
            Untyped("unsafe.Add over a number", @unsafe.Add(new @unsafe.Pointer((uintptr)(nuint)memory), 4));
            Untyped("unsafe.Add through a user function", Returned(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(native), 4)));
            Untyped("a field through a user function", Returned(@unsafe.Pointer.FromPinnedBox(native.of(cgoHdr.ᏑN))));
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // Why the emission matters to every arm above: a call result that is CAST to a number, as the converter emitted it
    // until DESIGN-unsafe-pointer-call-result, carries nothing. It reaches the door as a bare number, and an interior
    // number crosses (the disclosed number residue). The arms above hold only because the cast is gone.
    [TestMethod]
    public void ACallResultCastToANumberCarriesNothingAndCrossesAsANumber()
    {
        array<byte> bytes = new array<byte>(32);
        ж<byte> first = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer carrying = @unsafe.Pointer.FromPinnedBox(first);

        @unsafe.Pointer kept = @unsafe.Add(carrying, 4);
        @unsafe.Pointer cast = (uintptr)@unsafe.Add(carrying, 4);

        Assert.IsNotNull(kept.RetainedSource, "the call's own result carries a box");
        Assert.IsNull(cast.RetainedSource, "the same result cast to a number carries nothing");
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(kept, "f")), GoMemoryOwner);
        Assert.AreEqual(carrying.Value.Value + 4, GoCgoArgs.VoidPointer(cast, "f"), "DISCLOSED: the bare interior number crosses");

        GC.KeepAlive(first);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<int32> field, WeakReference root) FieldOfADroppedView(array<byte> bytes)
    {
        ж<cgoHdr> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, cgoHdr>();

        return (view.of(cgoHdr.ᏑN), new WeakReference(view));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<int32> sliced, WeakReference root) SliceOverADroppedView(array<byte> bytes)
    {
        ж<int32> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, int32>();

        return (@unsafe.Slice(view, 4), new WeakReference(view));
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    // The harm the review measured for finding 1: `return &h.n` from a helper, the view dropped, a collection, and the
    // field's address no longer named the slice. The view holds the pin, so the field must hold the view.
    [TestMethod]
    public void AFieldOfAViewKeepsTheViewAndItsPinAlive()
    {
        array<byte> bytes = new array<byte>(32);
        (ж<int32> field, WeakReference root) = FieldOfADroppedView(bytes);

        Collect();

        Assert.IsTrue(root.IsAlive, "the view a field was taken from is collected while the field is alive, and its pin with it");
        GC.KeepAlive(field);
    }

    // The same lifetime, with no cgo anywhere (COORD's arm): a slice built by unsafe.Slice over a view of managed
    // memory held nothing that kept the view, so the pin could be released under a live slice.
    [TestMethod]
    public void AnUnsafeSliceOverAViewKeepsTheViewAndItsPinAlive()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[4] = 9;
        (slice<int32> sliced, WeakReference root) = SliceOverADroppedView(bytes);

        Collect();

        Assert.IsTrue(root.IsAlive, "the view an unsafe.Slice was built over is collected while the slice is alive, and its pin with it");
        Assert.AreEqual(9, sliced[1], "the slice still reads the storage it was built over");

        slice<int32> tail = sliced[1..];
        sliced = default;
        Collect();

        Assert.IsTrue(root.IsAlive, "a re-slice keeps the view as the slice it was cut from did");
        Assert.AreEqual(9, tail[0]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uintptr NumberOfATemporaryPointer(array<byte> bytes) => (uintptr)(ж<byte>)new ElemRefBox<byte>(bytes, 0);

    // Finding 6, the review's sequence: p1 is alive and pinned and its number is refused; a second, temporary pointer to
    // the same element is converted and dies; after a collection the SAME number must still be refused, because p1 still
    // pins the storage it names.
    [TestMethod]
    public void ANumberStaysRefusedWhileAnyPointerThatPinsItsStorageIsAlive()
    {
        array<byte> bytes = new array<byte>(32);
        ж<byte> p1 = new ElemRefBox<byte>(bytes, 0);
        uintptr number = (uintptr)p1;

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(number), "f")), GoMemoryOwner);

        Assert.AreEqual(number, NumberOfATemporaryPointer(bytes), "the temporary pointer names the same element");
        Collect();

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(number), "f")), GoMemoryOwner,
            "p1 is alive and pinned");

        // And the other order: the first registrant dies, and a later one is alive.
        uintptr early = NumberOfATemporaryPointer(bytes);
        ж<byte> p2 = new ElemRefBox<byte>(bytes, 0);
        Assert.AreEqual(early, (uintptr)p2);
        Collect();

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(early), "f")), GoMemoryOwner,
            "p2 is alive and pinned");

        GC.KeepAlive(p1);
        GC.KeepAlive(p2);
    }
}
