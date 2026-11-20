using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// Door C, the unnamed form (<c>docs/phase4/DESIGN-array-pointer-doors.md</c> section 20, decision D5): Go's
/// <c>(*[N]E)(p)</c> kept in a local, passed or returned, emitted <c>@unsafe.Pointer.ToArray&lt;E&gt;(p, N)</c>.
/// </summary>
/// <remarks>
/// <para>
/// The door is the pointer conversion <c>To&lt;array&lt;E&gt;&gt;(p)</c> with a length. It runs the number conversion's own
/// arm order (<c>ж&lt;T&gt;.FromNumber</c>) and replaces only what stood at arm 5: the refusal of an array view over a number
/// no Go value owns, and the fall-through to a native box over a pinned real address of another pointee (arm 2b). Each
/// arm below is one place the door must answer what To answers, or the one place it must not.
/// </para>
/// <para>
/// The first member is modernc.org/sqlite's page-cache binding, <c>f := (*[2]uintptr)(unsafe.Pointer(stub))</c> over
/// <c>libc.Xcalloc</c> memory, then <c>f[0], f[1] = a, b</c>: refused by name through To, a window here.
/// </para>
/// </remarks>
[TestClass]
public class KeptArrayDoorTests
{
    private const int NativeBytes = 64;

    // struct{ a, b uint32; c uint64 }
    [StructLayout(LayoutKind.Sequential)]
    private struct Pair
    {
        internal uint a;
        internal uint b;
        internal ulong c;
    }

    // struct{ name string; n int64 }: it holds a reference, so its pointer's number is an order token, not an address.
    private struct Bearing
    {
        internal @string name;
        internal long n;
    }

    private static @unsafe.Pointer Pointer(nint address) => (@unsafe.Pointer)(uintptr)(nuint)address;

    private static nint Zeroed()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        for (int i = 0; i < NativeBytes; i++)
            Marshal.WriteByte(block, i, 0);

        return block;
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

    /// <summary>
    /// The sqlite binding itself: <c>f := (*[2]uintptr)(unsafe.Pointer(stub)); f[0], f[1] = a, b</c> over C memory.
    /// The pointer conversion refuses it by name; the door's window takes the tuple store, element by element, in
    /// place.
    /// </summary>
    [TestMethod]
    public void AKeptArrayPointerOverCMemoryIsAWindowThatTakesATupleStore()
    {
        nint block = Zeroed();

        try
        {
            string refused = PanicText(() => @unsafe.Pointer.To<array<uintptr>>(Pointer(block)));

            StringAssert.Contains(refused, "cannot view native memory as array<uintptr>", "the pointer conversion has no N, so over C memory it refuses by name");

            ж<array<uintptr>> f = @unsafe.Pointer.ToArray<uintptr>(Pointer(block), 2);

            // The emitted shape: a tuple store into two ref-returning element reads.
            (f.ElementRef(0), f.ElementRef(1)) = ((uintptr)0x1111, (uintptr)0x2222);

            Assert.AreEqual(0x1111L, Marshal.ReadInt64(block, 0), "f[0] is stored in place");
            Assert.AreEqual(0x2222L, Marshal.ReadInt64(block, 8), "f[1] is stored in place");
            Assert.AreEqual(0L, Marshal.ReadInt64(block, 16), "nothing past N is written");

            Marshal.WriteInt64(block, 8, 0x3333);
            Assert.AreEqual((uintptr)0x3333, f.ElementRef(1), "a read sees the memory as it is now");
            Assert.AreEqual((nint)block, (nint)(nuint)(uintptr)f, "the pointer's number is the address");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// The window has N elements: an index at N is Go's bounds panic, and nothing is written.
    /// </summary>
    [TestMethod]
    public void AnIndexAtNIsABoundsPanic()
    {
        nint block = Zeroed();

        try
        {
            ж<array<uintptr>> f = @unsafe.Pointer.ToArray<uintptr>(Pointer(block), 2);

            Assert.ThrowsException<PanicException>(() => f.ElementRef(2) = 7, "an index at N is out of range");
            Assert.AreEqual(0L, Marshal.ReadInt64(block, 16), "a refused store writes nothing");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// A nil unsafe.Pointer is a nil array pointer, through the door as through To.
    /// </summary>
    [TestMethod]
    public void ANilPointerIsANilArrayPointer()
    {
        ж<array<uintptr>> f = @unsafe.Pointer.ToArray<uintptr>((@unsafe.Pointer)default(NilType), 2);

        Assert.IsTrue(f == nil, "the door over a nil unsafe.Pointer is a nil pointer");
    }

    /// <summary>
    /// A pointer whose source is itself a pointer to the array type is that pointer, as To answers.
    /// </summary>
    [TestMethod]
    public void APointerToTheArrayTypeIsThatPointer()
    {
        heap(new array<uint>(4), out ж<array<uint>> Ꮡa);

        ж<array<uint>> f = @unsafe.Pointer.ToArray<uint>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 4);

        Assert.AreSame(Ꮡa, f, "the same-type alias answers the source box");
        GC.KeepAlive(Ꮡa);
    }

    /// <summary>
    /// Arm 2b: a pinned real address of a Go value of another pointee. To fell through to a native box of array&lt;E&gt;
    /// whose Value read element bytes as an array header; the door's window reads and writes the value's own bytes.
    /// </summary>
    [TestMethod]
    public void APinnedAddressOfAnotherPointeeIsAWindowOnItsBytes()
    {
        array<ulong> buffer = new(4);
        ж<array<ulong>> pinned = new StandardBox<array<ulong>>(buffer);

        // Taking the number pins the storage and registers it, so the number resolves to a box of another pointee.
        uintptr address = (uintptr)pinned;

        ж<array<uint>> f = @unsafe.Pointer.ToArray<uint>((@unsafe.Pointer)address, 8);

        f.ElementRef(1) = 0xAABBCCDD;
        f.ElementRef(2) = 0x11223344;

        Assert.AreEqual(0xAABBCCDD00000000UL, pinned.Value[0], "element 1 is the high half of the first uint64");
        Assert.AreEqual(0x11223344UL, pinned.Value[1], "element 2 is the low half of the second uint64");
        GC.KeepAlive(pinned);
    }

    /// <summary>
    /// MUST 1 of the review: an address inside a Go value, its source dropped by everything but the pointer. The
    /// window keeps the root the native box would have kept, so a collection neither frees nor moves the value, and
    /// a write after it reaches the value's bytes.
    /// </summary>
    [TestMethod]
    public void AnAddressInsideAGoValueKeepsTheValueAliveAcrossACollection()
    {
        // The pointer and the box are dropped inside the helper: after it returns only the window can keep the value.
        (ж<array<uint>> f, WeakReference source) = AWindowInsideAPairNothingElseHolds();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(source.IsAlive, "the window keeps the Go value it is a window on");

        f.ElementRef(1) = 0xEE;

        ж<Pair> Ꮡpair = (ж<Pair>)source.Target!;

        Assert.AreEqual(2u, f.ElementRef(0), "element 0 is field b, read after the collection");
        Assert.AreEqual(0x01020304000000EEUL, Ꮡpair.Value.c, "a write after the collection reaches the value");
        GC.KeepAlive(f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<array<uint>>, WeakReference) AWindowInsideAPairNothingElseHolds()
    {
        heap(new Pair { a = 1, b = 2, c = 0x0102030405060708UL }, out ж<Pair> Ꮡpair);

        return (@unsafe.Pointer.ToArray<uint>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡpair), 4), 3), new WeakReference(Ꮡpair));
    }

    /// <summary>
    /// MUST 2 of the review: token arithmetic keeps its refusal. An offset from an order token names no address, and
    /// the door does not open a window over it.
    /// </summary>
    [TestMethod]
    public void TokenArithmeticThroughTheDoorIsRefusedByName()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);
        @unsafe.Pointer moved = @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡb), 4);

        string viaTo = PanicText(() => @unsafe.Pointer.To<array<long>>(moved));
        string viaDoor = PanicText(() => @unsafe.Pointer.ToArray<long>(moved, 2));

        StringAssert.Contains(viaDoor, "unsafe pointer arithmetic on a managed pointer with no address");
        Assert.AreEqual(viaTo, viaDoor, "the door refuses what To refuses, by the same name");
        Assert.AreEqual(7L, Ꮡb.Value.n);
        GC.KeepAlive(Ꮡb);
    }

    /// <summary>
    /// MUST 2 of the review: an order token is not an address. Through the door it answers what To answers, and an
    /// element read refuses by the same name, rather than a window over the token's number.
    /// </summary>
    [TestMethod]
    public void AnOrderTokenThroughTheDoorAnswersWhatToAnswers()
    {
        heap(new Bearing { name = "x", n = 7 }, out ж<Bearing> Ꮡb);
        @unsafe.Pointer p = @unsafe.Pointer.FromPinnedBox(Ꮡb);

        ж<array<long>> viaTo = @unsafe.Pointer.To<array<long>>(p);
        ж<array<long>> viaDoor = @unsafe.Pointer.ToArray<long>(p, 3);

        Assert.AreEqual(viaTo.GetType(), viaDoor.GetType(), "the same box kind");
        Assert.IsNotInstanceOfType(viaDoor, typeof(NativeArrayBox<long>), "no window over a token");
        Assert.AreEqual(PanicText(() => _ = viaTo.ElementRef(2)), PanicText(() => _ = viaDoor.ElementRef(2)), "the same refusal");
        GC.KeepAlive(Ꮡb);
    }

    /// <summary>
    /// An element that holds a reference is not this door's: over C memory it refuses by name, as To does.
    /// </summary>
    [TestMethod]
    public void AReferenceBearingElementKeepsThePointerConversion()
    {
        nint block = Zeroed();

        try
        {
            string viaTo = PanicText(() => @unsafe.Pointer.To<array<@string>>(Pointer(block)));
            string viaDoor = PanicText(() => @unsafe.Pointer.ToArray<@string>(Pointer(block), 2));

            Assert.AreEqual(viaTo, viaDoor, "the door passes no opener for a reference-bearing element");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}
