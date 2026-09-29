using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// A16: builtin.NativeArrayPointer over a REFERENCE-BEARING element (the page allocator's
// *[8192]pallocData over a sysAlloc'd block). The elements live in a managed store that shadows the block
// (ShadowArrayBox); the pointer's address stays the block's, so Go's sysFree/sysHugePage get the memory
// Go allocated. The windows re-probe measured the naive managed store refusing ж -> uintptr ("Object
// contains references") inside FreePageAlloc's sysFree.
[TestClass]
public class ShadowArrayPointerTests
{
    // pallocData's shape: a struct over golib arrays, so managed at two levels.
    private struct RefElem
    {
        internal long n;
        internal array<ulong> bits;
    }

    private const int N = 16;

    [TestMethod]
    public void AReferenceBearingElementGetsAZeroedManagedStoreBehindTheNativeAddress()
    {
        nint block = Marshal.AllocHGlobal(4096);

        try
        {
            // AllocHGlobal does not zero; the untouched-block check below needs a known starting word.
            Marshal.WriteInt64(block, 0);

            ж<array<RefElem>> p = builtin.NativeArrayPointer<RefElem>((nuint)block, N);

            Assert.IsInstanceOfType(p, typeof(ShadowArrayBox<RefElem>));
            Assert.AreEqual((nuint)block, p.NativeAddress, "the pointer's address is the native block's");
            Assert.AreEqual((nuint)block, ((uintptr)p).Value, "ж -> uintptr hands out the block, as sysFree/sysHugePage need");

            // Zeroed, as fresh sysAlloc'd memory is.
            Assert.AreEqual(0L, p.at<RefElem>(5).Value.n, "a fresh element is the zero value");
            p.at<RefElem>(5).Value.n = 11;
            Assert.AreEqual(11L, p.at<RefElem>(5).Value.n);

            // A write through the element door (chunkOf's `&p.chunks[l1][l2]`) is read back through it.
            p.at<RefElem>(5).Value.bits = new array<ulong>(8);
            p.at<RefElem>(5).Value.bits[3] = 7UL;
            Assert.AreEqual(7UL, p.at<RefElem>(5).Value.bits[3]);
            Assert.AreEqual(7UL, p.Value[5].bits[3], "the element door and Value name one store");

            // The native block itself is never written: it stands unused behind the store.
            Assert.AreEqual(0L, Marshal.ReadInt64(block), "the store is managed; the block is untouched");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void TwoPointersToOneBlockAreOneGoPointer()
    {
        nint block = Marshal.AllocHGlobal(4096);

        try
        {
            ж<array<RefElem>> a = builtin.NativeArrayPointer<RefElem>((nuint)block, N);
            ж<array<RefElem>> b = builtin.NativeArrayPointer<RefElem>((nuint)block, N);

            Assert.IsTrue(a.Equals(b), "the same block is the same Go pointer");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreEqual(a.PointerOrderToken, b.PointerOrderToken);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AnUnmanagedElementStillGetsTheNativeView()
    {
        nint block = Marshal.AllocHGlobal(N * sizeof(long));

        try
        {
            ж<array<long>> p = builtin.NativeArrayPointer<long>((nuint)block, N);

            Assert.IsInstanceOfType(p, typeof(NativeArrayBox<long>), "an unmanaged element keeps aliasing the native bytes");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AZeroAddressIsTheNilPointer() =>
        Assert.IsTrue(builtin.NativeArrayPointer<RefElem>(0, N).IsNilPointer);
}
