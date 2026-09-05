using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards the native-backed pointer-to-array (Q58 / increment 8's read half): Go's <c>*[N]T</c> over
/// off-heap storage, the shape the page allocator builds when it hangs a <c>sysAlloc</c>'d chunk
/// block off <c>p.chunks[l1]</c>. The element door reads and WRITES the real block, an index past the
/// minted length is refused by the door's own bounds check, and the two things that cannot be done
/// honestly — dereferencing an <c>array&lt;T&gt;</c> that has no header to read, and windowing the
/// block as some other element type — are refused by name rather than answered with garbage.
/// Driven over unmanaged heap memory (AllocHGlobal) so the arms are platform-neutral.
/// </summary>
[TestClass]
public class NativeBackedArrayPointerTests
{
    // A pallocData-shaped element: unmanaged, several words wide, so an off-by-one stride shows up.
    private struct Elem
    {
        public ulong A;
        public ulong B;
    }

    private const int N = 8;

    private static nuint AllocBlock()
    {
        nint bytes = N * Marshal.SizeOf<Elem>();
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(bytes);

        unsafe
        {
            new Span<byte>((void*)addr, (int)bytes).Clear();
        }

        return addr;
    }

    [TestMethod]
    public void TheElementDoorReadsTheRealBlockAtTheRightStride()
    {
        nuint addr = AllocBlock();

        try
        {
            // Seed the block natively, so anything the box reads came from THIS memory.
            unsafe
            {
                Elem* raw = (Elem*)addr;

                for (int i = 0; i < N; i++)
                    raw[i] = new Elem { A = (ulong)(0x1000 + i), B = (ulong)(0x2000 + i) };
            }

            ж<array<Elem>> p = NativeArrayBox<Elem>.Over(addr, N);

            for (nint i = 0; i < N; i++)
            {
                Assert.AreEqual((ulong)(0x1000 + i), p.at<Elem>(i).Value.A, $"element {i}.A");
                Assert.AreEqual((ulong)(0x2000 + i), p.at<Elem>(i).Value.B, $"element {i}.B");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AWriteThroughTheElementPointerReachesTheNativeBlock()
    {
        nuint addr = AllocBlock();

        try
        {
            ж<array<Elem>> p = NativeArrayBox<Elem>.Over(addr, N);

            p.at<Elem>(3).Value = new Elem { A = 0xDEAD, B = 0xBEEF };

            // Read it back NATIVELY, not through the box: a box that quietly wrote into a copy
            // would satisfy a read-back through itself and fail here — which is the whole hazard
            // this kind exists to avoid.
            unsafe
            {
                Elem* raw = (Elem*)addr;
                Assert.AreEqual(0xDEADUL, raw[3].A, "the write did not reach the native block");
                Assert.AreEqual(0xBEEFUL, raw[3].B, "the write did not reach the native block");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AnIndexPastTheMintedLengthIsRefused()
    {
        nuint addr = AllocBlock();

        try
        {
            // Minted SHORTER than the block: the refusal must follow the minted length, not the
            // memory that happens to be mapped after it.
            ж<array<Elem>> p = NativeArrayBox<Elem>.Over(addr, 4);

            Assert.AreEqual((ulong)0, p.at<Elem>(3).Value.A, "index 3 is inside the minted length");
            Assert.ThrowsException<IndexOutOfRangeException>(() => p.at<Elem>(4), "index 4 is past the minted length");
            Assert.ThrowsException<IndexOutOfRangeException>(() => p.at<Elem>(-1), "a negative index");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void DereferencingTheArrayIsRefusedByName()
    {
        nuint addr = AllocBlock();

        try
        {
            ж<array<Elem>> p = NativeArrayBox<Elem>.Over(addr, N);

            // There is no array<Elem> at the address to return a ref to. Refusing by name is the
            // point of the kind: the alternative is reading element bytes AS an array header and
            // handing back a garbage Elem[] — the prestub null read this replaces.
            PanicException thrown = Assert.ThrowsException<PanicException>(() => p.Value);
            StringAssert.Contains(thrown.Message, "no array", "the refusal must say what it refuses");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AZeroAddressIsTheNilPointerAndANegativeLengthIsRefused()
    {
        ж<array<Elem>> nil = NativeArrayBox<Elem>.Over(0, N);
        Assert.IsTrue(nil.IsNilPointer, "a zero address is Go's nil pointer");

        Assert.ThrowsException<PanicException>(() => NativeArrayBox<Elem>.Over(0x1000, -1), "a negative length");
    }

    [TestMethod]
    public void AManagedReferenceElementTypeIsRefusedByTheNativeWindowDoor()
    {
        nuint addr = AllocBlock();

        try
        {
            // The element door delegates to golib's single native-window creation door, so its
            // refusal is inherited rather than re-implemented here.
            ж<array<string>> p = NativeArrayBox<string>.Over(addr, N);

            Assert.ThrowsException<PanicException>(() => p.at<string>(0), "a managed-reference element type");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }
}
