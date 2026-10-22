using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the persistent array element box (golib <c>ж.ElemRefBox.cs</c>, <c>OfArrayElement</c>; P-256
/// R3, COORD ruling 2026-10-22): Go's <c>&amp;a[i]</c> on an array answers ONE box per (backing,
/// absolute index) when the element type has had a field view (<c>BoxShape&lt;T&gt;.Slotted</c>), so the
/// views cached on that box are found by the next expression. The arms are the ruling's: identity across
/// expressions (all three index overloads, and a window over the same backing), the view cache hitting
/// with no allocation, Go's semantics on <c>&amp;table[i]</c> (aliasing both ways, equality, distinct
/// elements and backings), the scoping (a type with no field view keeps a fresh box per expression), and
/// the GC (a persistent box does not keep a dropped backing alive). One more bounds the cost: the slot
/// table grows by chunk, so one address of a big array is not O(N).
/// </summary>
[TestClass]
public class ElemBoxPersistTests
{
    private struct leaf
    {
        public nint n;
    }

    // A consumer type from its first mint, the shape fiat's P256Point has once its x/y/z views exist.
    [GoBoxViews]
    private struct cell
    {
        public leaf X;
        public leaf Y;
        internal static ref leaf ᏑX(ref cell c) => ref c.X;
        internal static ref leaf ᏑY(ref cell c) => ref c.Y;
    }

    // A type that never sees a field view in this process.
    private struct plain
    {
        public nint n;
    }

    [TestMethod]
    public void EveryAddressOfOneElementAnswersOneBox()
    {
        Assert.IsTrue(BoxShape<cell>.Slotted, "[GoBoxViews] makes cell a consumer type from its first mint");
        array<cell> table = new(4);

        ж<cell> first = Ꮡ(table, 1);
        Assert.IsTrue(ReferenceEquals(first, Ꮡ(table, 1)), "&table[1] asked twice must answer the SAME box");
        Assert.IsTrue(ReferenceEquals(first, Ꮡ(table, (nint)1)), "the nint overload answers the same box");
        Assert.IsTrue(ReferenceEquals(first, Ꮡ(table, (ulong)1)), "the ulong overload answers the same box");

        ж<cell> other = Ꮡ(table, 2);
        Assert.IsFalse(ReferenceEquals(first, other), "a different element is a different box");
        Assert.IsFalse(first.Equals(other), "and it does not compare equal");

        array<cell> copy = new(4);
        Assert.IsFalse(ReferenceEquals(first, Ꮡ(copy, 1)), "the same index of another backing is a different box");
        Assert.IsFalse(first.Equals(Ꮡ(copy, 1)), "and it does not compare equal");
    }

    [TestMethod]
    public void AWindowOverTheSameBackingAnswersTheBackingsBox()
    {
        array<cell> table = new(8);
        array<cell> window = array<cell>.Alias(new slice<cell>(table.Source, 2, 6), 4);

        Assert.IsTrue(ReferenceEquals(Ꮡ(table, 3), Ꮡ(window, 1)), "window[1] is table[3]: one storage location, one box");
        Assert.IsFalse(ReferenceEquals(Ꮡ(table, 1), Ꮡ(window, 1)), "the window's own index does not name the backing's slot");
    }

    [TestMethod]
    public void TheViewCachedOnAnElementBoxIsFoundByTheNextExpression()
    {
        FieldViewCache.Disabled = false;
        array<cell> table = new(4);

        ж<leaf> first = Ꮡ(table, 0).of(cell.ᏑX);
        ж<leaf> second = Ꮡ(table, 0).of(cell.ᏑX);
        Assert.IsTrue(ReferenceEquals(first, second), "&table[0].X from two expressions must answer the SAME cached view");
        Assert.IsFalse(ReferenceEquals(first, Ꮡ(table, 0).of(cell.ᏑY)), "a different field is a different view");

        long before = GC.GetAllocatedBytesForCurrentThread();
        ж<leaf> again = Ꮡ(table, 0).of(cell.ᏑX);
        long after = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(again);
        Assert.AreEqual(0L, after - before, "a repeated &table[0].X allocates nothing: neither the element box nor the view is re-minted");
    }

    [TestMethod]
    public void OneAddressOfABigArrayCostsAChunkNotTheArraysLength()
    {
        // warm the generic instantiation, the table and the JIT on another array
        array<cell> warm = new(4096);
        GC.KeepAlive(Ꮡ(warm, 7));

        array<cell> table = new(4096);
        long before = GC.GetAllocatedBytesForCurrentThread();
        ж<cell> p = Ꮡ(table, 7);
        long after = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(p);

        // 64 chunk references, one 64-slot chunk and the box: about 1.1 KB. A slot per element would be 32 KB.
        Assert.IsTrue(after - before <= 2048, $"the first &a[i] of a [4096] array allocated {after - before} B: the slot table must grow by chunk, not by the array's length");
    }

    [TestMethod]
    public void AddressOfAnArrayElementKeepsGosSemantics()
    {
        array<cell> table = new(4);
        ж<cell> p = Ꮡ(table, 2);

        p.Value.X.n = 7;
        Assert.AreEqual((nint)7, table[2].X.n, "a write through &table[2] is read through the array");

        table[2].Y.n = 9;
        Assert.AreEqual((nint)9, Ꮡ(table, 2).Value.Y.n, "a write to the array is read through &table[2]");
        Assert.AreEqual((nint)9, Ꮡ(table, 2).of(cell.ᏑY).Value.n, "and through the cached view of its field");

        Assert.IsTrue(Ꮡ(table, 2).Equals(Ꮡ(table, 2)), "Go: &a[i] == &a[i]");
        Assert.IsFalse(Ꮡ(table, 2).Equals(Ꮡ(table, 3)), "Go: &a[i] != &a[j] for i != j");
    }

    [TestMethod]
    public void ATypeWithNoFieldViewKeepsAFreshBoxPerExpression()
    {
        Assert.IsFalse(BoxShape<plain>.Slotted, "plain never sees a field view in this process");
        array<plain> table = new(4);

        ж<plain> first = Ꮡ(table, 1);
        ж<plain> second = Ꮡ(table, 1);
        Assert.IsFalse(ReferenceEquals(first, second), "outside the scope every &table[i] mints its own box, as before");
        Assert.IsTrue(first.Equals(second), "and the two still compare equal (Go: &a[i] == &a[i])");
    }

    [TestMethod]
    public void APersistentBoxDoesNotKeepADroppedBackingAlive()
    {
        WeakReference backing = AddressElementsOfADroppedTable();

        for (int i = 0; i < 3 && backing.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        Assert.IsFalse(backing.IsAlive, "the backing had persistent element boxes and views, and nothing else references it: it must be collected");
    }

    // The table, its element boxes and their views live only in this frame.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AddressElementsOfADroppedTable()
    {
        array<cell> table = new(16);

        for (int i = 0; i < 16; i++)
            Ꮡ(table, i).of(cell.ᏑX).Value.n = i;

        Assert.IsTrue(ReferenceEquals(Ꮡ(table, 5), Ꮡ(table, 5)), "the boxes are persistent while the table lives");
        return new WeakReference(table.Source);
    }
}
