using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The element and array doors for Go's <c>(*A)(p)[i]</c>, <c>&amp;(*A)(p)[i]</c>, <c>*(*A)(p)</c> and
/// <c>*(*A)(p) = v</c>, A an array type and p an unsafe.Pointer (docs/phase4/DESIGN-array-pointer-doors.md, door B,
/// 4.5 and 10.5): <c>@unsafe.Pointer.ElementRef</c>, <c>Element</c>, <c>LoadArray</c> and <c>StoreArray</c>.
/// </summary>
/// <remarks>
/// <para>
/// Before the doors these forms went through the pointer conversion to the array type. Over C memory that is
/// refused by name (<c>cannot view native memory as array&lt;Int32&gt;</c>), and a named array was read as a managed
/// wrapper out of the memory. The index form is the population in ccgo's output: 384 sites in sqlite and libc on
/// windows/amd64 and 784 on linux/amd64.
/// </para>
/// <para>
/// One arm per answer, named as the view doors name them. V3: C memory, where the location is the address and
/// nothing is allocated. V1: an element of a Go array of the element type, bounded by that array. V2: a Go value
/// of another type that its box holds still, bounded by that value. V4: a Go value nothing can alias, which the
/// pointer conversion answers. Then Go's panics for the index and the nil pointer.
/// </para>
/// </remarks>
[TestClass]
public class ArrayPointerElementDoorTests
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

    // struct{ a, b uint32; c uint64 }
    [StructLayout(LayoutKind.Sequential)]
    private struct Pair
    {
        internal uint a;
        internal uint b;
        internal ulong c;
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
    /// Arm R: <c>(*[4]int32)(unsafe.Pointer(p))[1] = 7</c> and its read, over C memory. The location is the memory:
    /// a store is in it, a later write to the memory is read, and the compound and step forms work on it in place.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationInNativeMemoryIsTheMemory()
    {
        nint block = Filled("");

        try
        {
            @unsafe.Pointer.ElementRef<int>(Pointer(block), 1, 4) = 7;
            Assert.AreEqual(7, Marshal.ReadInt32(block, 4), "the store must be in the memory");
            Assert.AreEqual(7, @unsafe.Pointer.ElementRef<int>(Pointer(block), 1, 4));

            Marshal.WriteInt32(block, 12, 40);
            Assert.AreEqual(40, @unsafe.Pointer.ElementRef<int>(Pointer(block), 3, 4), "the element strides by its size");

            @unsafe.Pointer.ElementRef<int>(Pointer(block), 3, 4) += 2;
            @unsafe.Pointer.ElementRef<int>(Pointer(block), 3, 4)++;
            Assert.AreEqual(43, Marshal.ReadInt32(block, 12), "compound assignment and ++ act on the memory");
            Assert.AreEqual(0, Marshal.ReadInt32(block, 0), "no other element moves");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arms D and M: <c>(*RawMem)(unsafe.Pointer(buf))[len(wd)] = 0</c>, libc's terminator store. The array's length
    /// is a bound and nothing of that length exists.
    /// </summary>
    [TestMethod]
    public void AnIndexedStoreThroughAnArrayNoMemoryHoldsIsOneByte()
    {
        nint block = Filled("XXXXXXXX");

        try
        {
            @unsafe.Pointer.ElementRef<byte>(Pointer(block), 4, RawMem) = 0;
            Assert.AreEqual("XXXX\0XXX", Peek(block, 8));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arm S: <c>e := &amp;(*[4]int32)(unsafe.Pointer(p))[2]</c>. Over C memory the pointer is a native box at the
    /// element's address that keeps nothing.
    /// </summary>
    [TestMethod]
    public void TheAddressOfAnElementInNativeMemoryIsItsAddress()
    {
        nint block = Filled("");

        try
        {
            ж<int> e = @unsafe.Pointer.Element<int>(Pointer(block), 2, 4);

            e.Value = 9;
            Assert.AreEqual(9, Marshal.ReadInt32(block, 8));
            Assert.IsTrue(e is NativeBox<int> { RetainedSource: null } native && native.NativeAddress == (nuint)block + 8,
                "C memory alone: a native box at the element, keeping nothing");
            Assert.AreEqual((uintptr)((nuint)block + 8), (uintptr)e, "its number is the element's address");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arm T: <c>*(*[2]byte)(unsafe.Pointer(p)) = [2]byte{'C', 0}</c>, and the load <c>v := *(*[4]byte)(p)</c>. The
    /// store writes N elements to the memory; the load is a Go array value, which is a copy.
    /// </summary>
    [TestMethod]
    public void AnArrayStoredAndLoadedThroughNativeMemoryIsCopiedElementByElement()
    {
        nint block = Filled("XXXXXXXX");

        try
        {
            @unsafe.Pointer.StoreArray<byte>(Pointer(block), 2, new array<byte>(new byte[] { (byte)'C', 0 }));
            Assert.AreEqual("C\0XXXXXX", Peek(block, 8));

            array<byte> loaded = @unsafe.Pointer.LoadArray<byte>(Pointer(block), 4);

            Assert.AreEqual((nint)4, loaded.Length);
            Assert.AreEqual((byte)'C', loaded[0]);
            Assert.AreEqual((byte)'X', loaded[3]);

            Marshal.WriteByte(block, 0, (byte)'Z');
            Assert.AreEqual((byte)'C', loaded[0], "a loaded array is a value: a later write to the memory is not in it");

            loaded[3] = (byte)'!';
            Assert.AreEqual("Z\0XX", Peek(block, 4), "and a write to it is not in the memory");

            array<uint> words = @unsafe.Pointer.LoadArray<uint>(Pointer(block), 2);

            Assert.AreEqual((uint)Marshal.ReadInt32(block, 4), words[1], "the load strides by its element");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// A stored value shorter than N is the zero value of an array that was never given storage
    /// (<c>default(array&lt;T&gt;)</c>, a field of a struct made by <c>default</c>). Go's value is N zeros.
    /// </summary>
    [TestMethod]
    public void AStoredArrayThatWasNeverGivenStorageIsZeros()
    {
        nint block = Filled("XXXXXXXX");

        try
        {
            @unsafe.Pointer.StoreArray<byte>(Pointer(block), 4, default(array<byte>));
            Assert.AreEqual("\0\0\0\0XXXX", Peek(block, 8));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    private static long AllocatedByIndexedLoop(@unsafe.Pointer p, int count)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        uint sum = 0;

        for (int i = 0; i < count; i++)
        {
            @unsafe.Pointer.ElementRef<uint>(p, i & 1023, 1L << 20) = (uint)i;
            sum += @unsafe.Pointer.ElementRef<uint>(p, (i + 1) & 1023, 1L << 20);
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        GC.KeepAlive(sum);

        return allocated;
    }

    // The measured loop: 1,000,000 iterations, a store and a read each, so 2,000,000 accesses.
    private const int IndexedLoopIterations = 1_000_000;

    // The smallest object a 64-bit process allocates: header, method table, one field.
    private const long SmallestObjectBytes = 24;

    // What the window may hold that is not per access. An access that allocates allocates at least one object, so a
    // per-access cost reads at least 2,000,000 x 24 B = 48 MB. A cost paid once per call or once per process is not
    // per access, and it lands in the window whenever the JIT moves the loop between tiers inside it: the loop's
    // unoptimized body allocates 24 B per call (measured with on-stack replacement off), and a full-suite Release
    // run read 5,784 B once (COORD, 2026-11-16). 64 KB is eleven times that one-time cost and below one smallest
    // object per 700 accesses, so the bound keeps the claim (nothing per access) and drops what is not that claim.
    private const long OneTimeCostBound = 64 * 1024;

    /// <summary>
    /// Stop line 7 of the design (10.5): stores and reads through <c>(*[1&lt;&lt;20]uint32)(unsafe.Pointer(c))[i]</c>
    /// over C memory allocate NOTHING per access, after one warm-up pass. The window is bounded, not zero: a tier
    /// change inside it is a one-time cost (see <see cref="OneTimeCostBound"/>). The control is the same loop over a
    /// Go slice's first element, which takes the element door's box and must read at least one smallest object per
    /// access, so the instrument reads a per-access cost when there is one, far above the bound.
    /// </summary>
    [TestMethod]
    public void AnIndexedLoopOverNativeMemoryAllocatesNothing()
    {
        nint block = Marshal.AllocHGlobal(4096);

        try
        {
            @unsafe.Pointer c = Pointer(block);
            const int last = IndexedLoopIterations - 1;

            AllocatedByIndexedLoop(c, 1000);

            long native = AllocatedByIndexedLoop(c, IndexedLoopIterations);

            Assert.IsTrue(native < OneTimeCostBound,
                $"bytes allocated by {2L * IndexedLoopIterations:N0} stores and reads over C memory: {native:N0}, " +
                $"where the one-time bound is {OneTimeCostBound:N0}");
            Assert.AreEqual((uint)last, (uint)Marshal.ReadInt32(block, (last & 1023) * 4), "and the stores are in the memory");

            slice<uint> s = new uint[1024];
            ж<uint> first = Ꮡ(s, 0);
            @unsafe.Pointer g = @unsafe.Pointer.FromPinnedBox(first);

            AllocatedByIndexedLoop(g, 1000);

            long managed = AllocatedByIndexedLoop(g, IndexedLoopIterations);

            Assert.IsTrue(managed >= 2L * IndexedLoopIterations * SmallestObjectBytes,
                $"the control: the same loop over a Go slice takes a box per access, read {managed:N0} bytes");
            Assert.AreEqual((uint)last, s[last & 1023], "and its stores are in the slice");
            GC.KeepAlive(first);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- V1: a Go array of the element type ----

    /// <summary>
    /// <c>(*RawMem)(unsafe.Pointer(&amp;buf[4]))[2]</c>: an element of a Go array is that array's element. The
    /// pointer to it is the array's own element box, so a collection changes nothing.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationAtAnElementOfAGoArrayIsTheArraysElement()
    {
        array<byte> buf = new(16);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4));

        @unsafe.Pointer.ElementRef<byte>(p, 2, RawMem) = 9;
        Assert.AreEqual((byte)9, buf[6]);

        ж<byte> e = @unsafe.Pointer.Element<byte>(p, 3, RawMem);

        Collect();
        e.Value = 11;
        Assert.AreEqual((byte)11, buf[7], "a write through the element pointer reaches the Go array after a collection");
        Assert.IsTrue(e.TryGetElementStorage(out byte[]? backing, out nint index) && ReferenceEquals(backing, buf.Source) && index == 7,
            "the pointer is an element box of the Go array itself");
    }

    /// <summary>
    /// The bound N is a promise; the Go array is all the storage there is. An index inside N and past the end of the
    /// Go array is refused by name, where the address arithmetic would have written the heap past the array.
    /// </summary>
    [TestMethod]
    public void AnIndexPastTheEndOfAGoArrayIsRefusedByName()
    {
        array<byte> buf = new(16);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 4));

        Assert.AreEqual((byte)0, @unsafe.Pointer.ElementRef<byte>(p, 11, RawMem), "the last element is in reach");

        string text = PanicText(() => @unsafe.Pointer.ElementRef<byte>(p, 12, RawMem) = 1);

        StringAssert.Contains(text, "(*[1125899906842623]uint8)(p)[12]");
        StringAssert.Contains(text, "would pass the end of the Go array p points into");

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.Element<byte>(p, 12, RawMem)), "would pass the end of the Go array p points into");
    }

    /// <summary>
    /// A Go array named by its OWN number, <c>uintptr(unsafe.Pointer(&amp;a))</c>, as ccgo's output names its arrays:
    /// the number is the array's element 0.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationAtTheNumberOfAGoArrayIsTheArraysElement()
    {
        ref array<uint> a = ref heap(new array<uint>(4), out ж<array<uint>> Ꮡa);
        uintptr number = (uintptr)Ꮡa;

        @unsafe.Pointer.ElementRef<uint>((@unsafe.Pointer)number, 1, 4) = 7;
        Assert.AreEqual(7u, a[1]);

        a[3] = 21;
        Assert.AreEqual(21u, @unsafe.Pointer.ElementRef<uint>((@unsafe.Pointer)number, 3, 4));
        GC.KeepAlive(Ꮡa);
    }

    /// <summary>
    /// The array forms over a Go array: the store reaches the array, the load is a copy of it.
    /// </summary>
    [TestMethod]
    public void AnArrayStoredAndLoadedThroughAGoArrayAnswersAsGoDoes()
    {
        array<byte> buf = new(8);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡ(buf, 2));

        @unsafe.Pointer.StoreArray<byte>(p, 3, new array<byte>(new byte[] { 1, 2, 3 }));
        Assert.AreEqual((byte)1, buf[2]);
        Assert.AreEqual((byte)3, buf[4]);
        Assert.AreEqual((byte)0, buf[5]);

        array<byte> loaded = @unsafe.Pointer.LoadArray<byte>(p, 3);

        buf[2] = 9;
        Assert.AreEqual((byte)1, loaded[0], "a loaded array does not follow the Go array");

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.StoreArray<byte>(p, 7, new array<byte>(7))), "would pass the end of the Go array p points into");
        Assert.AreEqual((byte)9, buf[2], "a refused store writes nothing");
    }

    // ---- V2: a Go value of another type ----

    /// <summary>
    /// <c>(*[8]byte)(unsafe.Pointer(&amp;x))[0] = 0xff</c>, x a uint64: the location is the value's own byte.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationInAGoValueOfAnotherTypeIsItsBytes()
    {
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡx);

        Assert.AreEqual((byte)0x01, @unsafe.Pointer.ElementRef<byte>(p, 7, 8));

        @unsafe.Pointer.ElementRef<byte>(p, 0, 8) = 0xff;
        Assert.AreEqual(0x01020304050607ffUL, Ꮡx.Value, "the store must reach the value");

        Ꮡx.Value = 0x1112131415161718UL;
        Assert.AreEqual((byte)0x17, @unsafe.Pointer.ElementRef<byte>(p, 1, 8), "the location reads the value as it is now");
        GC.KeepAlive(Ꮡx);
    }

    /// <summary>
    /// A struct of scalars viewed as four uint32: sqlite's PRNG state is this shape. And the value is all the
    /// storage there is: an index inside N and past the value's end is refused by name.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationInAStructOfScalarsReachesItsFieldsAndEndsWithIt()
    {
        ref Pair pair = ref heap(new Pair { a = 1, b = 2, c = 3 }, out ж<Pair> Ꮡpair);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡpair);

        @unsafe.Pointer.ElementRef<uint>(p, 1, 4) = 5;
        Assert.AreEqual(5u, pair.b);
        Assert.AreEqual(3u, @unsafe.Pointer.ElementRef<uint>(p, 2, 4), "the low half of c");

        string text = PanicText(() => @unsafe.Pointer.ElementRef<uint>(p, 4, 1L << 20) = 1);

        StringAssert.Contains(text, "(*[1048576]uint32)(p)[4]");
        StringAssert.Contains(text, "would pass the end of the Go value p points to");
        GC.KeepAlive(Ꮡpair);
    }

    /// <summary>
    /// An interior address of Go memory through unsafe.Add: <c>(*[8]byte)(unsafe.Add(unsafe.Pointer(&amp;buf[0]),
    /// 4))[2]</c>. The registry does not know the address; the pointer carries what it was derived from, which
    /// answers for where the storage ends.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationAtAnAddressInsideAGoValueIsInsideTheValue()
    {
        array<byte> buf = new(16);
        ж<byte> first = Ꮡ(buf, 0);
        @unsafe.Pointer p = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(first), 4);

        @unsafe.Pointer.ElementRef<byte>(p, 2, 8) = 3;
        Assert.AreEqual((byte)3, buf[6]);

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.ElementRef<byte>(p, 12, RawMem) = 1), "would pass the end of the Go value p points to");
        GC.KeepAlive(first);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<byte> element, WeakReference value) AnElementOfADroppedValue()
    {
        heap(0x0102030405060708UL, out ж<ulong> Ꮡx);

        return (@unsafe.Pointer.Element<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡx), 1, 8), new WeakReference(Ꮡx));
    }

    /// <summary>
    /// <c>e := &amp;(*[8]byte)(unsafe.Pointer(&amp;x))[1]</c> and nothing else holds x: Go's pointer keeps x alive.
    /// The element pointer keeps the box that holds the value still.
    /// </summary>
    [TestMethod]
    public void TheAddressOfAnElementOfAGoValueKeepsTheValue()
    {
        (ж<byte> element, WeakReference value) = AnElementOfADroppedValue();

        Collect();

        Assert.IsTrue(value.IsAlive, "the value's box is collected while a pointer to one of its bytes is alive");
        Assert.AreEqual((byte)0x07, element.Value);

        element.Value = 0x77;
        Assert.AreEqual(0x0102030405067708UL, ((ж<ulong>)value.Target!).Value, "and a write through it reaches the value");
    }

    /// <summary>
    /// The array forms over a Go value of another type: eight bytes stored into a uint64, and loaded from it.
    /// </summary>
    [TestMethod]
    public void AnArrayStoredAndLoadedThroughAGoValueOfAnotherTypeIsItsBytes()
    {
        heap(0UL, out ж<ulong> Ꮡx);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡx);

        @unsafe.Pointer.StoreArray<byte>(p, 8, new array<byte>(new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }));
        Assert.AreEqual(0x0102030405060708UL, Ꮡx.Value);

        array<ushort> loaded = @unsafe.Pointer.LoadArray<ushort>(p, 4);

        Assert.AreEqual((ushort)0x0708, loaded[0]);
        Assert.AreEqual((ushort)0x0102, loaded[3]);

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.StoreArray<byte>(p, 9, new array<byte>(9))), "would pass the end of the Go value p points to");
        GC.KeepAlive(Ꮡx);
    }

    // ---- V4: a Go value nothing can alias ----

    /// <summary>
    /// A Go value that holds a reference has no address: its pointer's number is an order token. The array store
    /// would land in a copy, so it is refused by name; the load reads through the box, which refuses a token by name.
    /// </summary>
    [TestMethod]
    public void TheArrayFormsOverAGoValueThatHoldsAReferenceAreRefusedByName()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡb);

        string store = PanicText(() => @unsafe.Pointer.StoreArray<long>(p, 3, new array<long>(3)));

        StringAssert.Contains(store, "*(*[3]int64)(p) = v cannot write the Go value p points to");
        Assert.AreEqual(7L, Ꮡb.Value.n, "a refused store writes nothing");

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.LoadArray<long>(p, 3)), "with no address");
        GC.KeepAlive(Ꮡb);
    }

    /// <summary>
    /// The index form over such a value is Go's own definition, <c>(*E)(unsafe.Add(p, i*size))</c>, which the pointer
    /// conversion answers: the field at that Go offset where there is one of the element's type.
    /// </summary>
    [TestMethod]
    public void AnIndexedLocationInAGoValueThatHoldsAReferenceIsThePointerConversionsAnswer()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡb);

        // struct{ name string; n int64 }: n is at Go offset 16, which is element 2 of three int64.
        Assert.AreEqual(7L, @unsafe.Pointer.ElementRef<long>(p, 2, 3));

        @unsafe.Pointer.ElementRef<long>(p, 2, 3) = 9;
        Assert.AreEqual(9L, Ꮡb.Value.n, "the store reaches the field");
        GC.KeepAlive(Ꮡb);
    }

    // ---- Go's panics, and the refusals ----

    /// <summary>
    /// Indexing through a nil array pointer is Go's nil dereference, in all four forms, before the index is read.
    /// </summary>
    [TestMethod]
    public void ANilArrayPointerIsANilDereference()
    {
        const string nilDereference = "invalid memory address or nil pointer dereference";
        @unsafe.Pointer nothing = @unsafe.Pointer.Nil;

        StringAssert.Contains(PanicText(() => @unsafe.Pointer.ElementRef<byte>(nothing, 9, 4) = 1), nilDereference);
        StringAssert.Contains(PanicText(() => @unsafe.Pointer.Element<byte>(nothing, 0, 4)), nilDereference);
        StringAssert.Contains(PanicText(() => @unsafe.Pointer.Element<byte>(null, 0, 4)), nilDereference);
        StringAssert.Contains(PanicText(() => @unsafe.Pointer.LoadArray<byte>(nothing, 4)), nilDereference);
        StringAssert.Contains(PanicText(() => @unsafe.Pointer.StoreArray<byte>(nothing, 4, new array<byte>(4))), nilDereference);
    }

    /// <summary>
    /// The index is checked against N with Go's texts, over C memory and before any memory is touched.
    /// </summary>
    [TestMethod]
    public void TheIndexPanicsWithGosTexts()
    {
        nint block = Filled("XXXXXXXX");

        try
        {
            @unsafe.Pointer p = Pointer(block);

            Assert.AreEqual("runtime error: index out of range [4] with length 4", PanicText(() => @unsafe.Pointer.ElementRef<byte>(p, 4, 4) = 1));
            Assert.AreEqual("runtime error: index out of range [-1]", PanicText(() => @unsafe.Pointer.ElementRef<byte>(p, -1, 4) = 1));
            Assert.AreEqual("runtime error: index out of range [4] with length 4", PanicText(() => @unsafe.Pointer.Element<byte>(p, 4, 4)));
            Assert.AreEqual("XXXXXXXX", Peek(block, 8), "a refused index writes nothing");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// The converter refuses an element that holds references at the site; the doors are public, so they refuse it
    /// by name too.
    /// </summary>
    [TestMethod]
    public void AnElementThatHoldsReferencesIsRefusedByName()
    {
        nint block = Filled("");

        try
        {
            StringAssert.Contains(PanicText(() => @unsafe.Pointer.Element<@string>(Pointer(block), 0, 2)), "an element that holds references cannot be viewed at an address");
            StringAssert.Contains(PanicText(() => @unsafe.Pointer.ElementRef<@string>(Pointer(block), 0, 2) = default), "an element that holds references cannot be viewed at an address");
            StringAssert.Contains(PanicText(() => @unsafe.Pointer.LoadArray<@string>(Pointer(block), 2)), "an element that holds references cannot be viewed at an address");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}
