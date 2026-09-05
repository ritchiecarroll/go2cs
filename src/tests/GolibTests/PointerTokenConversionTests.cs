using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The Q44 token's CONVERSION side, pinned from the operators' own angle after the Q44 chain found a
// behavioral row red (PointerCastSliceRange, 2026-09-05): its `**(**[2]int64)(unsafe.Pointer(&ip))`
// reinterprets a box whose pointee is a POINTER -- reference-bearing storage, no pinnable slot -- so the
// address route of Reinterpret reaches `(ж<ж<array<int64>>>)(uintptr)` with the token of a
// `ж<ж<array<nint>>>`, and the conversion answers with a native box OVER THE TOKEN. That is the
// design's chosen form (DESIGN-managed-pointer-token.md; ReinterpretSourceRetentionTests asserts it
// for the boundary idiom): an unrepresentable reinterpret faults LOUDLY at a non-canonical address
// instead of punning a transient slot address that survives only until the collector moves it. The
// row's dereference was exactly such a pun, and the row is amended to the compile-shape guard it
// documents. Three arms: the loud form itself; a PROVENANCE entry of another pointee type, whose
// address is real and keeps reading the data; and the void* in-operator, the one door the token arm
// had left asymmetric (a token handed to native code and back must come back as its box).
[TestClass]
public class PointerTokenConversionTests
{
    private struct Pair
    {
        internal ulong A;
        internal ulong B;
    }

    private struct Twin
    {
        internal ulong X;
        internal ulong Y;
    }

    [TestMethod]
    public void ATokenOfAnotherPointeeTypeIsANativeBoxOverTheToken_TheLoudFormTheDesignChose()
    {
        ref Pair target = ref heap(new Pair { A = 7, B = 11 }, out ж<Pair> Ꮡtarget);
        ref ж<Pair> ip = ref heap<ж<Pair>>(out ж<ж<Pair>> Ꮡip); // a box of a POINTER: reference-bearing, no pinnable slot
        ip = Ꮡtarget;

        nuint number = (nuint)(uintptr)Ꮡip;
        Assert.AreEqual(Ꮡip.PointerOrderToken, number, "a reference-bearing box hands out its order token, not an address");
        Assert.AreSame(Ꮡip, (ж<ж<Pair>>)(uintptr)number, "the SAME pointee type resolves the token to its box");

        ж<ж<Twin>> other = (ж<ж<Twin>>)(uintptr)number; // the row's shape: (**Twin)(unsafe.Pointer(&ip))

        Assert.IsTrue(other.IsNative, "ANOTHER pointee type is the loud form: a native box over the token, never a pun through the slot");
        Assert.AreEqual(number, other.NativeAddress, "whose address IS the token, so a boundary wrapper resolving the number still recovers the source");
        GC.KeepAlive(ip);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void AProvenanceEntryOfAnotherTypeKeepsTheNativeRoute_TheAddressReadsTheData()
    {
        ref array<byte> bytes = ref heap(new array<byte>(4), out ж<array<byte>> Ꮡbytes);
        bytes[0] = 0xAB;

        nuint address = (nuint)(uintptr)Ꮡbytes; // the fixed array's pinned DATA address, registered as provenance
        Assert.AreNotEqual(Ꮡbytes.PointerOrderToken, address, "a pinnable box hands out an address, not its token");

        ж<byte> first = (ж<byte>)(uintptr)address; // (*byte)(unsafe.Pointer(&arr)): another pointee type over a REAL address

        Assert.IsTrue(first.IsNative, "a provenance entry's address is real: the native route stays");
        Assert.AreEqual<byte>(0xAB, first.Value, "and it reads the DATA the pin holds still, not the array struct");
        GC.KeepAlive(bytes);
    }

    [TestMethod]
    public unsafe void AVoidPointerRoundTripRecoversTheTokenedBox()
    {
        ref Pair target = ref heap(new Pair { A = 3 }, out ж<Pair> Ꮡtarget);
        ref ж<Pair> ip = ref heap<ж<Pair>>(out ж<ж<Pair>> Ꮡip);
        ip = Ꮡtarget;

        void* raw = Ꮡip;       // operator void*: the token (the Q44 arm)
        ж<ж<Pair>> back = raw; // the in-operator resolves it exactly as uintptr's does

        Assert.AreSame(Ꮡip, back, "a token handed to native code and back is its box, never a native box over the token");
        GC.KeepAlive(ip);
        GC.KeepAlive(target);
    }
}
