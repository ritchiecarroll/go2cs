using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards Go's whole array assignment INTO an existing location (docs/phase4/DESIGN-array-reassign-in-place.md, section
/// 8): <c>a = b</c> copies b's elements into a's own storage, so whatever was taken from a beforehand still names it.
/// Each arm is one of P5's review conditions: C1 unequal lengths (a null-backed source reads length 0, a clamped
/// window is shorter than its source), C2 a null-backed destination takes the value with its length, C3 a nested
/// element is assigned in place and written back, C4 two windows over one backing copy in the safe direction, and C6
/// a pointer over native memory stores through the box.
/// </summary>
[TestClass]
public class ArrayAssignInPlaceTests
{
    private static array<nint> Of(params nint[] values) => new(values);

    [TestMethod]
    public void C1_ANullBackedSourceClearsTheDestination()
    {
        array<nint> dest = Of(1, 2, 3, 4);
        slice<nint> taken = dest[..];

        dest.Assign(default);

        Assert.AreEqual((nint)4, dest.Length);
        Assert.AreEqual("0 0 0 0", string.Join(" ", taken.ToArray()));
    }

    [TestMethod]
    public void C1_AWindowShorterThanItsSourceCopiesItsOwnLength()
    {
        slice<nint> storage = new(new nint[] { 0, 0, 0, 0, 0, 0 });
        array<nint> window = go.array<nint>.Alias(storage[1..3], 2);

        window.Assign(Of(7, 8, 9, 10));

        Assert.AreEqual("0 7 8 0 0 0", string.Join(" ", storage.ToArray()));
    }

    [TestMethod]
    public void C2_ANullBackedDestinationTakesTheLiteralWithItsLength()
    {
        array<nint> dest = default;

        dest.Assign(new array<nint>(4));
        dest[2] = 5;

        Assert.AreEqual((nint)4, dest.Length);
        Assert.AreEqual((nint)5, dest[2]);
    }

    [TestMethod]
    public void C3_ANestedElementKeepsItsBackingAndTakesTheValues()
    {
        array<array<nint>> outer = new(2, () => new array<nint>(2));
        array<nint> inner = outer[1];

        outer.Assign(new array<array<nint>>(new[] { Of(1, 2), Of(3, 4) }));

        Assert.AreEqual("3 4", string.Join(" ", inner.ToSpan().ToArray()));
    }

    [TestMethod]
    public void C3_ANullBackedNestedElementIsWrittenBack()
    {
        array<array<nint>> outer = new(2);

        outer.Assign(new array<array<nint>>(new[] { Of(1, 2), Of(3, 4) }));

        Assert.AreEqual((nint)2, outer[1].Length);
        Assert.AreEqual((nint)4, outer[1][1]);
    }

    [TestMethod]
    public void C4_TwoWindowsOverOneBackingCopyInTheSafeDirection()
    {
        slice<array<nint>> up = new(new[] { Of(1, 1), Of(2, 2), Of(3, 3) });
        array<array<nint>> above = go.array<array<nint>>.Alias(up[1..], 2);
        array<array<nint>> below = go.array<array<nint>>.Alias(up, 2);

        above.Assign(below);

        Assert.AreEqual("1 1 1 1 2 2", Flatten(up));

        slice<array<nint>> down = new(new[] { Of(1, 1), Of(2, 2), Of(3, 3) });
        array<array<nint>> lower = go.array<array<nint>>.Alias(down, 2);
        array<array<nint>> upper = go.array<array<nint>>.Alias(down[1..], 2);

        lower.Assign(upper);

        Assert.AreEqual("2 2 3 3 3 3", Flatten(down));
    }

    [TestMethod]
    public void C6_APointerOverNativeMemoryStoresThroughTheBox()
    {
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(4 * sizeof(long));

        try
        {
            ж<array<long>> p = NativeArrayBox<long>.Over(addr, 4);

            p.Assign(new array<long>(new long[] { 11, 12, 13, 14 }));

            unsafe
            {
                long* raw = (long*)addr;
                Assert.AreEqual("11 12 13 14", $"{raw[0]} {raw[1]} {raw[2]} {raw[3]}");
            }

            // A null-backed source clears the block, as C1 has it for a managed array.
            p.Assign(default);

            unsafe
            {
                long* raw = (long*)addr;
                Assert.AreEqual("0 0 0 0", $"{raw[0]} {raw[1]} {raw[2]} {raw[3]}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void C6_ASourceLongerThanTheBlockNeverWritesPastIt()
    {
        // P5's ask b: the block's N decides how far the store writes, never the source's length. Four elements, then a
        // guard word the store must not touch; the source holds six.
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(5 * sizeof(long));

        try
        {
            unsafe
            {
                long* raw = (long*)addr;
                for (int i = 0; i < 4; i++)
                    raw[i] = 0;
                raw[4] = 0x5A5A5A5A;
            }

            ж<array<long>> p = NativeArrayBox<long>.Over(addr, 4);

            p.Assign(new array<long>(new long[] { 1, 2, 3, 4, 5, 6 }));

            unsafe
            {
                long* raw = (long*)addr;
                Assert.AreEqual("1 2 3 4", $"{raw[0]} {raw[1]} {raw[2]} {raw[3]}");
                Assert.AreEqual(0x5A5A5A5AL, raw[4], "the store wrote past the block");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void C6_APointerArrayOverNativeWordsStoresThenLeavesNilWords()
    {
        // P5's ask a: NativePointerArrayBox.StoreElements stores min(N, len(source)) words, then the nil pointer for the
        // rest. Three zero words and a guard word after them. The source is one nil pointer, so every slot keeps the
        // nil a zero word mints (null): the next read of each word refuses nothing, and no word moves.
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(4 * sizeof(long));

        try
        {
            unsafe
            {
                long* raw = (long*)addr;
                raw[0] = raw[1] = raw[2] = 0;
                raw[3] = 0x77;
            }

            ж<array<ж<long>>> p = NativePointerArrayBox<long>.Over(addr, 3);

            p.Assign(new array<ж<long>>(1));

            IArray words = p.TryGetNativeArrayView(typeof(ж<long>))!;

            for (nint i = 0; i < 3; i++)
                Assert.IsNull(words[i], $"word {i} reads as nil");

            unsafe
            {
                long* raw = (long*)addr;
                Assert.AreEqual("0 0 0 119", $"{raw[0]} {raw[1]} {raw[2]} {raw[3]}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    [TestMethod]
    public void C6_TheValueDerefStillRefusesOverNativeMemory()
    {
        // Cut A's emission, `p.Value.Assign(b)`: the deref has no array<T> to hand out and refuses by name. This is
        // why `*p = b` for an unnamed array emits `p.Assign(b)`.
        nuint addr = (nuint)(nint)Marshal.AllocHGlobal(4 * sizeof(long));

        try
        {
            ж<array<long>> p = NativeArrayBox<long>.Over(addr, 4);

            Assert.ThrowsException<PanicException>(() => p.Value.Assign(new array<long>(4)));
        }
        finally
        {
            Marshal.FreeHGlobal((nint)addr);
        }
    }

    private static string Flatten(slice<array<nint>> s)
    {
        System.Text.StringBuilder text = new();

        foreach (array<nint> element in s.ToArray())
        {
            foreach (nint value in element.ToSpan())
                text.Append(text.Length == 0 ? "" : " ").Append(value);
        }

        return text.ToString();
    }
}
