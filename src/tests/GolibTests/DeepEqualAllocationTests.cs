using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using reflect = go.reflect_package;

namespace GolibTests;

// reflect.DeepEqual over the shapes of reflect's TestDeepEqualAllocs (Go 1.26), which wants 0 allocations
// for each: Go 1.26 keeps DeepEqual's visited map in its frame while it holds one group (8 entries). Each
// source this guards was measured on its own:
//   S1 the visited set (a HashSet per call, now 8 entries inline and a HashSet only for a ninth),
//   S2 a TypeOf of any slice read element 0 for array dims, boxing it,
//   S3 a slice's window read by FieldInfo.GetValue, boxing the nint,
//   S4 an array's dims array and its rendered intern key on every TypeOf,
//   S5 an elementwise Index walk, an ElemRefBox and a boxed element per element per side,
//   S6 reflect's canonType rendering the dims key on every Type(),
//   N1 the same walk one level down ([][]byte, [2][]int),
//   N2 a TypeOf of a slice of arrays boxing element 0 and rendering the key ([][6]byte).
// The correctness rows are Go 1.26's own answers for the paths those fixes take, read from a Go run.
[TestClass]
public class DeepEqualAllocationTests
{
    private const int Calls = 1000;

    // The fewest bytes one window of Calls DeepEquals allocated, over three windows after a warm-up. A
    // per-call allocation shows in every window (Calls bytes at the least); a one-off JIT or tiering
    // allocation on this thread shows in one at most.
    private static long minBytesPerWindow(object a, object b)
    {
        for (int i = 0; i < 200; i++)
            reflect.DeepEqual(a, b);

        long fewest = long.MaxValue;

        for (int window = 0; window < 3; window++)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < Calls; i++)
                reflect.DeepEqual(a, b);

            fewest = Math.Min(fewest, GC.GetAllocatedBytesForCurrentThread() - before);
        }

        return fewest;
    }

    private static slice<T> sliceOf<T>(params T[] items) => new(items);

    private static void assertNoBytes(string shape, object a, object b)
    {
        Assert.IsTrue(reflect.DeepEqual(a, b), $"{shape}: the pair must be deeply equal, or the walk under test never runs");
        Assert.AreEqual(0L, minBytesPerWindow(a, b), $"{shape}: bytes allocated by {Calls} DeepEqual calls");
    }

    [TestMethod]
    public void ScalarShapesAllocateNothing()
    {
        assertNoBytes("int8", (sbyte)99, (sbyte)99);
        assertNoBytes("[]int8", sliceOf<sbyte>(99), sliceOf<sbyte>(99));
        assertNoBytes("[]float64", sliceOf(1.414), sliceOf(1.414));
        assertNoBytes("string", (@string)"abcdef", (@string)"abcdef");
        assertNoBytes("[]string", sliceOf<@string>("abcdef"), sliceOf<@string>("abcdef"));
        assertNoBytes("[]byte", sliceOf("abcdef"u8.ToArray()), sliceOf("abcdef"u8.ToArray()));
        assertNoBytes("[6]byte", new array<byte>("abcabc"u8.ToArray()), new array<byte>("abcabc"u8.ToArray()));
    }

    [TestMethod]
    public void NestedShapesAllocateNothing()
    {
        assertNoBytes("[][]byte", sliceOf(sliceOf("abcdef"u8.ToArray())), sliceOf(sliceOf("abcdef"u8.ToArray())));
        assertNoBytes("[][6]byte", sliceOf(new array<byte>("abcabc"u8.ToArray())), sliceOf(new array<byte>("abcabc"u8.ToArray())));
        assertNoBytes("[][]float64", sliceOf(sliceOf(1.5, 2.5)), sliceOf(sliceOf(1.5, 2.5)));
        assertNoBytes("[2][]int", new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(2)]), new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(2)]));
    }

    // N2's door reads element 0 in place; the dims it reports, and so the descriptor, are the ones the
    // boxed read gave: a [][6]byte and a [][3]byte stay two types.
    [TestMethod]
    public void SliceOfArraysKeepsItsElementLength()
    {
        reflect.ΔType six = reflect.TypeOf(sliceOf(new array<byte>(new byte[6])));
        reflect.ΔType three = reflect.TypeOf(sliceOf(new array<byte>(new byte[3])));

        Assert.AreEqual((nint)6, six.Elem().Len());
        Assert.AreEqual((nint)3, three.Elem().Len());
        Assert.IsFalse(reflect.DeepEqual(sliceOf(new array<byte>(new byte[6])), sliceOf(new array<byte>(new byte[3]))));
        Assert.AreEqual((nint)(-1), ((IGoReflectSequence)sliceOf(sliceOf<byte>(1))).ReflectFirstArrayLength(), "a slice element is not an array");
        Assert.AreEqual((nint)(-1), ((IGoReflectSequence)new slice<array<byte>>(new array<byte>[0])).ReflectFirstArrayLength(), "an empty slice has no element 0");
        Assert.AreEqual((nint)(-1), ((IGoReflectSequence)sliceOf(new array<array<byte>>(1))).ReflectFirstArrayLength(), "a nested array keeps the boxed walk");
    }

    // A struct element wider than a reference and holding one, the third shape the layout guard reads.
    private struct wide
    {
        internal long a, b, c, d;
        internal object? r;
    }

    // N2 reads element 0's length by viewing the array<E> in place as an array<byte>. That holds only while
    // every array<E> has one layout (a backing reference, then its low and length, whatever E is). Pin it for a
    // reference element, a one-byte element and a wide struct element, through a WINDOW (an Alias with a
    // non-zero low and a length short of its backing), so a reordered or retyped field fails here by name
    // rather than reading a wrong length.
    [TestMethod]
    public void ArrayLayoutIsTheSameForEveryElementType()
    {
        assertLayout(new object[] { "a", "b", "c", "d", "e" });
        assertLayout(new byte[] { 1, 2, 3, 4, 5 });
        assertLayout(new wide[5]);
    }

    private static void assertLayout<E>(E[] backing)
    {
        string name = typeof(E).Name;
        Assert.AreEqual(Unsafe.SizeOf<array<byte>>(), Unsafe.SizeOf<array<E>>(), $"array<{name}> size");

        array<E> window = array<E>.Alias(new slice<E>(backing).slice(2), 2);
        Assert.AreEqual((nint)2, window.Length, $"array<{name}> window length");
        Assert.AreEqual((nint)2, Unsafe.As<array<E>, array<byte>>(ref window).Length, $"array<{name}> length read as array<byte>");
        Assert.IsTrue(ReferenceEquals(backing, Unsafe.As<array<E>, array<byte>>(ref window).m_array), $"array<{name}> backing read as array<byte>");
        Assert.AreEqual((nint)2, ((IGoReflectSequence)sliceOf(window)).ReflectFirstArrayLength(), $"slice of array<{name}>: element 0's length");
    }

    [TestMethod]
    public void NestedRowsAnswerAsGoDoes()
    {
        double nan = double.NaN;
        slice<double> shared = sliceOf(nan);
        slice<double> ones = sliceOf(1.0, 1.0);
        slice<double> nans = sliceOf(nan, nan);

        Assert.IsTrue(reflect.DeepEqual(sliceOf(shared), sliceOf(shared)), "nested shared NaN: &x[0] == &y[0] before content");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(sliceOf(nan)), sliceOf(sliceOf(nan))), "nested copied NaN");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(default(slice<byte>)), sliceOf(new slice<byte>(Array.Empty<byte>()))), "nested nil vs empty");
        Assert.IsTrue(reflect.DeepEqual(sliceOf(default(slice<byte>)), sliceOf(default(slice<byte>))), "nested nil vs nil");
        Assert.IsTrue(reflect.DeepEqual(sliceOf(sliceOf<byte>(1, 2), sliceOf<byte>(3)), sliceOf(sliceOf<byte>(1, 2), sliceOf<byte>(3))), "nested bytes equal");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(sliceOf<byte>(1, 2), sliceOf<byte>(3)), sliceOf(sliceOf<byte>(1, 2), sliceOf<byte>(4))), "nested bytes differ last");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(sliceOf<byte>(1, 2)), sliceOf(sliceOf<byte>(1))), "nested inner length differs");
        Assert.IsTrue(reflect.DeepEqual(sliceOf(new array<byte>([1, 2, 3, 4, 5, 6])), sliceOf(new array<byte>([1, 2, 3, 4, 5, 6]))), "slice of arrays equal");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(new array<byte>([1, 2, 3, 4, 5, 6])), sliceOf(new array<byte>([1, 2, 3, 4, 5, 7]))), "slice of arrays differ");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(new array<double>([nan])), sliceOf(new array<double>([nan]))), "slice of float arrays NaN");
        Assert.IsTrue(reflect.DeepEqual(new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(2)]), new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(2)])), "array of slices equal");
        Assert.IsFalse(reflect.DeepEqual(new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(2)]), new array<slice<nint>>([sliceOf<nint>(1), sliceOf<nint>(3)])), "array of slices differ");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(sliceOf<@string>("a")), sliceOf(sliceOf<@string>("b"))), "nested strings differ");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(sliceOf(new Complex(nan, nan))), sliceOf(sliceOf(new Complex(nan, nan)))), "nested complex NaN");
        Assert.IsTrue(reflect.DeepEqual(sliceOf(ones.slice(0, 1)), sliceOf(ones.slice(1, 2))), "same backing, other low");
        Assert.IsFalse(reflect.DeepEqual(sliceOf(nans.slice(0, 1)), sliceOf(nans.slice(1, 2))), "same backing, other low, NaN");
    }

    // `type recur []any` rings of twelve: the walk revisits each pair once per lap, so it terminates only
    // through the visited set, which holds twelve pairs: four past the eight it keeps inline.
    private static slice<object> ring(int n)
    {
        var links = new slice<object>[n];

        for (int i = 0; i < n; i++)
            links[i] = new slice<object>(1);

        for (int i = 0; i < n; i++)
            links[i][0] = links[(i + 1) % n];

        return links[0];
    }

    [TestMethod]
    public void CyclesPastTheInlineVisitedEntriesTerminate()
    {
        Assert.IsTrue(reflect.DeepEqual(ring(12), ring(12)), "ring of 12");

        slice<object> broken = ring(12);
        ((slice<object>)broken[0])[0] = (@string)"x";
        Assert.IsFalse(reflect.DeepEqual(ring(12), broken), "ring of 12, one link replaced");
    }
}
