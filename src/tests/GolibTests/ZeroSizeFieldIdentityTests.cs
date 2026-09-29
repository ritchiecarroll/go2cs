using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// A17 (COORD ruling 2026-09-28): a NAMED Go zero-size field laid out readonly at Go's offset (the
/// zero-size-field layout arc) answers golib's shared per-type slot as its ref TARGET, while its pointer
/// IDENTITY stays (containing allocation, field): &amp;x.f != &amp;y.f for distinct x and y, as in Go (C2's Z2
/// field-stays-distinct rule). And golib's native-memory door refuses an element type whose C# size is not
/// Go's, instead of striding past Go's elements.
/// </summary>
[TestClass]
public class ZeroSizeFieldIdentityTests
{
    private struct noCopy
    {
    }

    // internal/runtime/atomic.Uint64's shape as the converter now emits it: Go's size (8), the named
    // zero-size field readonly at Go's offset 0, sharing it with the value, and go2cs-gen's accessor for
    // it answering the shared slot.
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private struct Carrier
    {
        [FieldOffset(0)] internal readonly noCopy marker;
        [FieldOffset(0)] internal ulong value;

        internal static ref noCopy Ꮡmarker(ref Carrier instance) => ref GoZeroSizeSlot<noCopy>.Ref;
        internal static ref ulong Ꮡvalue(ref Carrier instance) => ref instance.value;
    }

    // The same fields with no explicit layout: C# gives the empty struct a byte and pads the ulong to 8.
    private struct Diverging
    {
        internal noCopy marker;
        internal ulong value;
    }

    private struct Nests
    {
        internal Diverging inner;
    }

    [TestMethod]
    public void TheArcRestoresGosSize()
    {
        Assert.AreEqual(8, Unsafe.SizeOf<Carrier>(), "the explicit layout is Go's size");
        Assert.AreEqual(16, Unsafe.SizeOf<Diverging>(), "without it the zero-size field costs a padded byte");
    }

    [TestMethod]
    public void DistinctStructsZeroSizeFieldsAreDistinctPointers()
    {
        ж<Carrier> x = Ꮡ(new Carrier());
        ж<Carrier> y = Ꮡ(new Carrier());

        ж<noCopy> fx = x.of(Carrier.Ꮡmarker);
        ж<noCopy> fy = y.of(Carrier.Ꮡmarker);

        // The TARGET is shared ...
        Assert.IsTrue(Unsafe.AreSame(ref fx.Value, ref fy.Value), "both refs land on the shared zero-size slot");

        // ... the IDENTITY is not.
        Assert.IsFalse(fx == fy, "&x.marker != &y.marker for distinct x and y");
        Assert.IsFalse(fx.Equals(fy));
        Assert.AreNotEqual(((uintptr)fx).Value, ((uintptr)fy).Value, "their uintptrs differ: base + offset, never the slot");

        HashSet<ж<noCopy>> keys = [fx, fy];
        Assert.AreEqual(2, keys.Count, "as map keys they stay two");
    }

    [TestMethod]
    public void AByteElementViewedAsAZeroSizeTypeKeepsItsAddress()
    {
        // `(*SID)(unsafe.Pointer(&b[0]))` -- Windows' variable-length SID over a byte buffer, Go's opaque
        // zero-size struct. The REINTERPRET is an aliasing view of b[0]'s real storage, not a zero-size
        // FIELD, so its target is the buffer and its uintptr is b[0]'s address: the syscall door must pass
        // it. A17's token rule (a zero-size field's target is the shared slot) turned it into an order
        // token and the door refused CopySid at argument 1 -- os/user's TestLookupGroup family and
        // internal/syscall/windows' TestRunAtLowIntegrity at the TRAIN I union.
        slice<byte> b = new byte[16].slice();
        ж<byte> first = Ꮡ(b, 0);

        ж<noCopy> sid = first.Reinterpret<byte, noCopy>();
        uintptr address = (uintptr)sid;

        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(address), "a view over real storage is an ADDRESS, never an order token");
        Assert.AreEqual(((uintptr)first).Value, address.Value, "the view's address is the element's own");
        Assert.IsFalse(Unsafe.AreSame(ref sid.Value, ref GoZeroSizeSlot<noCopy>.Ref), "its target is the buffer, not the shared zero-size slot");
    }

    [TestMethod]
    public void OneFieldTakenTwiceIsOnePointer()
    {
        ж<Carrier> x = Ꮡ(new Carrier());

        ж<noCopy> first = x.of(Carrier.Ꮡmarker);
        ж<noCopy> second = x.of(Carrier.Ꮡmarker);

        Assert.IsTrue(first == second);
        Assert.AreEqual(((uintptr)first).Value, ((uintptr)second).Value);
    }

    [TestMethod]
    public void AWriteThroughTheZeroSizeFieldNeverReachesItsNeighbour()
    {
        ж<Carrier> x = Ꮡ(new Carrier());
        x.of(Carrier.Ꮡvalue).Value = 0x0102030405060708UL;

        // Go stores nothing for a zero-size write; the slot absorbs C#'s one byte, the field's bytes stay put.
        x.of(Carrier.Ꮡmarker).Value = default;

        Assert.AreEqual(0x0102030405060708UL, x.Value.value, "the value sharing the zero-size field's offset is untouched");
    }

    [TestMethod]
    public void TheNativeDoorRefusesAStrideThatIsNotGos()
    {
        nint block = Marshal.AllocHGlobal(64);

        try
        {
            Assert.ThrowsException<PanicException>(() => go.slice<Diverging>.OverNativeMemory((nuint)block, 2), "16 bytes in C#, 8 in Go");
            Assert.ThrowsException<PanicException>(() => go.slice<Nests>.OverNativeMemory((nuint)block, 2), "a struct nesting a diverging one diverges");
            Assert.ThrowsException<PanicException>(() => go.slice<noCopy>.OverNativeMemory((nuint)block, 2), "a zero-size element: Go's stride is 0");

            go.slice<Carrier> view = go.slice<Carrier>.OverNativeMemory((nuint)block, 8);
            view[1].value = 42;
            Assert.AreEqual(42L, Marshal.ReadInt64(block, 8), "the arc's layout strides at Go's 8 bytes");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}
