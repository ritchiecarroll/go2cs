using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using reflect = go.reflect_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo Phase 3 increment 3.1, the second review (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16.12): two derivation
// families that reach C as "C memory alone" with no uintptr in the Go source. Each shape is built as the converter at
// 83fbf28560 emits it, read from a converted probe:
//
//   MUST-FIX 1. (*T)(p), where p is an unsafe.Pointer VALUE holding an INTERIOR Go address, is emitted
//   `(ж<T>)(uintptr)(p)`. The number misses the registry (only a box's own address is registered), so the conversion
//   mints a native box with no source and no pin.
//
//   MUST-FIX 2. An ARRAY field of a struct viewed over Go memory is a native window that keeps nothing of the view:
//   `h.Ꮡname.Reinterpret<array<byte>, byte>()`, `@unsafe.Add(@unsafe.Pointer.FromPinnedBox(h.Ꮡname), 1)`,
//   `h.of(hdr.Ꮡname).slice()` with `Ꮡ(s, 1)` and `@unsafe.SliceData(s)`, and `h.at(hdr.Ꮡname, 1)`.
//
// Beside each family, the same shapes over C memory cross every door they reach, so a refusal is the classification
// and not the door. These arms are RED at 83fbf28560 by design: they are the evidence for the fix.
public unsafe partial class GoCgoArgsTests
{
    // A Go struct with an array field, laid out as Go lays it: pad at 0, n at 4, name [8]byte at 8; size 16.
    public struct cgoNamed
    {
        public int32 Pad;
        public int32 N;
        public array<byte> Name;

        public cgoNamed() => Name = new array<byte>(8);

        // Converter-named, so a field view can read the Go field name off the accessor.
        public static ref int32 ᏑN(ref cgoNamed instance) => ref instance.N;
        public static ref array<byte> ᏑName(ref cgoNamed instance) => ref instance.Name;
    }

    private static List<string> CrossingsOf<T>(string form, ж<T> box)
    {
        var crossings = new List<string>();

        foreach (string door in DoorsCrossedBy(box))
            crossings.Add(form + ": " + door);

        return crossings;
    }

    private static List<string> CrossingsOf(string form, @unsafe.Pointer pointer)
    {
        var crossings = new List<string>();

        foreach (string door in DoorsCrossedBy(pointer))
            crossings.Add(form + ": " + door);

        return crossings;
    }

    // ---- MUST-FIX 1: a typed pointer converted from an unsafe.Pointer value that holds an interior Go address.

    [TestMethod]
    public void ATypedPointerConvertedFromAnInteriorUnsafePointerIsRefusedAtEveryDoor()
    {
        slice<byte> b = new slice<byte>(16);
        array<byte> backing = new array<byte>(32);
        ж<cgoHdr> view = new ElemRefBox<byte>(backing, 0).Reinterpret<byte, cgoHdr>();
        ж<array<int32>> local = new StandardBox<array<int32>>(new array<int32>(4));

        var crossings = new List<string>();

        // p := unsafe.Add(unsafe.Pointer(&b[0]), 4); (*byte)(p)
        @unsafe.Pointer added = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 4);
        crossings.AddRange(CrossingsOf("(*byte)(p), p := unsafe.Add(unsafe.Pointer(&b[0]), 4)", (ж<byte>)(uintptr)(added)));

        // p := unsafe.Pointer(&h.n) over a view; (*int32)(p)
        @unsafe.Pointer field = @unsafe.Pointer.FromPinnedBox(view.of(cgoHdr.ᏑN));
        crossings.AddRange(CrossingsOf("(*int32)(p), p := unsafe.Pointer(&h.n) over a view", (ж<int32>)(uintptr)(field)));

        // var x [4]int32; p := unsafe.Pointer(&x[1]); (*int32)(p)
        @unsafe.Pointer element = @unsafe.Pointer.FromPinnedBox(local.at<int32>(1));
        crossings.AddRange(CrossingsOf("(*int32)(p), p := unsafe.Pointer(&x[1]) over a local", (ж<int32>)(uintptr)(element)));

        // (*byte)(id(unsafe.Add(unsafe.Pointer(&b[0]), 4))), through a user function
        crossings.AddRange(CrossingsOf("(*byte)(id(unsafe.Add(unsafe.Pointer(&b[0]), 4)))",
            (ж<byte>)(uintptr)(Returned(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0)), 4)))));

        Assert.AreEqual(0, crossings.Count, "typed pointers over interior Go memory that crossed as C memory:\n" + string.Join("\n", crossings));

        GC.KeepAlive(b);
        GC.KeepAlive(view);
        GC.KeepAlive(local);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<byte> typed, WeakReference root) TypedFromAnInteriorPointerOfADroppedBox(array<byte> bytes)
    {
        ж<byte> first = new ElemRefBox<byte>(bytes, 0);
        @unsafe.Pointer p = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4);

        return ((ж<byte>)(uintptr)(p), new WeakReference(first));
    }

    // A collection that compacts: the generations collected and finalized three times, then a blocking, compacting gen2
    // collection with the large object heap compacted too, then the freed space reused. What is not pinned or kept moves
    // or is overwritten, so a read that still answers after this read storage that survived a MOVE, not only a collect.
    private static void CollectAndCompact()
    {
        Collect();
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

    // The same conversion with no cgo anywhere: the typed pointer is the only thing left that names the storage, and it
    // must keep what holds that storage still (the box it was derived from, and its pin).
    [TestMethod]
    public void ATypedPointerConvertedFromAnInteriorUnsafePointerKeepsItsStorageAlive()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[4] = 9;
        (ж<byte> typed, WeakReference root) = TypedFromAnInteriorPointerOfADroppedBox(bytes);
        WeakReference storage = new WeakReference(bytes.Source);
        bytes = default;

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the box a typed pointer was converted from is collected while the pointer is alive, and its pin with it");
        Assert.IsTrue(storage.IsAlive, "the storage a typed pointer names is collected while the pointer is alive");
        Assert.AreEqual((byte)9, typed.Value, "the typed pointer still reads the storage it was converted from");
        GC.KeepAlive(typed);
    }

    // Offset 0 is the case the registry already answers: the number is a registered box's own address.
    [TestMethod]
    public void ATypedPointerConvertedFromAnUnsafePointerAtOffsetZeroIsRefused()
    {
        slice<byte> b = new slice<byte>(16);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ(b, 0));

        List<string> crossings = CrossingsOf("(*byte)(p), p := unsafe.Pointer(&b[0])", (ж<byte>)(uintptr)(p));

        Assert.AreEqual(0, crossings.Count, string.Join("\n", crossings));
        GC.KeepAlive(b);
    }

    // ---- MUST-FIX 2: an array field of a struct viewed over Go memory.

    [TestMethod]
    public void AnArrayFieldOfAViewOverGoMemoryIsRefusedAtEveryDoor()
    {
        array<byte> backing = new array<byte>(32);
        ж<array<byte>> goArray = new StandardBox<array<byte>>(new array<byte>(32));

        // h := (*named)(unsafe.Pointer(&b[0])), and h2 := (*named)(unsafe.Pointer(&a)) over a Go array.
        ж<cgoNamed> overSlice = new ElemRefBox<byte>(backing, 0).Reinterpret<byte, cgoNamed>();
        ж<cgoNamed> overArray = (ж<cgoNamed>)(uintptr)goArray;

        var crossings = new List<string>();

        foreach ((string over, ж<cgoNamed> h) in new[] { ("over a slice", overSlice), ("over a Go array", overArray) })
        {
            // (*C.char)(unsafe.Pointer(&h.name))
            crossings.AddRange(CrossingsOf("(*byte)(unsafe.Pointer(&h.name)) " + over, h.of(cgoNamed.ᏑName).Reinterpret<array<byte>, byte>()));

            // unsafe.Add(unsafe.Pointer(&h.name), 1)
            crossings.AddRange(CrossingsOf("unsafe.Add(unsafe.Pointer(&h.name), 1) " + over, @unsafe.Add(@unsafe.Pointer.FromPinnedBox(h.of(cgoNamed.ᏑName)), 1)));

            // s := h.name[:]; &s[1] and unsafe.SliceData(s)
            slice<byte> s = h.of(cgoNamed.ᏑName).slice();
            crossings.AddRange(CrossingsOf("&s[1], s := h.name[:] " + over, Ꮡ(s, 1)));
            crossings.AddRange(CrossingsOf("unsafe.SliceData(s), s := h.name[:] " + over, @unsafe.SliceData(s)));

            // &h.name[1]
            crossings.AddRange(CrossingsOf("&h.name[1] " + over, h.at<byte>(cgoNamed.ᏑName, 1)));
        }

        Assert.AreEqual(0, crossings.Count, "pointers into an array field of a view over Go memory that crossed as C memory:\n" + string.Join("\n", crossings));

        GC.KeepAlive(backing);
        GC.KeepAlive(goArray);
        GC.KeepAlive(overSlice);
        GC.KeepAlive(overArray);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<byte> element, WeakReference root) ElementOfAnArrayFieldOfADroppedView(array<byte> bytes)
    {
        ж<cgoNamed> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, cgoNamed>();

        return (view.at<byte>(cgoNamed.ᏑName, 1), new WeakReference(view));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<byte> sliced, WeakReference root) SliceOfAnArrayFieldOfADroppedView(array<byte> bytes)
    {
        ж<cgoNamed> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, cgoNamed>();

        return (view.of(cgoNamed.ᏑName).slice(), new WeakReference(view));
    }

    // With no cgo anywhere: `p := &h.name[1]` and `s := h.name[:]`, the view dropped, a collection. The view holds the pin,
    // so what was derived from its array field must hold the view.
    [TestMethod]
    public void AnArrayFieldOfADroppedViewKeepsTheViewAndItsPinAlive()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[9] = 7;

        (ж<byte> element, WeakReference elementRoot) = ElementOfAnArrayFieldOfADroppedView(bytes);
        (slice<byte> sliced, WeakReference sliceRoot) = SliceOfAnArrayFieldOfADroppedView(bytes);
        bytes = default;

        CollectAndCompact();

        var dead = new List<string>();

        if (!elementRoot.IsAlive)
            dead.Add("&h.name[1]: the view is collected while the element is alive");

        if (!sliceRoot.IsAlive)
            dead.Add("h.name[:]: the view is collected while the slice is alive");

        Assert.AreEqual(0, dead.Count, string.Join("\n", dead));
        Assert.AreEqual((byte)7, element.Value, "the element still reads the byte at Go offset 9");
        Assert.AreEqual((byte)7, sliced[1], "the slice still reads the byte at Go offset 9");

        GC.KeepAlive(element);
        GC.KeepAlive(sliced);
    }

    // ---- The review's SHOULDs.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<ж<int32>> sliced, WeakReference root) PointerSliceOverAFieldOfADroppedView(array<byte> bytes)
    {
        ж<cgoHdr> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, cgoHdr>();

        // unsafe.Slice((**int32)(unsafe.Pointer(&h.p)), 1): emitted @unsafe.Slice(h.Ꮡp.Reinterpret<P, ж<int32>>(), 1).
        return (@unsafe.Slice(view.of(cgoHdr.ᏑP).Reinterpret<cgoPoint, ж<int32>>(), 1), new WeakReference(view));
    }

    // U5: unsafe.Slice over a view of Go memory, with a POINTER element type. The pointer element takes the native
    // pointer-array route (unsafe.Slice's NativeArrayPointer arm), which keeps no anchor, where every other element
    // type's slice keeps the view.
    [TestMethod]
    public void AnUnsafeSliceOfPointersOverAViewKeepsTheViewAndIsRefusedAtEveryDoor()
    {
        array<byte> bytes = new array<byte>(32);
        (slice<ж<int32>> sliced, WeakReference root) = PointerSliceOverAFieldOfADroppedView(bytes);

        var crossings = new List<string>();
        crossings.AddRange(CrossingsOf("&sl[0], sl := unsafe.Slice((**int32)(unsafe.Pointer(&h.p)), 1)", Ꮡ(sliced, 0)));
        crossings.AddRange(CrossingsOf("unsafe.SliceData(sl)", @unsafe.SliceData(sliced)));

        CollectAndCompact();

        if (!root.IsAlive)
            crossings.Add("the view is collected while a slice of pointers over its field is alive");

        Assert.AreEqual(0, crossings.Count, string.Join("\n", crossings));
        GC.KeepAlive(sliced);
        GC.KeepAlive(bytes);
    }

    // U6: finding 8's reflect pair over the derived shapes. reflect.Value.UnsafePointer has no uintptr in the Go source;
    // `unsafe.Pointer(v.UnsafeAddr())` does, and it is named apart so a crossing there reads as the number it is.
    [TestMethod]
    public void TheReflectPairOverADerivedPointerIsRefusedAtTheVoidDoors()
    {
        array<byte> backing = new array<byte>(32);
        ж<cgoHdr> view = new ElemRefBox<byte>(backing, 0).Reinterpret<byte, cgoHdr>();
        ж<cgoNamed> named = new ElemRefBox<byte>(backing, 0).Reinterpret<byte, cgoNamed>();
        slice<int32> sliced = @unsafe.Slice(new ElemRefBox<byte>(backing, 0).Reinterpret<byte, int32>(), 4);

        var crossings = new List<string>();

        // reflect.ValueOf(&h.n).UnsafePointer()
        crossings.AddRange(CrossingsOf("reflect.ValueOf(&h.n).UnsafePointer()", reflect.ValueOf(view.of(cgoHdr.ᏑN)).UnsafePointer()));
        // reflect.ValueOf(h).UnsafePointer(), the view itself
        crossings.AddRange(CrossingsOf("reflect.ValueOf(h).UnsafePointer()", reflect.ValueOf(view).UnsafePointer()));
        // reflect.ValueOf(&sl[1]).UnsafePointer()
        crossings.AddRange(CrossingsOf("reflect.ValueOf(&sl[1]).UnsafePointer()", reflect.ValueOf(Ꮡ(sliced, 1)).UnsafePointer()));
        // reflect.ValueOf(&h.name[1]).UnsafePointer()
        crossings.AddRange(CrossingsOf("reflect.ValueOf(&h.name[1]).UnsafePointer()", reflect.ValueOf(named.at<byte>(cgoNamed.ᏑName, 1)).UnsafePointer()));
        // unsafe.Pointer(reflect.ValueOf(h).Elem().Field(1).UnsafeAddr()): a uintptr in the Go source
        crossings.AddRange(CrossingsOf("unsafe.Pointer(reflect.ValueOf(h).Elem().Field(1).UnsafeAddr()) [a uintptr in the Go source]",
            (@unsafe.Pointer)reflect.ValueOf(view.OrTypedNil()).Elem().Field(1).UnsafeAddr()));

        Assert.AreEqual(0, crossings.Count, "the reflect pair over derived pointers into Go memory that crossed as C memory:\n" + string.Join("\n", crossings));

        GC.KeepAlive(backing);
        GC.KeepAlive(view);
        GC.KeepAlive(named);
        GC.KeepAlive(sliced);
    }

    // ---- COORD's arms for 4.5's header rebase and 4.4's void-door exception.

    // unsafeheader.Slice and runtime.slice: Data unsafe.Pointer; Len, Cap int.
    private struct cgoSliceHeader
    {
        public @unsafe.Pointer Data;
        public nint Len;
        public nint Cap;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<int32> rebased, WeakReference root) HeaderRebasedOverAFieldOfADroppedView(array<byte> bytes)
    {
        ж<cgoHdr> view = new ElemRefBox<byte>(bytes, 0).Reinterpret<byte, cgoHdr>();

        // (*unsafeheader.Slice)(unsafe.Pointer(&s)).Data = unsafe.Pointer(&h.n); then Cap and Len.
        ref slice<int32> alias = ref heap(default(slice<int32>), out ж<slice<int32>> Ꮡalias);
        ж<cgoSliceHeader> hdr = Ꮡalias.Reinterpret<slice<int32>, cgoSliceHeader>();

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(view.of(cgoHdr.ᏑN));
        hdr.OnHeaderStore();
        hdr.Value.Cap = 2;
        hdr.OnHeaderStore();
        hdr.Value.Len = 2;
        hdr.OnHeaderStore();

        return (alias, new WeakReference(view));
    }

    // U7: a slice re-based through its header over a pointer derived from a view of Go memory, the view dropped, U2's
    // compacting collection. The rebased slice must keep the view, as unsafe.Slice over the same pointer does.
    [TestMethod]
    public void ASliceRebasedThroughItsHeaderOverAViewKeepsTheViewAlive()
    {
        array<byte> bytes = new array<byte>(32);
        bytes[4] = 7;
        (slice<int32> rebased, WeakReference root) = HeaderRebasedOverAFieldOfADroppedView(bytes);
        bytes = default;

        CollectAndCompact();

        Assert.IsTrue(root.IsAlive, "the view a slice header was re-based over is collected while the slice is alive, and its pin with it");
        Assert.AreEqual(7, (int)rebased[0], "the slice still reads the storage it was re-based over");
        GC.KeepAlive(rebased);
    }

    // G1, the guard on 4.4's void-door exception: an element of a native slice that is ANCHORED to a view of Go memory
    // is still refused at the void door, in both mints. The exception admits an element of C memory alone, and the
    // anchor is what tells the two apart.
    [TestMethod]
    public void AnElementOfAnArrayFieldOfAViewOverGoMemoryIsStillRefusedAtTheVoidDoor()
    {
        array<byte> backing = new array<byte>(32);
        ж<cgoNamed> view = new ElemRefBox<byte>(backing, 0).Reinterpret<byte, cgoNamed>();
        ж<byte> element = view.at<byte>(cgoNamed.ᏑName, 1);

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(@unsafe.Pointer.FromPinnedBox(element), "f")), GoMemoryOwner, "the pinning mint");
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.VoidPointer(@unsafe.Pointer.FromBox(element), "f")), GoMemoryOwner, "the retaining mint");

        GC.KeepAlive(backing);
        GC.KeepAlive(view);
    }

    // ---- The controls: the same shapes over C memory.

    [TestMethod]
    public void TheReviewsShapesOverCMemoryStillCross()
    {
        void* memory = NativeMemory.AllocZeroed(64);

        try
        {
            void Typed<T>(string form, ж<T> box) =>
                Assert.AreEqual(TypedDoors, DoorsCrossedBy(box).Count, form + " crosses every door over C memory");

            void Untyped(string form, @unsafe.Pointer pointer) =>
                Assert.AreEqual(UntypedDoors, DoorsCrossedBy(pointer).Count, form + " crosses every door over C memory");

            @unsafe.Pointer c = new @unsafe.Pointer((uintptr)(nuint)memory);
            ж<cgoHdr> hdr = new NativeBox<cgoHdr>((nuint)memory);
            ж<cgoNamed> named = new NativeBox<cgoNamed>((nuint)memory);

            // MUST-FIX 1's shapes.
            Typed("(*byte)(unsafe.Add(p, 4))", (ж<byte>)(uintptr)(@unsafe.Add(c, 4)));
            Typed("(*int32)(unsafe.Pointer(&h.n))", (ж<int32>)(uintptr)(@unsafe.Pointer.FromPinnedBox(hdr.of(cgoHdr.ᏑN))));
            Typed("(*byte)(id(unsafe.Add(p, 4)))", (ж<byte>)(uintptr)(Returned(@unsafe.Add(c, 4))));

            // MUST-FIX 2's shapes, but &h.name[1], which C2 reads on its own.
            Typed("(*byte)(unsafe.Pointer(&h.name))", named.of(cgoNamed.ᏑName).Reinterpret<array<byte>, byte>());
            Untyped("unsafe.Add(unsafe.Pointer(&h.name), 1)", @unsafe.Add(@unsafe.Pointer.FromPinnedBox(named.of(cgoNamed.ᏑName)), 1));
            slice<byte> s = named.of(cgoNamed.ᏑName).slice();
            Typed("&s[1], s := h.name[:]", Ꮡ(s, 1));
            Typed("unsafe.SliceData(s), s := h.name[:]", @unsafe.SliceData(s));
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // The review's SHOULD: an element of a C struct's array field is C memory, and the void door hands C its address.
    [TestMethod]
    public void AnElementOfAnArrayFieldOfACStructCrossesTheVoidDoorAsItsAddress()
    {
        void* memory = NativeMemory.AllocZeroed(64);

        try
        {
            ж<cgoNamed> named = new NativeBox<cgoNamed>((nuint)memory);
            ж<byte> element = named.at<byte>(cgoNamed.ᏑName, 1);

            nuint crossed = 0;
            string? refusal = null;

            try
            {
                crossed = GoCgoArgs.VoidPointer(@unsafe.Pointer.FromPinnedBox(element), "f");
            }
            catch (PanicException ex)
            {
                refusal = ex.Message;
            }

            Assert.IsNull(refusal, "&h.name[1] over C memory was refused at the void door: " + refusal);
            Assert.AreEqual((nuint)memory + 9, crossed, "the void door hands C the element's own address (Go offset 9)");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }
}
