using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The safety floor of <see cref="NativeArrayViewFloorTests"/>, for a NAMED array type
/// (<c>type RawMem [1&lt;&lt;50 - 1]byte</c>, modernc.org/libc).
/// </summary>
/// <remarks>
/// <para>
/// A named array emits as a go2cs-gen wrapper struct whose one field is a lazily filled holder, a managed
/// reference. A native box reads its value with <c>Unsafe.AsRef&lt;T&gt;</c> at the address, so
/// <c>(*RawMem)(p)</c> over C memory read that reference OUT OF the C memory. Measured before the floor
/// (docs/phase4/DESIGN-native-array-view.md, the 2026-11-14 block): over memory that is not zero the bytes are
/// dereferenced as a reference; over zeroed memory a huge array dies in <c>new array&lt;byte&gt;(1&lt;&lt;50 - 1)</c>
/// with an OverflowException; over zeroed memory a SMALL array allocates a managed array, stores its reference
/// INTO the C memory and reports success. The collector cannot see that reference, and the C side reads eight
/// bytes it never wrote.
/// </para>
/// <para>
/// The discriminator is the address's PROVENANCE, as it is for <c>array&lt;E&gt;</c>: the same wrapper over an
/// address the registry knows converts as before. The admitted arms below are as load-bearing as the refused ones.
/// </para>
/// </remarks>
[TestClass]
public class NamedArrayViewFloorTests
{
    private const int NativeBytes = 128;

    private static nint Zeroed()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        for (int i = 0; i < NativeBytes; i++)
            Marshal.WriteByte(block, i, 0);

        return block;
    }

    private static nint Filled(byte with)
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        for (int i = 0; i < NativeBytes; i++)
            Marshal.WriteByte(block, i, with);

        return block;
    }

    // `(*A)(p)` for an unsafe.Pointer made from a number, as the converter emits it.
    private static ж<T> Converted<T>(nint address) => @unsafe.Pointer.To<T>((@unsafe.Pointer)(uintptr)(nuint)address);

    private static void AssertNamesTheRefusal(PanicException panic, string typeName)
    {
        string message = panic.Message ?? "";

        StringAssert.Contains(message, typeName, "the refusal must name the type it refused");
        StringAssert.Contains(message, "no managed element storage", "the refusal must name the cause");
        StringAssert.Contains(message, "DESIGN-native-array-view.md", "the refusal must point at the document");
    }

    /// <summary>
    /// Arm M0: <c>(*smallMem)(unsafe.Pointer(z))[5] = 0</c> over zeroed C memory, a 64-byte named array.
    /// Before the floor this returned normally.
    /// </summary>
    [TestMethod]
    public void ASmallNamedArrayOverZeroedNativeMemoryIsRefusedByName()
    {
        nint block = Zeroed();

        try
        {
            PanicException panic = Assert.ThrowsException<PanicException>(
                () => Converted<FloorSmallMem>(block).Value[5] = 7,
                "a named array over an address with no provenance record must not become a box of its wrapper");

            AssertNamesTheRefusal(panic, nameof(FloorSmallMem));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// The same arm, read at the memory: whatever the conversion and the store do, nothing is written into the
    /// C memory. Before the floor the first eight bytes held the address of a managed object.
    /// </summary>
    [TestMethod]
    public void ASmallNamedArrayOverZeroedNativeMemoryStoresNothingIntoIt()
    {
        nint block = Zeroed();

        try
        {
            try
            {
                Converted<FloorSmallMem>(block).Value[5] = 7;
            }
            catch (PanicException)
            {
                // The refusal. This arm reads the memory, the arm above reads the panic.
            }

            byte[] after = new byte[NativeBytes];
            Marshal.Copy(block, after, 0, NativeBytes);

            CollectionAssert.AreEqual(new byte[NativeBytes], after,
                $"the native memory was written: its first eight bytes read {BitConverter.ToString(after, 0, 8)}");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arms C0 and D0: libc's <c>RawMem</c> over zeroed C memory. Before the floor: OverflowException out of
    /// the wrapper's lazy <c>new array&lt;byte&gt;(1&lt;&lt;50 - 1)</c>.
    /// </summary>
    [TestMethod]
    public void AHugeNamedArrayOverZeroedNativeMemoryIsRefusedByName()
    {
        nint block = Zeroed();

        try
        {
            PanicException panic = Assert.ThrowsException<PanicException>(
                () => Converted<FloorRawMem>(block).Value[4] = 7,
                "a huge named array over zeroed native memory must be the named refusal, not an allocation failure");

            AssertNamesTheRefusal(panic, nameof(FloorRawMem));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// Arms C and D: the same over C memory that holds data. Before the floor the data bytes were read as a
    /// managed reference and dereferenced.
    /// </summary>
    [TestMethod]
    public void AHugeNamedArrayOverFilledNativeMemoryIsRefusedByName()
    {
        // 'X', the fill of the measured arm: a non-canonical address, so the read before the floor faulted
        // as a null dereference and did not take the host down.
        nint block = Filled((byte)'X');

        try
        {
            PanicException panic = Assert.ThrowsException<PanicException>(
                () => Converted<FloorRawMem>(block).Value[4] = 7,
                "a named array over filled native memory must be the named refusal, not a read of its bytes as a reference");

            AssertNamesTheRefusal(panic, nameof(FloorRawMem));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// The number conversion is the same door as the typed one: <c>(ж&lt;A&gt;)(uintptr)addr</c> is refused too.
    /// </summary>
    [TestMethod]
    public void TheNumberConversionIsTheSameDoor()
    {
        nint block = Zeroed();

        try
        {
            PanicException panic = Assert.ThrowsException<PanicException>(
                () => _ = (ж<FloorSmallMem>)(uintptr)(nuint)block);

            AssertNamesTheRefusal(panic, nameof(FloorSmallMem));
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// A nil pointer is not an address: <c>(*A)(unsafe.Pointer(nil))</c> converts to a nil pointer.
    /// </summary>
    [TestMethod]
    public void ANilPointerAtANamedArrayPointeeIsANilPointer()
    {
        ж<FloorSmallMem> fromNil = @unsafe.Pointer.To<FloorSmallMem>((@unsafe.Pointer)default(NilType));
        ж<FloorSmallMem> fromZero = (ж<FloorSmallMem>)(uintptr)0;

        Assert.IsTrue(fromNil == nil, "the typed door over a nil unsafe.Pointer is a nil pointer");
        Assert.IsTrue(fromZero == nil, "the number 0 is a nil pointer");
    }

    /// <summary>
    /// Go memory keeps working: a pointer to a named array converted to unsafe.Pointer and back is that pointer.
    /// </summary>
    [TestMethod]
    public void APointerToANamedArrayInGoMemoryRoundTrips()
    {
        ж<FloorSmallMem> box = new StandardBox<FloorSmallMem>(new FloorSmallMem());
        box.Value[3] = 9;

        ж<FloorSmallMem> typed = @unsafe.Pointer.To<FloorSmallMem>(@unsafe.Pointer.FromBox(box));
        ж<FloorSmallMem> numbered = (ж<FloorSmallMem>)(uintptr)box;

        Assert.AreSame(box, typed, "(*A)(unsafe.Pointer(p)) over Go memory is p");
        Assert.AreSame(box, numbered, "the number of a pointer to a named array resolves to that pointer");
        Assert.AreEqual((byte)9, typed.Value[3]);

        GC.KeepAlive(box);
    }

    /// <summary>
    /// THE BOUND, as <see cref="NativeArrayViewFloorTests.APinnedManagedAddressAtAnArrayPointeeIsAdmittedBecauseItResolves"/>
    /// states it for <c>array&lt;E&gt;</c>: an address that resolves to a live box of ANOTHER pointee type is
    /// admitted. The floor refuses the no-provenance class, not every wrong view.
    /// </summary>
    [TestMethod]
    public void APinnedManagedAddressAtANamedArrayPointeeIsAdmittedBecauseItResolves()
    {
        ж<array<uint64>> pinned = new StandardBox<array<uint64>>(new array<uint64>(8));
        uintptr address = (uintptr)pinned;

        ж<FloorSmallMem> view = (ж<FloorSmallMem>)address;

        Assert.IsNotNull(view, "an address with a provenance record is not this floor's class");

        GC.KeepAlive(pinned);
    }

    /// <summary>
    /// THE SCOPE: the floor is keyed on an array wrapper. A slice is an <c>IArray</c> too (ISlice extends it),
    /// and a slice header over C memory is not refused HERE. That is a stated bound of this cut, not a claim
    /// that the shape is sound.
    /// </summary>
    [TestMethod]
    public void ASlicePointeeIsOutsideThisFloor()
    {
        nint block = Zeroed();

        try
        {
            ж<slice<byte>> header = (ж<slice<byte>>)(uintptr)(nuint)block;

            Assert.IsNotNull(header, "widening the floor to slice headers is a different cut with its own measurement");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }
}

// The storage half of the wrapper go2cs-gen writes for `type A [N]E` (InheritedTypeTemplate): one field, a lazily
// filled holder, and a Value that fills it on first use. That field is the managed reference the floor is about.
[GoType("[64]byte")]
public struct FloorSmallMem : IArray<byte>
{
    private StrongBox<array<byte>>? m_value;

    public array<byte> Value => m_value is { } value ? value.Value : materializeValue();

    private array<byte> materializeValue()
    {
        StrongBox<array<byte>> created = new(new array<byte>(64));
        return (Interlocked.CompareExchange(ref m_value, created, null) ?? created).Value;
    }

    public byte[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref byte this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (byte)value!; }
    public Span<byte> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<byte> ToSpan() => Value.ToSpan();
    public object Clone() => this;
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => ((IEnumerable<byte>)Value).GetEnumerator();
}

// libc's RawMem on a 64-bit target: [1<<50 - 1]byte.
[GoType("[1125899906842623]byte")]
public struct FloorRawMem : IArray<byte>
{
    private StrongBox<array<byte>>? m_value;

    public array<byte> Value => m_value is { } value ? value.Value : materializeValue();

    private array<byte> materializeValue()
    {
        StrongBox<array<byte>> created = new(new array<byte>(1125899906842623));
        return (Interlocked.CompareExchange(ref m_value, created, null) ?? created).Value;
    }

    public byte[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref byte this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (byte)value!; }
    public Span<byte> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<byte> ToSpan() => Value.ToSpan();
    public object Clone() => this;
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Value).GetEnumerator();
    IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => ((IEnumerable<byte>)Value).GetEnumerator();
}
