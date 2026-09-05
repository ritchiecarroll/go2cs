using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Guards the header → slice reinterpretation (increment 7 of the runtime row, W2a's mirror):
/// <c>*(*[]T)(unsafe.Pointer(&amp;sl))</c> over a <c>notInHeapSlice</c>-shaped header — a
/// <c>ж</c>-typed array box and two <c>nint</c>s — becomes a native-backed slice over the header's
/// address with Go's (len, cap), the two shapes the box refuses are refused by name, the shapes it
/// must not touch are not adapted, and the capacity door it re-bases through carries Go's third word.
/// Driven over unmanaged heap memory (AllocHGlobal) rather than mmap so the arms are platform-neutral.
/// </summary>
[TestClass]
public class HeaderSliceReinterpretTests
{
    // The runtime's notInHeapSlice: array *notInHeap; len, cap int — a ж-typed first field.
    private struct NativeHeaderShape
    {
        public ж<byte> array;
        public nint len;
        public nint cap;
    }

    // The runtime's slice header (array unsafe.Pointer; len, cap int) — the Pointer-first shape the
    // mirror deliberately leaves on its existing route.
    private struct PointerHeaderShape
    {
        public @unsafe.Pointer array;
        public nint len;
        public nint cap;
    }

    // Three integers: NOT a header.
    private struct ThreeWords
    {
        public nint a;
        public nint b;
        public nint c;
    }

    private struct NotASlice
    {
        public nint x;
    }

    private static ж<byte> NativePointer(IntPtr buffer)
    {
        unsafe
        {
            ж<byte> pointer = (void*)buffer;
            return pointer;
        }
    }

    [TestMethod]
    public void ANativeHeaderBecomesASliceAliasingTheMemoryWithGosWords()
    {
        Assert.IsTrue(HeaderSliceBox<NativeHeaderShape, slice<byte>>.Applies, "the ж-first header over slice<byte> is the box's shape");

        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            for (int i = 0; i < 64; i++)
                Marshal.WriteByte(buffer, i, (byte)i);

            NativeHeaderShape header = new() { array = NativePointer(buffer), len = 4, cap = 16 };
            ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);

            slice<byte> s = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>();

            Assert.AreEqual(4, (int)s.Length, "len is the header's second word");
            Assert.AreEqual(16, (int)s.Capacity, "cap is the header's third word");
            Assert.IsTrue(s.IsNativeBacked, "re-based over the address, not copied");
            Assert.AreEqual((byte)2, s[2], "reads reach the memory");

            s[2] = 0x5A;
            Assert.AreEqual(0x5A, Marshal.ReadByte(buffer, 2), "writes reach the memory");

            slice<byte> grown = s[..12];
            Assert.AreEqual(12, (int)grown.Length, "a reslice within cap is the same window, longer");
            Assert.AreEqual((byte)11, grown[11], "and it reads the bytes past the original length");

            // A second view over the same header is the same words again (the header was not consumed).
            slice<byte> again = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>();
            Assert.AreEqual(4, (int)again.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [TestMethod]
    public void ANilArrayWithZeroWordsIsGosNilSlice()
    {
        NativeHeaderShape header = new() { array = null!, len = 0, cap = 0 };
        ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);

        slice<byte> s = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>();

        Assert.IsTrue(s == nil, "array nil, len 0, cap 0 is the nil slice");
        Assert.AreEqual(0, (int)s.Length);

        // The typed nil box is the same nil.
        NativeHeaderShape boxedNil = new() { array = global::go.ж<byte>.NilBox, len = 0, cap = 0 };
        ref NativeHeaderShape sl2 = ref heap(boxedNil, out ж<NativeHeaderShape> Ꮡsl2);
        Assert.IsTrue((~Ꮡsl2.Reinterpret<NativeHeaderShape, slice<byte>>()) == nil);
    }

    [TestMethod]
    public void ANilArrayWithALengthIsRefusedByName()
    {
        NativeHeaderShape header = new() { array = null!, len = 3, cap = 3 };
        ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);

        PanicException refusal = Assert.ThrowsException<PanicException>(() => _ = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>());
        StringAssert.Contains(refusal.Message, "nil array with len 3 and cap 3");
    }

    [TestMethod]
    public void AManagedElementBoxIsRefusedByName()
    {
        slice<byte> managed = new(new byte[8]);
        NativeHeaderShape header = new() { array = Ꮡ(managed, 0), len = 8, cap = 8 };
        ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);

        PanicException refusal = Assert.ThrowsException<PanicException>(() => _ = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>());
        StringAssert.Contains(refusal.Message, "names MANAGED storage");
    }

    [TestMethod]
    public void AReferenceBearingElementTypeIsRefusedByTheDoor()
    {
        Assert.IsTrue(HeaderSliceBox<NativeHeaderShape, slice<@string>>.Applies, "the shape is admitted; the element type is the door's to refuse");

        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            NativeHeaderShape header = new() { array = NativePointer(buffer), len = 1, cap = 1 };
            ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);

            PanicException refusal = Assert.ThrowsException<PanicException>(() => _ = ~Ꮡsl.Reinterpret<NativeHeaderShape, slice<@string>>());
            StringAssert.Contains(refusal.Message, "contains managed references");
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [TestMethod]
    public void AReassignmentThroughTheViewIsRefusedOnTheNextAccess()
    {
        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            NativeHeaderShape header = new() { array = NativePointer(buffer), len = 4, cap = 16 };
            ref NativeHeaderShape sl = ref heap(header, out ж<NativeHeaderShape> Ꮡsl);
            ж<slice<byte>> view = Ꮡsl.Reinterpret<NativeHeaderShape, slice<byte>>();

            view.Value = new slice<byte>(new byte[2]);   // lands on the materialized copy

            PanicException refusal = Assert.ThrowsException<PanicException>(() => _ = view.Value.Length);
            StringAssert.Contains(refusal.Message, "slice written through a header reinterpretation");

            // The header itself was never touched.
            Assert.AreEqual(4, (int)sl.len);
            Assert.AreEqual(16, (int)sl.cap);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [TestMethod]
    public void ShapesTheBoxMustNotTouch()
    {
        Assert.IsFalse(HeaderSliceBox<ThreeWords, slice<byte>>.Applies, "three integers are not a header");
        Assert.IsFalse(HeaderSliceBox<NativeHeaderShape, NotASlice>.Applies, "the target must be a slice");
        Assert.IsFalse(HeaderSliceBox<PointerHeaderShape, slice<byte>>.Applies, "the unsafe.Pointer-first header stays on its existing route (readMetricsLocked's managed array)");
        Assert.IsFalse(HeaderSliceBox<slice<byte>, slice<byte>>.Applies, "a slice is not a header");
        Assert.IsFalse(HeaderSliceBox<nint, slice<byte>>.Applies, "a primitive is not a header");
    }

    [TestMethod]
    public void TheCapacityDoorCarriesGosThirdWord()
    {
        IntPtr buffer = Marshal.AllocHGlobal(64);

        try
        {
            slice<byte> s = global::go.slice<byte>.OverNativeMemory((nuint)(nint)buffer, 2, 8);
            Assert.AreEqual(2, (int)s.Length);
            Assert.AreEqual(8, (int)s.Capacity);

            slice<byte> two = global::go.slice<byte>.OverNativeMemory((nuint)(nint)buffer, 5);
            Assert.AreEqual(5, (int)two.Length);
            Assert.AreEqual(5, (int)two.Capacity);

            PanicException refusal = Assert.ThrowsException<PanicException>(() => global::go.slice<byte>.OverNativeMemory((nuint)(nint)buffer, 4, 2));
            StringAssert.Contains(refusal.Message, "capacity 2 is smaller than length 4");
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
