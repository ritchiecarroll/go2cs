using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// User arenas over managed allocations (runtime's arena_impl.cs). An arena allocation is a typed,
/// collector-tracked object; the arena keeps what it allocated alive until free; Clone copies what
/// the arena allocated (a pointer's object, a slice's backing, and a string over those bytes, which
/// aliases it) and returns anything else unchanged; an allocation over the chunk's max goes to the
/// heap, as in Go.
/// </summary>
[TestClass]
public class RuntimeUserArenaTests
{
    [TestMethod]
    public void AnArenaAllocationIsAZeroedPointerOfItsType()
    {
        object arena = GoUserArenaNew();
        object first = GoUserArenaAlloc(arena, typeof(long));
        object second = GoUserArenaAlloc(arena, typeof(long));

        Assert.IsInstanceOfType(first, typeof(ж<long>));
        Assert.AreNotSame(first, second, "two allocations are two objects");
        Assert.AreEqual(0L, ((ж<long>)first).Value);

        ((ж<long>)first).Value = 42;
        Assert.AreEqual(0L, ((ж<long>)second).Value);
        GoUserArenaFree(arena);
    }

    [TestMethod]
    public void ClonePointerCopiesAnArenaObjectAndReturnsAHeapOneUnchanged()
    {
        object arena = GoUserArenaNew();
        ж<long> inArena = (ж<long>)GoUserArenaAlloc(arena, typeof(long));
        inArena.Value = 7;

        object? copy = GoUserArenaClone(inArena);

        Assert.AreNotSame(inArena, copy, "an arena pointer is copied");
        Assert.AreEqual(7L, ((ж<long>)copy!).Value);

        ж<long> onHeap = new StandardBox<long>(9);
        Assert.AreSame(onHeap, GoUserArenaClone(onHeap), "a heap pointer is returned unchanged");
        GoUserArenaFree(arena);
    }

    [TestMethod]
    public void CloneCopiesAnArenaSliceItsSubSliceAndAStringOverItsBytes()
    {
        object arena = GoUserArenaNew();
        ж<slice<byte>> pointer = new StandardBox<slice<byte>>(default(slice<byte>));
        GoUserArenaSlice(arena, pointer, 4);
        slice<byte> b = pointer.Value;

        Assert.AreEqual((nint)4, b.Length);
        Assert.AreEqual((nint)4, b.Capacity);

        for (int i = 0; i < 4; i++)
            b[i] = (byte)('a' + i);

        slice<byte> copy = (slice<byte>)GoUserArenaClone(b)!;
        Assert.AreNotSame(b.Source, copy.Source);
        Assert.AreNotEqual(unsafe_package.SliceData(b), unsafe_package.SliceData(copy), "the slice is copied");
        CollectionAssert.AreEqual(b.ToArray(), copy.ToArray());

        slice<byte> sub = b[1..3];
        slice<byte> subCopy = (slice<byte>)GoUserArenaClone(sub)!;
        Assert.AreNotEqual(unsafe_package.SliceData(sub), unsafe_package.SliceData(subCopy), "a sub-slice is copied");
        CollectionAssert.AreEqual(new byte[] { (byte)'b', (byte)'c' }, subCopy.ToArray());

        @string s = unsafe_package.String(unsafe_package.SliceData(b), b.Length);
        @string sCopy = (@string)GoUserArenaClone(s)!;
        Assert.AreNotEqual(unsafe_package.StringData(s), unsafe_package.StringData(sCopy), "a string over arena bytes is copied");
        Assert.AreEqual("abcd", sCopy.ToString());

        @string heap = s + s;
        Assert.AreEqual(unsafe_package.StringData(heap), unsafe_package.StringData((@string)GoUserArenaClone(heap)!), "a heap string is returned unchanged");

        slice<byte> heapSlice = new byte[] { 1, 2 }.slice();
        Assert.AreEqual(unsafe_package.SliceData(heapSlice), unsafe_package.SliceData((slice<byte>)GoUserArenaClone(heapSlice)!), "a heap slice is returned unchanged");
        GoUserArenaFree(arena);
    }

    [TestMethod]
    public void AnAllocationOverTheChunkMaxIsTheHeaps()
    {
        object arena = GoUserArenaNew();
        ж<slice<byte>> pointer = new StandardBox<slice<byte>>(default(slice<byte>));
        GoUserArenaSlice(arena, pointer, (nint)(2 << 20) + 1);
        slice<byte> big = pointer.Value;

        Assert.AreEqual(unsafe_package.SliceData(big), unsafe_package.SliceData((slice<byte>)GoUserArenaClone(big)!), "an over-max allocation is not the arena's");
        GoUserArenaFree(arena);
    }

    [TestMethod]
    public void AnObjectReachableOnlyFromTheArenaLivesUntilFree()
    {
        object arena = GoUserArenaNew();
        WeakReference referent = PlantReferent(arena);

        Collect();
        Assert.IsTrue(referent.IsAlive, "an object referenced from arena memory stays alive while the arena lives");

        GoUserArenaFree(arena);
        Collect();
        Assert.IsFalse(referent.IsAlive, "after free, the arena no longer keeps it alive");
    }

    [TestMethod]
    public void AnArenaCannotBeFreedTwice()
    {
        object arena = GoUserArenaNew();
        GoUserArenaFree(arena);

        PanicException panic = Assert.ThrowsException<PanicException>(() => GoUserArenaFree(arena));
        StringAssert.Contains(panic.Message, "arena double free");
    }

    [TestMethod]
    public void CloneRefusesAValueThatIsNotAPointerSliceOrString()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(() => GoUserArenaClone((nint)2));
        StringAssert.Contains(panic.Message, "arena: Clone only supports pointers, slices, and strings");
    }

    // Allocates a *(*int64) in the arena and points it at a fresh heap object, which nothing but the
    // arena allocation references once this frame returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference PlantReferent(object arena)
    {
        ж<ж<long>> slot = (ж<ж<long>>)GoUserArenaAlloc(arena, typeof(ж<long>));
        ж<long> referent = new StandardBox<long>(5);
        slot.Value = referent;
        return new WeakReference(referent);
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++) {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }
    }
}
