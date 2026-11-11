using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.sync; // sync/atomic's method set is extension methods on atomic_package
using go.@internal.runtime; // and internal/runtime/atomic's
using static go.builtin;
using syncatomic = go.sync.atomic_package;
using rtatomic = go.@internal.runtime.atomic_package;

namespace GolibTests;

/// <summary>
/// S3 site 10, the review's F7 (docs/phase4/DATA-la64-s3-census.md §4): an UNALIGNED 64-bit atomic faults on arm64,
/// where amd64's LOCK prefix only slows down. Go gives atomic.Int64 and atomic.Uint64 an align64 field so they are
/// 8-aligned on every platform. The converted types carry it as an explicit 8-byte layout with the value at offset 0.
/// What reading cannot prove (§4) is that the CLR PLACES such a struct at an 8-aligned address when it is a local, a
/// field after a byte, boxed on the heap, or an array element.
/// This class measures each placement and then runs the real converted atomic operation on it. On arm64 a misaligned
/// one would fault the test host instead of failing an assert.
/// - Flavour-neutral: it runs on every leg. The arm leg (S5b's golib-tests at linux/arm64) is the reading that
///   matters; the others hold the same property.
/// - The control: a Pack=1 struct puts the field at offset 1. The alignment check must SEE that (a red the check
///   can produce on any host), and the control is never given to an atomic operation.
/// - Go's own internal/runtime/atomic and sync/atomic tests run on the arm leg as the linux/arm64 bank's shards A-7
///   and A-14 (docs/phase4/DATA-la64-s7-bank-plan.md).
/// </summary>
[TestClass]
public class AtomicAlignmentTests
{
    // Go's shape `struct { flag bool; n atomic.Int64 }`: the atomic follows a one-byte field.
    private struct SyncAfterByte
    {
        public bool flag;
        public syncatomic.Int64 n;
        public byte tail;
        public syncatomic.Uint64 u;
    }

    private struct RuntimeAfterByte
    {
        public bool flag;
        public rtatomic.Int64 n;
        public byte tail;
        public rtatomic.Uint64 u;
    }

    // THE CONTROL: packed, so the atomic lands at offset 1. Never handed to an atomic operation.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PackedControl
    {
        public byte flag;
        public syncatomic.Int64 n;
    }

    private static unsafe nuint Misalignment<T>(ref T field) => (nuint)Unsafe.AsPointer(ref field) % 8;

    private static void AssertAligned<T>(ref T field, string shape) =>
        Assert.AreEqual((nuint)0, Misalignment(ref field), $"{shape}: the 64-bit atomic is not 8-aligned");

    [TestMethod]
    public void TheCheckSeesAPackedMisalignment()
    {
        // An array's data starts 8-aligned on a 64-bit CLR, so element 0's field is at offset 1, element 1's at 10.
        // (A packed LOCAL has no guaranteed placement, so it is not used here.)
        PackedControl[] array = new PackedControl[3];
        Assert.AreEqual((nuint)1, Misalignment(ref array[0].n), "Pack=1 puts element 0's atomic at offset 1");
        Assert.AreEqual((nuint)2, Misalignment(ref array[1].n), "and element 1's at offset 10");
    }

    [TestMethod]
    public void SyncAtomicInt64AndUint64AreEightAlignedInEveryPlacement()
    {
        syncatomic.Int64 local = default;
        AssertAligned(ref local, "a local");
        Assert.AreEqual(5L, local.Add(5), "Add on a local");

        SyncAfterByte s = default;
        AssertAligned(ref s.n, "a field after a bool");
        AssertAligned(ref s.u, "a Uint64 after a byte");
        Assert.AreEqual(7L, s.n.Add(7), "Add on a field after a bool");
        Assert.AreEqual(9UL, s.u.Add(9), "Add on a Uint64 after a byte");

        ж<SyncAfterByte> box = Ꮡ(new SyncAfterByte());
        AssertAligned(ref box.Value.n, "a boxed struct's field");
        AssertAligned(ref box.Value.u, "a boxed struct's Uint64");
        Assert.AreEqual(11L, box.Value.n.Add(11), "Add on a boxed struct's field");
        Assert.AreEqual(13UL, box.Value.u.Add(13), "Add on a boxed struct's Uint64");

        SyncAfterByte[] array = new SyncAfterByte[5];
        for (int i = 0; i < array.Length; i++)
        {
            AssertAligned(ref array[i].n, $"array element {i}");
            AssertAligned(ref array[i].u, $"array element {i}'s Uint64");
            Assert.AreEqual((long)i + 1, array[i].n.Add(i + 1), $"Add on array element {i}");
            Assert.AreEqual((ulong)i + 2, array[i].u.Add((ulong)i + 2), $"Add on array element {i}'s Uint64");
        }
    }

    [TestMethod]
    public void RuntimeAtomicInt64AndUint64AreEightAlignedInEveryPlacement()
    {
        RuntimeAfterByte s = default;
        AssertAligned(ref s.n, "a runtime Int64 after a bool");
        AssertAligned(ref s.u, "a runtime Uint64 after a byte");

        RuntimeAfterByte[] array = new RuntimeAfterByte[5];
        for (int i = 0; i < array.Length; i++)
        {
            AssertAligned(ref array[i].n, $"runtime array element {i}");
            AssertAligned(ref array[i].u, $"runtime array element {i}'s Uint64");
        }

        // The runtime's own operations take a box (ж<Int64>), the shape the converted runtime hands them.
        ж<rtatomic.Int64> n = Ꮡ(new rtatomic.Int64());
        AssertAligned(ref n.Value, "a boxed runtime Int64");
        Assert.AreEqual(3L, n.Add(3), "the runtime's Add on a boxed Int64");

        ж<rtatomic.Uint64> u = Ꮡ(new rtatomic.Uint64());
        AssertAligned(ref u.Value, "a boxed runtime Uint64");
        Assert.AreEqual(4UL, u.Add(4), "the runtime's Add on a boxed Uint64");
    }
}
