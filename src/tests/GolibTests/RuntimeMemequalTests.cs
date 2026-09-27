using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// runtime.memequal (src/core/runtime/memmove_impl.cs) through a slice header's array word, the pointer
// shape RuntimeMemmoveTests' MemclrThroughASliceHeaderClearsTheSliceAfterTheCollectorMovesIt reads for
// memclr. That word is golib's FromBox pointer: a TRANSIENT address (the element's address under a
// `fixed` that has ended) that RETAINS the element box. A compacting collection between the header
// read and the compare moves the backing, and a compare through the stale numbers reads where the
// backing used to be instead of the slice.
[TestClass]
public class RuntimeMemequalTests
{
    // runtime's own slice header, as (*slice)(unsafe.Pointer(&b)) reads it.
    private struct SliceHeaderShape
    {
        public unsafe_package.Pointer array;
        public nint len;
        public nint cap;
    }

    private static unsafe_package.Pointer ArrayWordOf(slice<byte> window)
    {
        heap(window, out ж<slice<byte>> Ꮡw);

        return Ꮡw.Reinterpret<slice<byte>, SliceHeaderShape>().Value.array;
    }

    private static void Write(byte[] backing, int at, int first)
    {
        for (int i = 0; i < 8; i++)
            backing[at + i] = (byte)(first + i);
    }

    [TestMethod]
    public void MemequalThroughSliceHeadersComparesTheSlicesAfterTheCollectorMovesThem()
    {
        // Four 8-byte windows of ONE backing, so every stale number is displaced by the same move and a
        // compare through them reads ONE snapshot of the old location. The two pairs are built so that
        // the answer FLIPS after the move: (a, b) is equal before and unequal after, (c, d) the reverse.
        // Whatever that snapshot holds (the old bytes left in place, zeroes, or one foreign object's
        // bytes), a compare that reads it answers at least one pair as it stood before the move.
        byte[] backing = new byte[64];
        backing.AsSpan().Fill(0xee);
        Write(backing, 8, 1);
        Write(backing, 24, 1);
        Write(backing, 40, 1);
        Write(backing, 48, 9);

        slice<byte> mem = new(backing);
        unsafe_package.Pointer a = ArrayWordOf(mem[8..16]);
        unsafe_package.Pointer b = ArrayWordOf(mem[24..32]);
        unsafe_package.Pointer c = ArrayWordOf(mem[40..48]);
        unsafe_package.Pointer d = ArrayWordOf(mem[48..56]);

        // CONTROL, before any collection: the words are current, so both arms of memequal agree with the
        // bytes, and the arm below is not vacuous.
        Assert.IsTrue(runtime_package.GoMemequal(a, b, new uintptr(8)), "before the move: a == b");
        Assert.IsFalse(runtime_package.GoMemequal(c, d, new uintptr(8)), "before the move: c != d");

        nuint minted = a.Value.Value;
        bool moved = false;

        for (int attempt = 0; attempt < 20 && !moved; attempt++)
        {
            GC.KeepAlive(new byte[4096]);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            moved = unsafe_package.Pointer.FromBox(new ElemRefBox<byte>(new slice<byte>(backing), 8)).Value.Value != minted;
        }

        if (!moved)
            Assert.Inconclusive("the collector did not move the backing, so this run cannot tell a stale address from a live one");

        // The live slices change AFTER the move; the old location does not see it.
        Write(backing, 8, 101);
        Write(backing, 48, 1);

        Assert.IsFalse(runtime_package.GoMemequal(a, b, new uintptr(8)), "after the move: a (101..108) != b (1..8)");
        Assert.IsTrue(runtime_package.GoMemequal(c, d, new uintptr(8)), "after the move: c (1..8) == d (1..8)");
    }
}
