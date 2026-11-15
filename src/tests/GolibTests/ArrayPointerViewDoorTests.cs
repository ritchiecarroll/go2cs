using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The view doors for Go's <c>(*A)(p)[lo:hi]</c>, A an array type and p an unsafe.Pointer
/// (docs/phase4/DESIGN-array-pointer-doors.md, door A): <c>@unsafe.Pointer.View</c>, <c>CopyInto</c> and
/// <c>CopyFrom</c>.
/// </summary>
/// <remarks>
/// <para>
/// Before the doors the converter emitted these forms as a span over the pointer cast to a number. The span
/// constructor copies, so a write through the slice never reached the memory (libc's memcpy, read, snprintf and
/// sqlite's column blobs all write through such a view), the capacity was the length, the length was narrowed to
/// int, and a named array longer than memory (<c>type RawMem [1&lt;&lt;50 - 1]byte</c>) could not be sliced open at
/// all.
/// </para>
/// <para>
/// One arm per answer the door gives, named by the design's letters. V3: an address the registry does not know (C
/// memory) is a native window with Go's capacity. V1: an element of a Go array of the element type, or such an
/// array named by its own address, is a managed window on that array, bounded by it. V2: any other Go value its
/// box can hold still (a scalar, a struct of scalars, an array of another element type) is a native window over
/// the value's own bytes, anchored on the box, so a write reaches the value and the value stays where it is for
/// as long as the view lives. V4: a Go value that holds a reference is a copy; a copy door refuses to write into
/// one. Then Go's panics for the bounds and the nil pointer, and the span limit by name.
/// </para>
/// </remarks>
[TestClass]
public class ArrayPointerViewDoorTests
{
    // libc's RawMem: an array no memory holds. The doors take it as a bound.
    private const long RawMem = (1L << 50) - 1;

    private const int NativeBytes = 64;

    // struct{ name string; n int64 }: it holds a reference, so its pointer's number is an order token, not an address.
    private struct Bearing
    {
        internal @string name;
        internal long n;
    }

    private static nint Filled(string with)
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        for (int i = 0; i < NativeBytes; i++)
            Marshal.WriteByte(block, i, i < with.Length ? (byte)with[i] : (byte)0);

        return block;
    }

    // `unsafe.Pointer(n)` for a number, as the converter emits it.
    private static @unsafe.Pointer Pointer(nint address) => (@unsafe.Pointer)(uintptr)(nuint)address;

    private static string Peek(nint block, int count)
    {
        char[] text = new char[count];

        for (int i = 0; i < count; i++)
            text[i] = (char)Marshal.ReadByte(block, i);

        return new string(text);
    }

    private static string Text(slice<byte> bytes)
    {
        char[] text = new char[bytes.Length];

        for (int i = 0; i < text.Length; i++)
            text[i] = (char)bytes[i];

        return new string(text);
    }

    private static string Text(array<byte> bytes) => Text(bytes[..]);

    private static slice<byte> Bytes(string text)
    {
        slice<byte> bytes = new byte[text.Length];

        for (int i = 0; i < text.Length; i++)
            bytes[i] = (byte)text[i];

        return bytes;
    }

    private static string PanicText(Action action)
    {
        try
        {
            action();
        }
        catch (PanicException panic)
        {
            return panic.Message ?? "";
        }

        Assert.Fail("no panic");
        return "";
    }

    private static void Collect()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    // ---- V3: C memory ----

    /// <summary>
    /// Arms A, E and K: <c>(*RawMem)(unsafe.Pointer(p))[:n]</c> over C memory. A write through the view is in the
    /// memory, a later write to the memory is in the view, and the capacity is Go's.
    /// </summary>
    [TestMethod]
    public void ATwoIndexViewOfNativeMemoryAliasesItWithGosCapacity()
    {
        nint block = Filled("XXXXXXXXXXXXXXXX");

        try
        {
            slice<byte> view = @unsafe.Pointer.View<byte>(Pointer(block), 0, 5, RawMem);

            Assert.AreEqual((nint)5, view.Length);
            Assert.AreEqual((nint)RawMem, view.Capacity, "a two-index view of the array has the array's capacity");

            view[0] = (byte)'u';
            Assert.AreEqual("uXXXXXXX", Peek(block, 8), "a write through the view must reach the memory");

            Marshal.WriteByte(block, 3, (byte)'!');
            Assert.AreEqual((byte)'!', view[3], "the view must read the memory as it is now");

            slice<byte> rest = view[1..];
            Assert.AreEqual((nint)(RawMem - 1), rest.Capacity);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>Arms B and I: <c>[lo:hi:max]</c> starts at element lo, with length hi-lo and capacity max-lo.</summary>
    [TestMethod]
    public void AThreeIndexViewOfNativeMemoryStartsAtItsLowBound()
    {
        nint block = Filled("abcdefghijklmnop");

        try
        {
            slice<byte> view = @unsafe.Pointer.View<byte>(Pointer(block), 2, 6, 8, RawMem);

            Assert.AreEqual("cdef", Text(view));
            Assert.AreEqual((nint)6, view.Capacity);

            view[3] = (byte)'!';
            Assert.AreEqual("abcde!gh", Peek(block, 8));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>An element wider than a byte: the low bound counts elements, and a write is the element's bytes.</summary>
    [TestMethod]
    public void AViewOfNativeMemoryStridesByItsElement()
    {
        nint block = Filled("");

        try
        {
            Marshal.WriteInt32(block, 4, 0x01020304);

            slice<uint> view = @unsafe.Pointer.View<uint>(Pointer(block), 1, 3, 4);

            Assert.AreEqual(0x01020304u, view[0]);

            view[1] = 0xa1b2c3d4;
            Assert.AreEqual(unchecked((int)0xa1b2c3d4), Marshal.ReadInt32(block, 8));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>Arm M's first form away from copy: <c>(*smallMem)(p)[lo:]</c> is the array from lo.</summary>
    [TestMethod]
    public void AnOpenViewOfASmallArrayIsTheArrayFromItsLowBound()
    {
        nint block = Filled("abcdefgh");

        try
        {
            slice<byte> view = @unsafe.Pointer.View<byte>(Pointer(block), 4, NativeBytes);

            Assert.AreEqual((nint)(NativeBytes - 4), view.Length);
            Assert.AreEqual((nint)(NativeBytes - 4), view.Capacity);
            Assert.AreEqual((byte)'e', view[0]);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- the bounds, the nil pointer, the span limit ----

    /// <summary>Go's panics for slicing an array, at full width: every text is the one Go prints.</summary>
    [TestMethod]
    public void TheBoundsPanicWithGosTexts()
    {
        nint block = Filled("");

        try
        {
            @unsafe.Pointer p = Pointer(block);

            Assert.AreEqual("runtime error: slice bounds out of range [:65] with length 64", PanicText(() => @unsafe.Pointer.View<byte>(p, 0, 65, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [3:2]", PanicText(() => @unsafe.Pointer.View<byte>(p, 3, 2, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [:-1]", PanicText(() => @unsafe.Pointer.View<byte>(p, 0, -1, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [::65] with length 64", PanicText(() => @unsafe.Pointer.View<byte>(p, 0, 4, 65, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [:5:4]", PanicText(() => @unsafe.Pointer.View<byte>(p, 0, 5, 4, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [3:2:]", PanicText(() => @unsafe.Pointer.View<byte>(p, 3, 2, 4, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [65:64]", PanicText(() => @unsafe.Pointer.View<byte>(p, 65, 64)));
            Assert.AreEqual("runtime error: slice bounds out of range [65:64]", PanicText(() => @unsafe.Pointer.CopyInto(p, 65, 64, Bytes("x"))));
            Assert.AreEqual("runtime error: slice bounds out of range [65:64]", PanicText(() => @unsafe.Pointer.CopyFrom(Bytes("x"), p, 65, 64)));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>Slicing through a nil array pointer is Go's nil dereference, whatever the bounds and even to copy nothing.</summary>
    [TestMethod]
    public void ANilArrayPointerIsANilDereference()
    {
        @unsafe.Pointer nilPointer = Pointer(0);
        const string nilText = "runtime error: invalid memory address or nil pointer dereference";

        Assert.AreEqual(nilText, PanicText(() => @unsafe.Pointer.View<byte>(nilPointer, 0, 0, 64)));
        Assert.AreEqual(nilText, PanicText(() => @unsafe.Pointer.View<byte>(null, 0, 0, 0, 64)));
        Assert.AreEqual(nilText, PanicText(() => @unsafe.Pointer.View<byte>(nilPointer, 0, 64)));
        Assert.AreEqual(nilText, PanicText(() => @unsafe.Pointer.CopyInto(nilPointer, 0, 64, default(slice<byte>))));
        Assert.AreEqual(nilText, PanicText(() => @unsafe.Pointer.CopyFrom(default(slice<byte>), nilPointer, 0, 64)));
    }

    /// <summary>
    /// A length no span holds is refused by name, at its full value: the narrowing it replaces read
    /// <c>2^32 + 5</c> as five elements. A small length under the same array is not refused.
    /// </summary>
    [TestMethod]
    public void ALengthPastTheSpanLimitIsRefusedByName()
    {
        nint block = Filled("");

        try
        {
            @unsafe.Pointer p = Pointer(block);
            const string limit = "exceeds the maximum slice length of this runtime";

            StringAssert.Contains(PanicText(() => @unsafe.Pointer.View<byte>(p, 0, (long)Array.MaxLength + 1, RawMem)), limit);
            StringAssert.Contains(PanicText(() => @unsafe.Pointer.View<byte>(p, 0, (1L << 32) + 5, RawMem)), $"length {(1L << 32) + 5} {limit}");
            StringAssert.Contains(PanicText(() => @unsafe.Pointer.View<byte>(p, 0, RawMem)), limit, "the open form of an array longer than a span, away from copy");

            Assert.AreEqual((nint)4, @unsafe.Pointer.View<byte>(p, 0, 4, RawMem).Length);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- V1: a Go array of the element type ----

    /// <summary>
    /// Arms G and O: <c>(*RawMem)(unsafe.Pointer(&amp;buf[0]))[:n:n]</c> over a Go byte array. The view is a window
    /// on the array itself, so a collection between the view and the write changes nothing.
    /// </summary>
    [TestMethod]
    public void AViewAtAnElementOfAGoArrayIsAWindowOnTheArray()
    {
        array<byte> buf = new(16);

        slice<byte> view = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, 16, 16, RawMem);

        Collect();
        copy(view, Bytes("errno 7"));

        Assert.AreEqual("errno 7", Text(buf)[..7], "a write through the view must reach the Go array");
        Assert.AreEqual((nint)16, view.Capacity);
    }

    /// <summary>
    /// The capacity of a view of a Go array ends where the array ends, whatever the bound says: there is no storage
    /// past it. From element 4 of sixteen, twelve remain.
    /// </summary>
    [TestMethod]
    public void AViewOfAGoArrayHasTheArraysRemainingCapacity()
    {
        array<byte> buf = new(16);

        Assert.AreEqual((nint)16, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, 4, RawMem).Capacity);
        Assert.AreEqual((nint)12, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4)), 0, 4, RawMem).Capacity);
        Assert.AreEqual((nint)10, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4)), 2, 4, RawMem).Capacity);
        Assert.AreEqual((nint)6, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4)), 2, 4, 8, RawMem).Capacity);
    }

    /// <summary>
    /// libc's C ABI: the pointer arrives as a NUMBER (<c>memcpy(dest uintptr, ...)</c>) taken from
    /// <c>&amp;buf[0]</c> by the caller, who still holds the array. The number resolves to the element.
    /// </summary>
    [TestMethod]
    public void ANumberTakenFromAnElementOfAGoArrayIsAWindowOnTheArray()
    {
        array<byte> buf = new(16);
        ж<byte> first = Ꮡ(buf, 0);
        uintptr dest = (uintptr)first;

        slice<byte> view = @unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 2, 6, 6, RawMem);
        copy(view, Bytes("wxyz"));

        Assert.AreEqual("\0\0wxyz\0\0", Text(buf)[..8]);
        GC.KeepAlive(first);
    }

    /// <summary>
    /// Arm pd: a Go array named by its OWN address, <c>uintptr(unsafe.Pointer(&amp;buf))</c>, as sqlite's generated
    /// code names its package arrays for memcpy. The address is the array's element 0. unsafe.Slice's element
    /// window misses this number, so the write was lost; the door resolves the array.
    /// </summary>
    [TestMethod]
    public void ANumberTakenFromAGoArrayItselfIsAWindowOnTheArray()
    {
        ref array<byte> buf = ref heap(new array<byte>(8), out ж<array<byte>> Ꮡbuf);
        uintptr dest = (uintptr)Ꮡbuf;

        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, 8, 8, RawMem), Bytes("ÙÕ\u0005ù ¡c×"));
        Assert.AreEqual((byte)0xd9, buf[0]);
        Assert.AreEqual((byte)0xd7, buf[7]);

        slice<byte> tail = @unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 6, 8, RawMem);
        tail[0] = 1;
        Assert.AreEqual((byte)1, buf[6]);
        Assert.AreEqual((nint)2, tail.Capacity);

        // The same array through the pointer that carries its box.
        @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡbuf), 0, 1, RawMem)[0] = 7;
        Assert.AreEqual((byte)7, buf[0]);

        GC.KeepAlive(Ꮡbuf);
    }

    /// <summary>
    /// A view that would pass the end of the Go array is refused by name before anything is cut: the managed array
    /// is all the storage there is, where Go would read and write its neighbours.
    /// </summary>
    [TestMethod]
    public void AViewPastTheEndOfAGoArrayIsRefusedByName()
    {
        array<byte> buf = new(16);

        string text = PanicText(() => @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4)), 0, 13, RawMem));

        Assert.AreEqual($"go2cs: (*[{RawMem}]uint8)(p)[0:13] would pass the end of the Go array p points into (12 elements remain)", text);
    }

    // ---- V2: a Go value of another type that its box holds still ----

    // sqlite's random number state (`sqlite3Prng`, sqlite_windows.go): struct{ s [16]uint32; out [64]uint8; n uint8 },
    // 132 bytes with no reference in it.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Prng
    {
        internal fixed uint s[16];
        internal fixed byte @out[64];
        internal byte n;
    }

    // struct{ a, b uint32; c uint64 }
    [StructLayout(LayoutKind.Sequential)]
    private struct Pair
    {
        internal uint a;
        internal uint b;
        internal ulong c;

        internal static ref uint Ꮡa(ref Pair instance) => ref instance.a;

        internal static ref ulong Ꮡc(ref Pair instance) => ref instance.c;
    }

    // sqlite's chacha20_init and eight bytes of another table.
    private static array<uint> ChachaInit() => new(new uint[] { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 });

    private static array<byte> Magic() => new(new byte[] { 0xd9, 0xd5, 0x05, 0xf9, 0x20, 0xa1, 0x63, 0xd7 });

    // libc's Xmemcpy on windows (libc.go:1959) as the door emission spells it: both sides are views of a number.
    private static void Memcpy(uintptr dest, uintptr src, long n)
    {
        if (n != 0)
            copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, n, n, RawMem), @unsafe.Pointer.View<byte>((@unsafe.Pointer)src, 0, n, n, RawMem));
    }

    /// <summary>
    /// Arm N: a uint64 viewed as eight bytes. The view is the value's own storage: a write through it is in the
    /// value, and a later write to the value is in the view.
    /// </summary>
    [TestMethod]
    public void AViewOfAGoValueOfAnotherTypeIsItsBytes()
    {
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);

        slice<byte> view = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡx), 0, 8, 8, 8);

        Assert.AreEqual((byte)0x08, view[0]);
        Assert.AreEqual((byte)0x01, view[7]);

        view[0] = 0xff;
        Assert.AreEqual(0x01020304050607ffUL, Ꮡx.Value, "a write through the view must reach the value");

        Ꮡx.Value = 0x1112131415161718UL;
        Assert.AreEqual((byte)0x18, view[0], "the view must read the value as it is now");

        slice<byte> inner = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡx), 2, 4, 8);

        Assert.AreEqual((byte)0x16, inner[0], "the low bound must offset the view");
        Assert.AreEqual((nint)6, inner.Capacity, "a two-index view has the array's capacity from its low bound");
        GC.KeepAlive(Ꮡx);
    }

    /// <summary>
    /// The same through the value's number, which is how libc's C ABI receives it. A view wider than the value
    /// (RawMem) has the value's capacity: the value is all the storage there is.
    /// </summary>
    [TestMethod]
    public void ANumberTakenFromAGoValueOfAnotherTypeIsItsBytes()
    {
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);
        uintptr number = Ꮡx;

        slice<byte> view = @unsafe.Pointer.View<byte>((@unsafe.Pointer)number, 0, 4, RawMem);

        Assert.AreEqual((nint)8, view.Capacity, "the view ends where the value ends");

        view[3] = 0xaa;
        Assert.AreEqual(0x01020304aa060708UL, Ꮡx.Value);

        slice<ushort> words = @unsafe.Pointer.View<ushort>((@unsafe.Pointer)number, 1, 3, 1 << 20);

        Assert.AreEqual((ushort)0xaa06, words[0], "a wider element strides by its own size");
        Assert.AreEqual((nint)3, words.Capacity);
        GC.KeepAlive(Ꮡx);
    }

    /// <summary>
    /// Arm pa, sqlite3_randomness (sqlite_windows.go:5802): <c>memcpy(&amp;sqlite3Prng, &amp;chacha20_init, 16)</c>,
    /// the first seeding of sqlite's random number generator. Before V2 the destination was a copy and the state
    /// stayed zero.
    /// </summary>
    [TestMethod]
    public unsafe void MemcpyIntoAStructOfArraysReachesTheStruct()
    {
        Assert.AreEqual(132, sizeof(Prng));

        heap(default(Prng), out ж<Prng> Ꮡprng);
        heap(ChachaInit(), out ж<array<uint>> Ꮡinit);

        Memcpy(Ꮡprng, Ꮡinit, 16);

        Prng state = Ꮡprng.Value;

        Assert.AreEqual(0x61707865u, state.s[0]);
        Assert.AreEqual(0x3320646eu, state.s[1]);
        Assert.AreEqual(0x79622d32u, state.s[2]);
        Assert.AreEqual(0x6b206574u, state.s[3]);
        Assert.AreEqual(0u, state.s[4], "sixteen bytes and no more");
        GC.KeepAlive(Ꮡinit);
    }

    /// <summary>
    /// Arm pb, sqlite3PrngSaveState: the whole state, struct to struct, both named by their numbers.
    /// </summary>
    [TestMethod]
    public unsafe void MemcpyFromAStructToAStructCopiesTheWholeState()
    {
        Prng initial = default;

        initial.s[5] = 7;
        initial.@out[2] = 9;
        initial.n = 3;

        heap(initial, out ж<Prng> Ꮡprng);
        heap(default(Prng), out ж<Prng> Ꮡsaved);

        Memcpy(Ꮡsaved, Ꮡprng, sizeof(Prng));

        Prng saved = Ꮡsaved.Value;

        Assert.AreEqual(7u, saved.s[5]);
        Assert.AreEqual((byte)9, saved.@out[2]);
        Assert.AreEqual((byte)3, saved.n);
        GC.KeepAlive(Ꮡprng);
    }

    /// <summary>
    /// Arm pc: a struct of scalars as the destination.
    /// </summary>
    [TestMethod]
    public void MemcpyIntoAStructOfScalarsReachesItsFields()
    {
        heap(default(Pair), out ж<Pair> Ꮡpair);
        heap(ChachaInit(), out ж<array<uint>> Ꮡinit);

        Memcpy(Ꮡpair, Ꮡinit, 16);

        Assert.AreEqual(0x61707865u, Ꮡpair.Value.a);
        Assert.AreEqual(0x3320646eu, Ꮡpair.Value.b);
        Assert.AreEqual(0x6b20657479622d32UL, Ꮡpair.Value.c);
        GC.KeepAlive(Ꮡinit);
    }

    /// <summary>
    /// Arm pe: a Go array of another element type as the destination, named by the array's own number.
    /// </summary>
    [TestMethod]
    public void MemcpyIntoAGoArrayOfAnotherElementReachesTheArray()
    {
        heap(new array<uint>(4), out ж<array<uint>> Ꮡw);
        heap(Magic(), out ж<array<byte>> Ꮡmagic);

        Memcpy(Ꮡw, Ꮡmagic, 8);

        Assert.AreEqual(0xf905d5d9u, Ꮡw.Value[0]);
        Assert.AreEqual(0xd763a120u, Ꮡw.Value[1]);
        Assert.AreEqual(0u, Ꮡw.Value[2]);

        string past = PanicText(() => @unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)Ꮡw, 0, 17, RawMem));

        Assert.AreEqual($"go2cs: (*[{RawMem}]uint8)(p)[0:17] would pass the end of the Go value p points to (16 elements of it remain)", past);
        GC.KeepAlive(Ꮡmagic);
    }

    /// <summary>
    /// A named Go array (<c>type small [64]byte</c>) named by its own number: the bytes of the array it holds.
    /// </summary>
    [TestMethod]
    public void ANumberTakenFromANamedGoArrayIsItsElements()
    {
        FloorSmallMem small = default;

        small[0] = (byte)'a';

        heap(small, out ж<FloorSmallMem> Ꮡsmall);
        uintptr number = Ꮡsmall;

        slice<byte> view = @unsafe.Pointer.View<byte>((@unsafe.Pointer)number, 0, 4, RawMem);

        Assert.AreEqual((byte)'a', view[0]);
        Assert.AreEqual((nint)64, view.Capacity);

        view[1] = (byte)'k';
        Assert.AreEqual((byte)'k', Ꮡsmall.Value[1], "a write through the view must reach the named array");

        Assert.AreEqual((nint)4, @unsafe.Pointer.CopyInto((@unsafe.Pointer)number, 2, 64, Bytes("wxyz")));
        Assert.AreEqual((byte)'z', Ꮡsmall.Value[5]);
        GC.KeepAlive(Ꮡsmall);
    }

    /// <summary>
    /// A view at a field of a Go struct: the field's own bytes, and room to the end of the struct the field is in,
    /// which is the allocation Go's pointer rules allow the view to reach.
    /// </summary>
    [TestMethod]
    public void AViewAtAFieldOfAGoStructEndsWhereTheStructEnds()
    {
        heap(new Pair { a = 1, b = 2, c = 0x0102030405060708UL }, out ж<Pair> Ꮡpair);

        slice<byte> tail = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡpair.of(Pair.Ꮡc)), 0, 8, RawMem);

        Assert.AreEqual((nint)8, tail.Capacity);

        tail[0] = 0xee;
        Assert.AreEqual(0x01020304050607eeUL, Ꮡpair.Value.c);

        slice<byte> whole = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡpair.of(Pair.Ꮡa)), 0, 16, RawMem);

        Assert.AreEqual((byte)2, whole[4], "the second field follows the first");

        string past = PanicText(() => @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡpair.of(Pair.Ꮡc)), 0, 9, RawMem));

        Assert.AreEqual($"go2cs: (*[{RawMem}]uint8)(p)[0:9] would pass the end of the Go value p points to (8 elements of it remain)", past);
        GC.KeepAlive(Ꮡpair);
    }

    /// <summary>
    /// One Go value of the element type (<c>(*[1]byte)(unsafe.Pointer(&amp;b))[:]</c>) is that value, and a view of
    /// more than one element of it is refused by name.
    /// </summary>
    [TestMethod]
    public void AViewOfOneGoValueOfTheElementTypeIsThatValue()
    {
        heap((byte)7, out ж<byte> Ꮡb);

        slice<byte> view = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, 1);

        Assert.AreEqual((byte)7, view[0]);

        view[0] = 9;
        Assert.AreEqual((byte)9, Ꮡb.Value);

        string past = PanicText(() => @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, 2, RawMem));

        Assert.AreEqual($"go2cs: (*[{RawMem}]uint8)(p)[0:2] would pass the end of the Go value p points to (1 elements of it remain)", past);
        GC.KeepAlive(Ꮡb);
    }

    // ---- V2 under stress (the design's section 5 and stop line 1) ----

    private const int StressRounds = 10_000;

    // Garbage of varying sizes ahead of the round's values, so that a compacting collection has somewhere to slide
    // them to. What a round allocates here is dead by the collection.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Churn(int round)
    {
        for (int i = 0; i < 8; i++)
            GC.KeepAlive(new byte[64 + (round * 31 + i * 257) % 1500]);
    }

    private static unsafe nuint AddressOf(byte[] array)
    {
        fixed (byte* first = array)
            return (nuint)first;
    }

    // The three shapes of section 5 as one round's variables: a uint64, a [4]uint32 and a struct of scalars, each
    // viewed as bytes through its number.
    private sealed class Round
    {
        internal ж<ulong> scalar = null!;
        internal ж<array<uint>> words = null!;
        internal ж<Pair> pair = null!;
    }

    private readonly struct Views
    {
        internal readonly slice<byte> scalar;
        internal readonly slice<byte> words;
        internal readonly slice<byte> pair;

        internal Views(Round round)
        {
            scalar = @unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)round.scalar, 0, 8, RawMem);
            words = @unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)round.words, 0, 16, RawMem);
            pair = @unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)round.pair, 0, 16, RawMem);
        }

        internal void Write(byte seed)
        {
            for (int i = 0; i < 8; i++)
                scalar[i] = (byte)(seed + i);

            for (int i = 0; i < 16; i++)
            {
                words[i] = (byte)(seed + 2 * i);
                pair[i] = (byte)(seed + 3 * i);
            }
        }

        internal bool Reads(byte seed)
        {
            for (int i = 0; i < 8; i++)
                if (scalar[i] != (byte)(seed + i))
                    return false;

            for (int i = 0; i < 16; i++)
                if (words[i] != (byte)(seed + 2 * i) || pair[i] != (byte)(seed + 3 * i))
                    return false;

            return true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Round NewRound()
    {
        Round round = new();

        heap(0UL, out round.scalar);
        heap(new array<uint>(4), out round.words);
        heap(default(Pair), out round.pair);

        return round;
    }

    // What the variables themselves hold, read through their boxes and never through a view.
    private static bool VariablesHold(Round round, byte seed)
    {
        ulong scalar = 0;

        for (int i = 7; i >= 0; i--)
            scalar = scalar << 8 | (byte)(seed + i);

        if (round.scalar.Value != scalar)
            return false;

        for (int word = 0; word < 4; word++)
        {
            uint want = 0;

            for (int i = 3; i >= 0; i--)
                want = want << 8 | (byte)(seed + 2 * (4 * word + i));

            if (round.words.Value[word] != want)
                return false;
        }

        Pair pair = round.pair.Value;
        Span<byte> bytes = MemoryMarshal.AsBytes(new Span<Pair>(ref pair));

        for (int i = 0; i < 16; i++)
            if (bytes[i] != (byte)(seed + 3 * i))
                return false;

        return true;
    }

    // The second mode's round: the views, and weak references to the variables, which nothing else holds once
    // this returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (Views views, WeakReference<Round> holder, WeakReference[] variables) ViewsOfVariablesNothingElseHolds(byte seed)
    {
        Round round = NewRound();
        Views views = new(round);

        views.Write(seed);

        return (views, new WeakReference<Round>(round), [new WeakReference(round.scalar), new WeakReference(round.words), new WeakReference(round.pair)]);
    }

    // The variables of a second-mode round, reached again through the weak references, or null when one is gone.
    private static Round? Revived(WeakReference[] variables)
    {
        if (variables[0].Target is ж<ulong> scalar && variables[1].Target is ж<array<uint>> words && variables[2].Target is ж<Pair> pair)
            return new Round { scalar = scalar, words = words, pair = pair };

        return null;
    }

    /// <summary>
    /// Stop line 1. 10,000 rounds, each with its own variables allocated behind fresh garbage: write through the
    /// views, force a compacting collection, read through the variables and through the views. In the second mode
    /// the variables go out of use and the views alone must keep them, where they are, with what was written. One
    /// wrong round withdraws V2.
    /// </summary>
    /// <remarks>
    /// The control inside the test is an array nothing holds still, allocated the same way each round: the
    /// collection must have moved it in some rounds, or the rounds prove nothing about a value that did not move.
    /// </remarks>
    [TestMethod]
    [Timeout(900_000)]
    public void TenThousandRoundsOfWriteCollectReadKeepEveryViewOnItsValue()
    {
        int moved = 0;

        for (int i = 0; i < StressRounds; i++)
        {
            byte seed = (byte)(i * 7 + 1);

            Churn(i);

            // Mode 1: the variables stay in use.
            Round held = NewRound();
            Views heldViews = new(held);

            // Mode 2: only the views stay.
            (Views loneViews, WeakReference<Round> holder, WeakReference[] variables) = ViewsOfVariablesNothingElseHolds((byte)(seed + 100));

            byte[] unheld = new byte[96];
            nuint before = AddressOf(unheld);

            heldViews.Write(seed);

            Collect();

            if (AddressOf(unheld) != before)
                moved++;

            if (!VariablesHold(held, seed))
                Assert.Fail($"round {i}: a write through a view is not in its variable after the collection");

            if (!heldViews.Reads(seed))
                Assert.Fail($"round {i}: a view no longer reads its variable after the collection");

            // Checked before any read through the lone views: a view that did not keep its value is over memory
            // that is no longer the value's.
            if (Revived(variables) is not { } revived)
            {
                Assert.Fail($"round {i}: a view did not keep the value it views");
                return;
            }

            if (holder.TryGetTarget(out _))
                Assert.Fail($"round {i}: the test's own holder is still reachable, so the views are not what kept the values");

            if (!loneViews.Reads((byte)(seed + 100)))
                Assert.Fail($"round {i}: a view whose variable went out of use no longer reads what was written");

            if (!VariablesHold(revived, (byte)(seed + 100)))
                Assert.Fail($"round {i}: a view whose variable went out of use is no longer over that variable");

            GC.KeepAlive(held);
            GC.KeepAlive(unheld);
        }

        Assert.IsTrue(moved > 0, $"the collection moved the unheld control in {moved} of {StressRounds} rounds: the rounds must be able to move a value");
        Console.WriteLine($"V2 stress: {StressRounds} rounds, the unheld control moved in {moved}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (slice<byte> window, WeakReference value) AWindowWithoutItsAnchor()
    {
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);
        uintptr number = Ꮡx;

        return (go.slice<byte>.OverNativeMemory((nuint)number.Value, 8, 8, null), new WeakReference(Ꮡx));
    }

    /// <summary>
    /// The twin (safety floor item 13): the same window cut WITHOUT the anchor does not keep the value, so the
    /// second mode above rests on the anchor and on nothing else. The window is not read here: its memory is no
    /// longer the value's.
    /// </summary>
    [TestMethod]
    public void WithoutItsAnchorTheSameWindowDoesNotKeepTheValue()
    {
        (slice<byte> window, WeakReference value) = AWindowWithoutItsAnchor();

        Collect();

        Assert.IsFalse(value.IsAlive, "a bare native window keeps nothing; the door's anchor is what keeps the value");
        Assert.AreEqual((nint)8, window.Length);
    }

    // ---- V4: a Go value that holds a reference ----

    /// <summary>
    /// A Go value that holds a reference has no address: its pointer's number is an order token. The span emission
    /// read memory AT that number. The door reads through the box, which refuses a token by name.
    /// </summary>
    [TestMethod]
    public void AViewOfAGoValueThatHoldsAReferenceIsRefusedByNameAtTheRead()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);

        string text = PanicText(() => @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, 8, 8, RawMem));

        StringAssert.Contains(text, "with no address", "the order token's own refusal");
        Assert.AreEqual((nint)0, @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, 0, RawMem).Length, "a view of no elements reads nothing");
        GC.KeepAlive(Ꮡb);
    }

    // ---- the copy doors ----

    /// <summary>
    /// Arms C and C0: <c>copy((*RawMem)(unsafe.Pointer(buf))[:], wd)</c>. Go copies len(wd) bytes; nothing of the
    /// array's length is spanned.
    /// </summary>
    [TestMethod]
    public void CopyIntoNativeMemoryCopiesTheSourcesLength()
    {
        nint block = Filled("XXXXXXXXXXXXXXXX");

        try
        {
            Assert.AreEqual((nint)4, @unsafe.Pointer.CopyInto(Pointer(block), 0, RawMem, Bytes("/tmp")));
            Assert.AreEqual("/tmpXXXX", Peek(block, 8));

            Assert.AreEqual((nint)2, @unsafe.Pointer.CopyInto(Pointer(block), 6, RawMem, (@string)"ab"), "a string source");
            Assert.AreEqual("/tmpXXab", Peek(block, 8));

            Assert.AreEqual((nint)0, @unsafe.Pointer.CopyInto(Pointer(block), 0, RawMem, default(slice<byte>)));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>The array's own length bounds the copy when it is the smaller: four bytes remain from 60 of 64.</summary>
    [TestMethod]
    public void CopyIntoStopsAtTheArraysLength()
    {
        nint block = Filled("");

        try
        {
            Assert.AreEqual((nint)4, @unsafe.Pointer.CopyInto(Pointer(block), 60, NativeBytes, Bytes("0123456789")));
            Assert.AreEqual((byte)'3', Marshal.ReadByte(block, 63));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary><c>copy(dst, (*RawMem)(unsafe.Pointer(p))[lo:])</c>: len(dst) elements from lo.</summary>
    [TestMethod]
    public void CopyFromNativeMemoryCopiesTheDestinationsLength()
    {
        nint block = Filled("abcdefghijklmnop");

        try
        {
            slice<byte> dst = new byte[6];

            Assert.AreEqual((nint)6, @unsafe.Pointer.CopyFrom(dst, Pointer(block), 2, RawMem));
            Assert.AreEqual("cdefgh", Text(dst));

            Assert.AreEqual((nint)2, @unsafe.Pointer.CopyFrom(dst, Pointer(block), NativeBytes - 2, NativeBytes), "two elements remain in the array");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arm W, the twin of sqlite-cs w5b2: the three statements over one local <c>[16]byte</c>, with Go's answers
    /// (docs/phase4/data/array-pointer-doors/amend-1114/w/go.out).
    /// </summary>
    [TestMethod]
    public void TheCopyDoorsOverAGoArrayAnswerAsGoDoes()
    {
        array<byte> buf = new(16);
        array<byte> @out = new(4);

        nint n = @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, RawMem, (@string)"name\0");
        Assert.AreEqual((nint)5, n);
        Assert.AreEqual("name\0\0", Text(buf)[..6]);

        n = @unsafe.Pointer.CopyFrom(@out[..], @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, RawMem);
        Assert.AreEqual((nint)4, n);
        Assert.AreEqual("name", Text(@out));

        n = @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 12, RawMem, (@string)"wxyz");
        Assert.AreEqual((nint)4, n);
        Assert.AreEqual("\0\0wxyz", Text(buf)[10..]);
    }

    /// <summary>
    /// COORD's ruling 6: over a Go array the copy doors stop at the array. A 20-byte source into the 16-byte array
    /// and a 32-byte destination from it are refused by name, and nothing is copied. Go has no answer for either
    /// program: it writes or reads past its own allocation.
    /// </summary>
    [TestMethod]
    public void TheCopyDoorsRefusePastTheEndOfAGoArrayByNameAndCopyNothing()
    {
        array<byte> buf = new(16);
        slice<byte> wide = new byte[32];

        for (int i = 0; i < 16; i++)
            buf[i] = (byte)'b';

        string into = PanicText(() => @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, RawMem, Bytes("01234567890123456789")));
        Assert.AreEqual($"go2cs: copy through (*[{RawMem}]uint8)(p)[0:] would pass the end of the Go array p points into (16 elements remain)", into);
        Assert.AreEqual(new string('b', 16), Text(buf), "the refused copy must not have written");

        string from = PanicText(() => @unsafe.Pointer.CopyFrom(wide, @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, RawMem));
        Assert.AreEqual($"go2cs: copy through (*[{RawMem}]uint8)(p)[0:] would pass the end of the Go array p points into (16 elements remain)", from);
        Assert.AreEqual((byte)0, wide[0], "the refused copy must not have written");

        // The array's length exactly is not past its end.
        Assert.AreEqual((nint)16, @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 0)), 0, RawMem, Bytes("0123456789abcdef")));
    }

    /// <summary>
    /// A copy door whose destination can only be viewed as a copy (a Go value that holds a reference) refuses by
    /// name: the write would be made and lost. A Go value of another type that its box holds still is written.
    /// </summary>
    [TestMethod]
    public void CopyIntoAGoValueTheDoorCannotAliasIsRefusedByName()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);

        string bearing = PanicText(() => @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡb), 0, RawMem, Bytes("abcd")));
        Assert.AreEqual($"go2cs: copy through (*[{RawMem}]uint8)(p)[0:] cannot write the Go value p points to: it is not an array of uint8, and a view of it is a copy", bearing);

        Assert.AreEqual((nint)4, @unsafe.Pointer.CopyInto(@unsafe.Pointer.FromPinnedBox(Ꮡx), 0, 8, Bytes("abcd")));
        Assert.AreEqual(0x0102030464636261UL, Ꮡx.Value, "the four bytes are in the value");

        slice<byte> dst = new byte[8];
        Assert.AreEqual((nint)8, @unsafe.Pointer.CopyFrom(dst, @unsafe.Pointer.FromPinnedBox(Ꮡx), 0, 8));
        Assert.AreEqual((byte)0x61, dst[0]);
        Assert.AreEqual((byte)0x01, dst[7]);

        GC.KeepAlive(Ꮡb);
        GC.KeepAlive(Ꮡx);
    }
}
