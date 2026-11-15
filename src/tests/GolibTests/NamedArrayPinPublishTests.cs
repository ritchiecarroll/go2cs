using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// A write at the ADDRESS of a named array reaches the array, also when nothing has used the array yet.
/// </summary>
/// <remarks>
/// <para>
/// The wrapper go2cs-gen writes for <c>type small [N]E</c> holds one field, a holder it fills on the first use of
/// its array. A pointer box's two pin paths (its conversion to a number and to a raw pointer) read the box's value
/// as an <see cref="IArray"/>, which for a value-type wrapper is a boxed COPY, and pinned that copy's
/// <see cref="IArray.Source"/>. On a wrapper nothing had used, the copy filled a backing of its own: the address
/// handed out was of an array the variable never reads, and a write there was lost without a panic. Go prints the
/// value written.
/// </para>
/// <para>
/// The element pointer has had a gate for the same class since August (<c>arrayView</c> and its
/// <c>publishArrayBacking</c>): fill on the copy, then copy the wrapper back over the real storage under the
/// box's lock. The pin paths now go through that publish before they pin.
/// </para>
/// <para>
/// One arm per shape the write was lost in, each as the converter spells it: a local, a package variable by
/// number, a field of a struct, <c>new(small)</c> by number, libc's <c>memcpy</c> destination, and the raw-pointer
/// conversion. Each read after the write goes through the variable, never through the pointer the write used.
/// The two controls were right before the change: a named array used before its address is taken, and an
/// unnamed array.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NamedArrayPinPublishTests
{
    private const long RawMem = 1L << 20;

    private struct Holder
    {
        internal int n;
        internal FloorSmallMem buf;

        internal static ref FloorSmallMem Ꮡbuf(ref Holder instance) => ref instance.buf;
    }

    // A package variable, as the converter declares one. One per arm that needs it: "never used" is spent by the
    // first address taken.
    private static readonly ж<FloorSmallMem> ᏑglobalByNumber = new StaticBox<FloorSmallMem>(default(FloorSmallMem));

    private static ref FloorSmallMem globalByNumber => ref ((StaticBox<FloorSmallMem>)ᏑglobalByNumber).ValueSlot;

    // `*(*byte)(p) = b`, a write at an address as a system call or libc's C ABI makes it.
    private static void fill(@unsafe.Pointer p, byte b) => @unsafe.Pointer.To<byte>(p).Value = b;

    private static void fillNumber(uintptr p, byte b) => @unsafe.Pointer.To<byte>((@unsafe.Pointer)p).Value = b;

    // libc's Xmemcpy: `copy((*RawMem)(unsafe.Pointer(dest))[:n:n], (*RawMem)(unsafe.Pointer(src))[:n:n])`.
    private static void memcpy(uintptr dest, uintptr src, nint n) =>
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, n, n, RawMem), @unsafe.Pointer.View<byte>((@unsafe.Pointer)src, 0, n, n, RawMem));

    /// <summary><c>var local small; fill(unsafe.Pointer(&amp;local), 41)</c>.</summary>
    [TestMethod]
    public void AWriteAtTheAddressOfALocalNamedArrayNothingHasUsedReachesIt()
    {
        ref FloorSmallMem local = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡlocal);

        fill(@unsafe.Pointer.FromPinnedBox(Ꮡlocal), 41);

        Assert.AreEqual((byte)41, local[0], "the write landed in a backing the variable does not read");
        GC.KeepAlive(Ꮡlocal);
    }

    /// <summary><c>fillNumber(uintptr(unsafe.Pointer(&amp;global)), 42)</c>, a package variable.</summary>
    [TestMethod]
    public void AWriteAtTheNumberOfAPackageNamedArrayNothingHasUsedReachesIt()
    {
        fillNumber((uintptr)ᏑglobalByNumber, 42);

        Assert.AreEqual((byte)42, globalByNumber[0], "the write landed in a backing the variable does not read");
    }

    /// <summary><c>h := &amp;holder{}; fill(unsafe.Pointer(&amp;h.buf), 43)</c>, a named array field.</summary>
    [TestMethod]
    public void AWriteAtTheAddressOfANamedArrayFieldNothingHasUsedReachesIt()
    {
        ж<Holder> h = Ꮡ(new Holder());

        fill(@unsafe.Pointer.FromPinnedBox(h.of(Holder.Ꮡbuf)), 43);

        Assert.AreEqual((byte)43, h.Value.buf[0], "the write landed in a backing the field does not read");
        GC.KeepAlive(h);
    }

    /// <summary><c>x := new(small); fillNumber(uintptr(unsafe.Pointer(x)), 44)</c>.</summary>
    [TestMethod]
    public void AWriteAtTheNumberOfANewNamedArrayReachesIt()
    {
        ж<FloorSmallMem> x = @new<FloorSmallMem>();

        fillNumber((uintptr)x, 44);

        Assert.AreEqual((byte)44, x.Value[0], "the write landed in a backing the allocation does not read");
        GC.KeepAlive(x);
    }

    /// <summary>libc's <c>memcpy</c> into a named array nothing has used, from an unnamed array.</summary>
    [TestMethod]
    public void MemcpyIntoANamedArrayNothingHasUsedReachesIt()
    {
        ref FloorSmallMem dst = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡdst);
        ref array<byte> src = ref heap<array<byte>>(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }.array(), out ж<array<byte>> Ꮡsrc);

        memcpy((uintptr)Ꮡdst, (uintptr)Ꮡsrc, 8);

        for (nint i = 0; i < 8; i++)
            Assert.AreEqual((byte)(i + 1), dst[i], $"byte {i} of the destination");

        GC.KeepAlive(Ꮡdst);
        GC.KeepAlive(Ꮡsrc);
    }

    /// <summary>The raw-pointer conversion, the second pin path: the address is of the array the variable reads.</summary>
    [TestMethod]
    public void TheRawPointerOfANamedArrayNothingHasUsedIsOfTheArrayTheVariableReads()
    {
        ref FloorSmallMem local = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡlocal);
        byte* data = (byte*)(void*)Ꮡlocal;

        data[0] = 47;
        data[63] = 48;

        Assert.AreEqual((byte)47, local[0], "the write landed in a backing the variable does not read");
        Assert.AreEqual((byte)48, local[63]);
        GC.KeepAlive(Ꮡlocal);
    }

    /// <summary>
    /// The number is taken once and the pin stays: a second conversion answers the same address, and a write to
    /// the variable shows at it.
    /// </summary>
    [TestMethod]
    public void TheNumberOfANamedArrayNothingHasUsedIsStableAndShowsLaterWrites()
    {
        ref FloorSmallMem local = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡlocal);
        uintptr first = Ꮡlocal;

        local[5] = 9;

        uintptr second = Ꮡlocal;

        Assert.AreEqual(first, second, "the second conversion must answer the first one's address");
        Assert.AreEqual((byte)9, ((byte*)(nuint)first.Value)[5], "a write to the variable must show at its address");
        GC.KeepAlive(Ꮡlocal);
    }

    /// <summary>Control: a named array used before its address is taken was right before the change.</summary>
    [TestMethod]
    public void AWriteAtTheAddressOfANamedArrayUsedBeforeReachesItAndKeepsItsBytes()
    {
        ref FloorSmallMem used = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡused);

        used[7] = 9;
        fill(@unsafe.Pointer.FromPinnedBox(Ꮡused), 45);

        Assert.AreEqual((byte)45, used[0]);
        Assert.AreEqual((byte)9, used[7], "the pin must not replace the array the variable already had");
        GC.KeepAlive(Ꮡused);
    }

    /// <summary>Control: an unnamed array nothing has used was right before the change.</summary>
    [TestMethod]
    public void AWriteAtTheAddressOfAnUnnamedArrayNothingHasUsedReachesIt()
    {
        ref array<byte> plain = ref heap(new array<byte>(8), out ж<array<byte>> Ꮡplain);

        fill(@unsafe.Pointer.FromPinnedBox(Ꮡplain), 46);

        Assert.AreEqual((byte)46, plain[0]);
        GC.KeepAlive(Ꮡplain);
    }
}
