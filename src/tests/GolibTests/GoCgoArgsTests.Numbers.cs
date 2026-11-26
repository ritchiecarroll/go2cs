using System;
using System.Runtime.CompilerServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo Phase 3, S4 (docs/phase4/DESIGN-cgo-phase3-marshaling.md 20.4, P3's conditions, COORD's ruling on Q3): a NUMBER of
// Go memory handed to a void pointer, `unsafe.Pointer(uintptr(unsafe.Pointer(&x)))` in one expression or through a
// variable, crosses as the pin of the storage the record of its mint names. Four rows (P3): a live box, an offset
// inside, an offset past the end, a dead box. And a pointer-sized integer that is a token is refused by name
// (GoCgoArgs.Word, the uintptr_t parameter).
public unsafe partial class GoCgoArgsTests
{
    // A live box's exact number: the pin names the box's own bytes, and a write through it is what Go then reads.
    [TestMethod]
    public void S4_ALiveBoxsNumberCrossesAsItsOwnBytes()
    {
        ж<int32> scalar = new StandardBox<int32>(7);
        uintptr number = (uintptr)scalar;

        GoCgoPin pin = GoCgoArgs.PinVoidPointer(new @unsafe.Pointer(number), "fill");
        Assert.IsTrue(pin.IsGoMemory, "the number crosses as Go memory, pinned");

        fixed (byte* pinned = pin)
        {
            Assert.AreEqual((nint)number.Value, pin.Address(pinned), "the address C receives is the box's own slot");
            *(int*)pin.Address(pinned) = 42;
        }

        Assert.AreEqual((int32)42, scalar.Value, "C's write lands in the box");
        GC.KeepAlive(scalar);
    }

    // An offset inside the storage: `uintptr(unsafe.Pointer(&a)) + 4` names a's second element, bounded by a's storage.
    [TestMethod]
    public void S4_AnOffsetInsideTheStorageCrossesAtThatOffset()
    {
        ж<array<int32>> fixedArray = new StandardBox<array<int32>>(new array<int32>(8));
        nuint data = ((uintptr)fixedArray).Value;

        GoCgoPin pin = GoCgoArgs.PinVoidPointer(new @unsafe.Pointer((uintptr)(data + 4)), "fill");
        Assert.IsTrue(pin.IsGoMemory);

        fixed (byte* pinned = pin)
        {
            Assert.AreEqual((nint)(data + 4), pin.Address(pinned));
            *(int*)pin.Address(pinned) = 9;
        }

        Assert.AreEqual((int32)9, fixedArray.Value[1], "the write lands in the element at the offset");
        GC.KeepAlive(fixedArray);
    }

    // Past the end: one past the storage is the end cursor of a begin/end pair and crosses as its number, pinning nothing
    // (the pin-extent index is end-exclusive, docs/phase4/DESIGN-cgo-pin-extents.md). It is never pinned as Go memory.
    [TestMethod]
    public void S4_AnOffsetPastTheEndPinsNothing()
    {
        ж<array<int32>> fixedArray = new StandardBox<array<int32>>(new array<int32>(8));
        nuint data = ((uintptr)fixedArray).Value;

        GoCgoPin pin = GoCgoArgs.PinVoidPointer(new @unsafe.Pointer((uintptr)(data + 32)), "fill");
        Assert.IsFalse(pin.IsGoMemory, "one past the end is not pinned as Go memory");

        fixed (byte* pinned = pin)
            Assert.AreEqual((nint)(data + 32), pin.Address(pinned), "it crosses as its number");

        GC.KeepAlive(fixedArray);
    }

    // P3's SHOULD 3: the commonest number form, a FIELD, `uintptr(unsafe.Pointer(&s)) + unsafe.Offsetof(s.f)`, on a
    // reference-free struct in its own box. The box's conversion pins its one-element slot and registers the pin (the
    // pin-extent index holds it), so the interior number crosses at the field, held still, and C's write lands there.
    [TestMethod]
    public void S4_AStructFieldsNumberCrossesAtTheField()
    {
        ж<cgoPoint> point = new StandardBox<cgoPoint>(default(cgoPoint));
        nuint start = ((uintptr)point).Value;
        nuint field = start + 4; // Offsetof(cgoPoint.Y): two int32s

        GoCgoPin pin = GoCgoArgs.PinVoidPointer(new @unsafe.Pointer((uintptr)field), "fill");
        Assert.IsTrue(pin.IsGoMemory, "the field's number crosses as Go memory, pinned, never as a raw managed address");

        fixed (byte* pinned = pin)
        {
            Assert.AreEqual((nint)field, pin.Address(pinned));
            *(int*)pin.Address(pinned) = 5;
        }

        Assert.AreEqual((int32)5, point.Value.Y, "C's write lands in the field");
        GC.KeepAlive(point);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint NumberOfADroppedBox()
    {
        ж<int32> scalar = new StandardBox<int32>(7);
        return ((uintptr)scalar).Value;
    }

    // A dead box: the number's entry outlives its box until the registry next looks, and the door refuses it by name.
    [TestMethod]
    public void S4_ADeadBoxsNumberIsRefusedByName()
    {
        nuint number = NumberOfADroppedBox();

        for (int i = 0; i < 3; i++)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.PinVoidPointer(new @unsafe.Pointer((uintptr)number), "fill")), "has been collected");
    }

    // The uintptr_t parameter: a token is go2cs's name for a Go value with no address, so C would receive garbage, and the
    // call is refused by name. A real address and an ordinary integer cross as they are.
    [TestMethod]
    public void S4_APointerSizedIntegerThatIsATokenIsRefusedByName()
    {
        ж<cgoHolder> references = new StandardBox<cgoHolder>(default(cgoHolder));
        ulong token = ((uintptr)references).Value;
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken((nuint)token), "the premise: a box holding references gives a token");

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.Word(token, "word")), "token for a Go value that has no address");

        ж<int32> scalar = new StandardBox<int32>(7);
        ulong address = ((uintptr)scalar).Value;
        Assert.AreEqual(address, GoCgoArgs.Word(address, "word"), "a real address crosses as an integer");
        Assert.AreEqual(42UL, GoCgoArgs.Word(42, "word"));
        Assert.AreEqual(ulong.MaxValue, GoCgoArgs.Word(ulong.MaxValue, "word"), "(uintptr_t)-1 crosses");

        // P3's MUST: a plain integer with a token's bit pattern (bit 63 set, bit 47 clear), never minted, crosses as Go
        // passes it: a hash or an opaque id.
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken((nuint)0x8000000000000001UL), "the premise: the pattern matches");
        Assert.AreEqual(0x8000000000000001UL, GoCgoArgs.Word(0x8000000000000001UL, "word"), "an unminted tagged integer crosses");

        GC.KeepAlive(references);
        GC.KeepAlive(scalar);
    }
}
