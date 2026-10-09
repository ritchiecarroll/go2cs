using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Guards golib's slice-HEADER reinterpretation (ж.SliceHeaderBox.cs): <c>(*slice)(unsafe.Pointer(&amp;b))</c>,
/// emitted as <c>Ꮡb.Reinterpret&lt;slice&lt;T&gt;, Δsliceᴛ&gt;()</c>, now materializes Go's (array, len, cap)
/// from the LIVE slice instead of minting a NativeBox over the pinned managed struct whose first field
/// read back the backing array's reference as a pointer (measured 2026-09-04: a type-confused
/// <c>System.Byte[]</c>, len/cap = golib's low/length, a native SIGSEGV on the first dereference).
/// Arms: the words of a subslice and the retained element box behind <c>array</c>; nil and empty-non-nil;
/// a reassigned slice followed through the box; a write through the header refused by name on the next
/// access; the pointer type's factory contract the adapter resolves by reflection; and the shapes the
/// adapter must NOT touch — a three-integer struct (the address route, unchanged) and the string header
/// (expected-today: the string half is Q44's, and this arm flips when it lands). The negative control is
/// the arm's removal from Reinterpret: the subslice and nil arms then read the type-confused header.
/// </summary>
[TestClass]
public class SliceHeaderReinterpretTests
{
    // The runtime's own slice header (runtime.slice: array unsafe.Pointer; len, cap int).
    private struct SliceHeaderShape
    {
        public @unsafe.Pointer array;
        public nint len;
        public nint cap;
    }

    // Three integers: NOT a header, and must not be adapted.
    private struct ThreeWords
    {
        public nint a;
        public nint b;
        public nint c;
    }

    // The runtime's string header (runtime.stringStruct: str unsafe.Pointer; len int).
    private struct StringHeaderShape
    {
        public @unsafe.Pointer str;
        public nint len;
    }

    private static byte[] Backing(int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
            bytes[i] = (byte)(0xA0 + i);
        return bytes;
    }

    [TestMethod]
    public void ASubsliceHeaderCarriesGosWordsAndRetainsTheElementBox()
    {
        byte[] backing = Backing(40);
        slice<byte> whole = new(backing);
        slice<byte> sub = whole[16..24];
        ref slice<byte> b = ref heap(sub, out ж<slice<byte>> Ꮡb);

        ж<SliceHeaderShape> header = Ꮡb.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.IsInstanceOfType(header, typeof(SliceHeaderBox<slice<byte>, SliceHeaderShape>), "the pair takes the header box, not the address route");
        Assert.AreEqual(8, (int)header.Value.len, "len is the subslice's");
        Assert.AreEqual(24, (int)header.Value.cap, "cap runs to the end of the backing, as Go's does");

        @unsafe.Pointer array = header.Value.array;
        Assert.AreEqual(typeof(@unsafe.Pointer), ((object)array).GetType(), "array is a real unsafe.Pointer, not a type-confused reference");
        Assert.IsFalse(array.IsNull, "a non-nil slice's array pointer is non-nil");
        Assert.IsInstanceOfType(array.RetainedSource, typeof(ж<byte>), "the pointer retains the element box");
        Assert.AreEqual(backing[16], ((ж<byte>)array.RetainedSource!).Value, "the element box is element 0 of the SUBSLICE (backing[16]), not of the backing");

        // The same pointer object across accesses while the slice's words stand still.
        Assert.AreSame(array, header.Value.array, "the pointer is minted once per (backing, low)");
    }

    [TestMethod]
    public void NilAndEmptySlicesReadAsGoReadsThem()
    {
        slice<byte> nilSlice = default;
        ref slice<byte> n = ref heap(nilSlice, out ж<slice<byte>> Ꮡn);
        ж<SliceHeaderShape> nilHeader = Ꮡn.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.IsTrue(nilHeader.Value.array.IsNull, "a nil slice's array word is nil");
        Assert.AreEqual(0, (int)nilHeader.Value.len);
        Assert.AreEqual(0, (int)nilHeader.Value.cap);

        // make([]byte, 0, 8): len 0, cap 8, and a NON-nil array pointing at the backing.
        slice<byte> empty = new slice<byte>(new byte[8])[..0];
        ref slice<byte> e = ref heap(empty, out ж<slice<byte>> Ꮡe);
        ж<SliceHeaderShape> emptyHeader = Ꮡe.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.IsFalse(emptyHeader.Value.array.IsNull, "an empty non-nil slice still points at its backing, as Go's does");
        Assert.AreEqual(0, (int)emptyHeader.Value.len);
        Assert.AreEqual(8, (int)emptyHeader.Value.cap);
    }

    [TestMethod]
    public void AZeroCapacitySliceHasANonNilArrayWordAndNoPastTheEndPointer()
    {
        // runtime's TestMemclr: MemclrBytes(mem[size:size]) reads the header of a slice whose low
        // index IS the backing's length. Go never builds a pointer past the end (a cap-0 slice keeps
        // an in-bounds base); the header read threw IndexOutOfRangeException minting element 0.
        byte[] backing = Backing(8);
        slice<byte> atEnd = new slice<byte>(backing)[8..8];
        ref slice<byte> a = ref heap(atEnd, out ж<slice<byte>> Ꮡa);
        ж<SliceHeaderShape> atEndHeader = Ꮡa.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.IsFalse(atEndHeader.Value.array.IsNull, "a non-nil slice's array word is non-nil, cap 0 or not");
        Assert.AreEqual(0, (int)atEndHeader.Value.len);
        Assert.AreEqual(0, (int)atEndHeader.Value.cap);

        // make([]byte, 0): an empty backing, cap 0.
        slice<byte> made = new slice<byte>(0);
        ref slice<byte> m = ref heap(made, out ж<slice<byte>> Ꮡm);
        ж<SliceHeaderShape> madeHeader = Ꮡm.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.IsFalse(madeHeader.Value.array.IsNull, "make([]byte, 0) is non-nil, so its array word is too");
        Assert.AreEqual(0, (int)madeHeader.Value.cap);
    }

    [TestMethod]
    public void AHeaderReadAfterACapZeroViewFollowsTheSameLowOnceItHasCapacity()
    {
        // s = backing[8:8:8] (cap 0 at an in-bounds low: the zerobase), then s = backing[8:16]
        // through the SAME header box: same backing, same low, cap 8. The pointer cache keyed only
        // on (backing, low) kept the zerobase, so the clear landed on it and missed the slice.
        byte[] backing = Backing(16);
        ref slice<byte> s = ref heap(new slice<byte>(backing, 8, 8, 8), out ж<slice<byte>> Ꮡs);
        ж<SliceHeaderShape> header = Ꮡs.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.AreEqual(0, (int)header.Value.cap);

        s = new slice<byte>(backing, 8, 16, 16);
        @unsafe.Pointer array = header.Value.array;
        Assert.AreEqual(8, (int)header.Value.cap);

        runtime_package.GoMemclrNoHeapPointers(array, new uintptr(8));

        for (int i = 8; i < 16; i++)
            Assert.AreEqual((byte)0, backing[i], $"backing[{i}]");
    }

    [TestMethod]
    public void AReassignedSliceIsFollowedThroughTheHeader()
    {
        byte[] backing = Backing(40);
        slice<byte> whole = new(backing);
        ref slice<byte> b = ref heap(whole, out ж<slice<byte>> Ꮡb);
        ж<SliceHeaderShape> header = Ꮡb.Reinterpret<slice<byte>, SliceHeaderShape>();

        Assert.AreEqual(40, (int)header.Value.len);
        @unsafe.Pointer before = header.Value.array;

        // The header aliases the VARIABLE: assigning a new slice to it moves the words the header shows.
        Ꮡb.Value = whole[8..12];

        Assert.AreEqual(4, (int)header.Value.len, "len follows the reassignment");
        Assert.AreEqual(32, (int)header.Value.cap, "cap follows the reassignment");
        Assert.AreNotSame(before, header.Value.array, "the low index moved, so the pointer was re-minted");
        Assert.AreEqual(backing[8], ((ж<byte>)header.Value.array.RetainedSource!).Value, "…at the new element 0");
    }

    [TestMethod]
    public void AWriteThroughTheHeaderIsRefusedByNameOnTheNextAccess()
    {
        slice<byte> whole = new(Backing(16));
        ref slice<byte> b = ref heap(whole, out ж<slice<byte>> Ꮡb);
        ж<SliceHeaderShape> header = Ꮡb.Reinterpret<slice<byte>, SliceHeaderShape>();

        header.Value.len = 3;   // lands on the materialized copy: the managed slice cannot be rebuilt from it

        PanicException refusal = Assert.ThrowsException<PanicException>(() => _ = header.Value.cap);
        StringAssert.Contains(refusal.Message, "slice header written through a reinterpretation");

        // The slice itself was never touched.
        Assert.AreEqual(16, (int)Ꮡb.Value.Length);
    }

    [TestMethod]
    public void ThePointerTypesFactoryContractTheAdapterResolvesByReflection()
    {
        // golib cannot name the unsafe assembly, so the adapter resolves `FromBoxObject(object)` on the
        // header's pointer field type by name (trim stage 3c-2a: the untyped twin of FromBox<X>, so the adapter closes no
        // generic method over the element). This arm is what pins that contract.
        MethodInfo? fromBoxObject = typeof(@unsafe.Pointer).GetMethod("FromBoxObject", BindingFlags.Public | BindingFlags.Static, [typeof(object)]);

        Assert.IsNotNull(fromBoxObject, "unsafe.Pointer.FromBoxObject(object) must exist, public and static");
        Assert.AreEqual(typeof(@unsafe.Pointer), fromBoxObject!.ReturnType, "…returning the pointer type");
        Assert.IsTrue(typeof(IUnsafePointer).IsAssignableFrom(typeof(@unsafe.Pointer)), "…on a type the adapter's detector admits");

        // The typed FromBox<X> stays the converter's mint, and the name the adapter looks up stays unambiguous.
        MethodInfo? fromBox = typeof(@unsafe.Pointer).GetMethod("FromBox", BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(fromBox, "unsafe.Pointer.FromBox must exist, public and static");
        Assert.IsTrue(fromBox!.IsGenericMethodDefinition && fromBox.GetGenericArguments().Length == 1, "…generic in the pointee");

        // And the factory retains without pinning: a transient address with the box behind it.
        byte value = 7;
        ref byte v = ref heap(value, out ж<byte> Ꮡv);
        @unsafe.Pointer p = @unsafe.Pointer.FromBoxObject(Ꮡv);
        Assert.AreSame(Ꮡv, p.RetainedSource);
    }

    [TestMethod]
    public void ShapesTheAdapterMustNotTouch()
    {
        // Three integers over a slice: not a header — the pre-existing route, whatever it is, not the header box.
        slice<byte> whole = new(Backing(16));
        ref slice<byte> b = ref heap(whole, out ж<slice<byte>> Ꮡb);
        ж<ThreeWords> words = Ꮡb.Reinterpret<slice<byte>, ThreeWords>();
        Assert.IsNotInstanceOfType(words, typeof(SliceHeaderBox<slice<byte>, ThreeWords>), "a three-integer struct is not a header");
        Assert.IsFalse(SliceHeaderBox<slice<byte>, ThreeWords>.Applies);

        // A header over an ARRAY box, not a slice: Go has no such reinterpretation and the adapter declines.
        Assert.IsFalse(SliceHeaderBox<array<byte>, SliceHeaderShape>.Applies, "the source must be slice<X>");

        // The STRING header is the other half of the seam and is deliberately NOT switched on here: its
        // live route is the (uintptr) token bridge, which resolves a reference-bearing @string box only once
        // Q44's token registry lands. EXPECTED TODAY; this arm flips when the string half is admitted.
        Assert.IsFalse(SliceHeaderBox<@string, StringHeaderShape>.Applies, "the string half is not admitted (Q44 first)");
    }
}
