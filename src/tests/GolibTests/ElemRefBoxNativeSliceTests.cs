using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

/// <summary>
/// Guards <c>ElemRefBox</c>'s managed-backing predicate against NATIVE-backed slices (Q58's split
/// seat). A native-backed <c>slice&lt;T&gt;</c> carries <c>m_array = []</c> as its sentinel — non-null
/// AND empty, and <c>[]</c> is the shared <c>Array.Empty&lt;T&gt;()</c> singleton — so a
/// <c>m_array is not null</c> test captures that empty array as the backing. Two consequences, and
/// this class holds one arm for each: every element read indexes the empty array and throws, and every
/// native block canonicalizes onto the SAME object, so element pointers into UNRELATED blocks compare
/// equal. The predicate was sound until native-backed slices existed.
/// </summary>
/// <remarks>
/// The boxes are constructed DIRECTLY rather than through <c>Ꮡ</c>, because every <c>Ꮡ</c> door routes
/// a native-backed slice to a <c>NativeBox</c> before it reaches <c>ElemRefBox</c>. The live consumer
/// is <c>ж&lt;array&lt;T&gt;&gt;.at()</c>, which builds the element box straight from its array view
/// and arrives with the pointer-to-array kind; guarding the predicate here keeps this seat independent
/// of that increment.
/// </remarks>
[TestClass]
public class ElemRefBoxNativeSliceTests
{
    private struct Elem
    {
        public ulong A;
        public ulong B;
    }

    private const int N = 8;

    private static nuint AllocBlock(ulong seed)
    {
        nint bytes = N * Marshal.SizeOf<Elem>();
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(bytes);

        unsafe
        {
            Elem* raw = (Elem*)addr;

            for (int i = 0; i < N; i++)
                raw[i] = new Elem { A = seed + (ulong)i, B = seed + 0x100 + (ulong)i };
        }

        return addr;
    }

    [TestMethod]
    public void AnElementBoxOverANativeSliceReadsTheBlockAndNotAnEmptyArray()
    {
        nuint addr = AllocBlock(0x1000);

        try
        {
            global::go.slice<Elem> native = global::go.slice<Elem>.OverNativeMemory(addr, N);

            // The IArray constructor — the arm ж<array<T>>.at() takes.
            ж<Elem> viaInterface = new ElemRefBox<Elem>((IArray)native, 3);
            Assert.AreEqual(0x1003UL, viaInterface.Value.A, "IArray ctor: read the native block");

            // The CONCRETE-header constructor. Its callers route native slices away today, so this
            // is defence at the predicate — which is why it is guarded HERE rather than left to the
            // first caller that does not.
            ж<Elem> viaConcrete = new ElemRefBox<Elem>(native, 5);
            Assert.AreEqual(0x1005UL, viaConcrete.Value.A, "concrete ctor: read the native block");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void AWriteThroughAnElementBoxOverANativeSliceReachesTheBlock()
    {
        nuint addr = AllocBlock(0x2000);

        try
        {
            global::go.slice<Elem> native = global::go.slice<Elem>.OverNativeMemory(addr, N);
            ж<Elem> p = new ElemRefBox<Elem>((IArray)native, 4);

            p.Value = new Elem { A = 0xFEED, B = 0xFACE };

            // Read back NATIVELY: a box that wrote into a copy would satisfy its own read and fail here.
            unsafe
            {
                Elem* raw = (Elem*)addr;
                Assert.AreEqual(0xFEEDUL, raw[4].A, "the write did not reach the native block");
                Assert.AreEqual(0xFACEUL, raw[4].B, "the write did not reach the native block");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void ElementPointersIntoDIFFERENTNativeBlocksAreNotTheSamePointer()
    {
        nuint first = AllocBlock(0x3000);
        nuint second = AllocBlock(0x4000);

        try
        {
            global::go.slice<Elem> a = global::go.slice<Elem>.OverNativeMemory(first, N);
            global::go.slice<Elem> b = global::go.slice<Elem>.OverNativeMemory(second, N);

            ж<Elem> pa = new ElemRefBox<Elem>((IArray)a, 2);
            ж<Elem> pb = new ElemRefBox<Elem>((IArray)b, 2);

            // The quiet half of the defect: canonicalizing onto the shared Array.Empty<T>() made two
            // pointers into unrelated blocks report one storage identity, so these compared EQUAL.
            Assert.IsFalse(pa.Equals(pb), "pointers into different native blocks must not be equal");
            Assert.AreNotEqual(pa.PointerOrderToken, pb.PointerOrderToken, "nor share an order token");

            // And the same element of the same block still is the same pointer.
            ж<Elem> paAgain = new ElemRefBox<Elem>((IArray)a, 2);
            Assert.IsTrue(pa.Equals(paAgain), "the same element of the same block is the same pointer");
        }
        finally
        {
            Marshal.FreeHGlobal((nint)first);
            Marshal.FreeHGlobal((nint)second);
        }
    }

    [TestMethod]
    public void AManagedBackedSliceStillTakesTheFastBackingArm()
    {
        // The must-not-regress direction: nothing about a managed slice changes, and it keeps the
        // absolute-index identity a re-sliced view shares with its parent.
        global::go.slice<Elem> managed = new global::go.slice<Elem>(new Elem[N]);
        managed[2] = new Elem { A = 0x77, B = 0x88 };

        ж<Elem> p = new ElemRefBox<Elem>((IArray)managed, 2);
        Assert.AreEqual(0x77UL, p.Value.A, "managed slice element");

        ж<Elem> viaConcrete = new ElemRefBox<Elem>(managed, 2);
        Assert.IsTrue(p.Equals(viaConcrete), "both ctors name the same element of the same backing");

        p.Value = new Elem { A = 0x99, B = 0xAA };
        Assert.AreEqual(0x99UL, managed[2].A, "the write reached the managed backing");
    }
}
