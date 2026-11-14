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
/// One mechanism. The native box refuses the READ of its whole value when its pointee is such a wrapper, whatever
/// made the box: the number conversion over C memory, an address with a provenance record (pinned Go storage of
/// another type), or the Reinterpret conversion. No real address holds a wrapper struct: storage that holds a
/// reference is never pinned. The box itself may exist: until it is read it is an address
/// (<see cref="NamedArrayCarrierTests"/>, which also holds one arm per read path). Not refused: a pointer converted
/// back to its own type. A nil box and an order token answer their own panics at the dereference.
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

    private static PanicException? Attempt(Action store)
    {
        try
        {
            store();
            return null;
        }
        catch (PanicException panic)
        {
            return panic;
        }
    }

    private static void AssertNamesTheRefusal(PanicException panic, string typeName)
    {
        string message = panic.Message ?? "";

        StringAssert.Contains(message, typeName, "the refusal must name the type it refused");
        StringAssert.Contains(message, "C memory, or Go storage of another type", "the refusal must name the cause, for both arrivals");
        Assert.IsFalse(message.Contains("no managed element storage"), "pinned Go storage HAS managed element storage: the unnamed array's text is not this refusal's");
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
                "a store through a named array over an address with no provenance record must be refused");

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
    /// The number conversion is the same door as the typed one: <c>(ж&lt;A&gt;)(uintptr)addr</c> makes a pointer,
    /// and the read through it is refused too.
    /// </summary>
    [TestMethod]
    public void TheNumberConversionIsTheSameDoor()
    {
        nint block = Zeroed();

        try
        {
            ж<FloorSmallMem> numbered = (ж<FloorSmallMem>)(uintptr)(nuint)block;

            PanicException panic = Assert.ThrowsException<PanicException>(() => _ = numbered.Value[0]);

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
    /// GO memory, arm gh: <c>(*smallMem)(unsafe.Pointer(&amp;w))</c> over a <c>[8]uint64</c>. The address has a
    /// provenance record. Before the floor the store was lost and <c>w[0]</c> read the address of a managed object.
    /// </summary>
    /// <remarks>
    /// This arm replaces one that asserted the opposite, "an address that resolves to a live box of another type
    /// is admitted", the bound <see cref="NativeArrayViewFloorTests"/> states for <c>array&lt;E&gt;</c>. For the
    /// wrapper that bound was the hole.
    /// </remarks>
    [TestMethod]
    public void ANamedArrayOverAGoArrayOfAnotherElementTypeIsRefusedAndStoresNothing()
    {
        array<uint64> words = new(8);
        ж<array<uint64>> pinned = new StandardBox<array<uint64>>(words);

        PanicException? panic = Attempt(
            () => @unsafe.Pointer.To<FloorSmallMem>(@unsafe.Pointer.FromPinnedBox(pinned)).Value[0] = 1);

        Assert.AreEqual(0UL, words[0], $"the Go array was written: its first word reads 0x{words[0]:x}");
        Assert.IsNotNull(panic, "a named array over Go storage of another type must be refused, not answered");
        AssertNamesTheRefusal(panic, nameof(FloorSmallMem));

        GC.KeepAlive(pinned);
    }

    /// <summary>
    /// GO memory, arm gg: <c>(*smallMem)(unsafe.Pointer(&amp;s[0]))</c> over a <c>[]byte</c>.
    /// </summary>
    [TestMethod]
    public void ANamedArrayOverASliceElementIsRefusedAndStoresNothing()
    {
        slice<byte> bytes = make<slice<byte>>(64);

        PanicException? panic = Attempt(
            () => @unsafe.Pointer.To<FloorSmallMem>(@unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0))).Value[7] = 9);

        CollectionAssert.AreEqual(new byte[8], bytes[..8].ToArray(),
            $"the slice was written: its first eight bytes read {BitConverter.ToString(bytes[..8].ToArray())}");
        Assert.IsNotNull(panic, "a named array over a slice's storage must be refused, not answered");
        AssertNamesTheRefusal(panic, nameof(FloorSmallMem));
    }

    /// <summary>
    /// GO memory, arms ga and gd: <c>(*smallMem)(unsafe.Pointer(&amp;arr))</c> over a <c>[64]byte</c>, which the
    /// converter emits as <c>Reinterpret&lt;array&lt;byte&gt;, smallMem&gt;()</c>. This conversion never passes the
    /// number conversion's test.
    /// </summary>
    [TestMethod]
    public void ANamedArrayReinterpretedOverItsUnnamedArrayIsRefusedAndStoresNothing()
    {
        array<byte> bytes = new(64);
        ж<array<byte>> boxed = new StandardBox<array<byte>>(bytes);

        PanicException? panic = Attempt(() => boxed.Reinterpret<array<byte>, FloorSmallMem>().Value[1] = 3);

        CollectionAssert.AreEqual(new byte[8], bytes[..8].ToArray(),
            $"the array was written: its first eight bytes read {BitConverter.ToString(bytes[..8].ToArray())}");
        Assert.IsNotNull(panic, "a named array over the unnamed array it is defined on must be refused, not answered");
        AssertNamesTheRefusal(panic, nameof(FloorSmallMem));

        GC.KeepAlive(boxed);
    }

    /// <summary>
    /// NOT this floor: the number of a pointer whose pointee holds a reference is an ORDER TOKEN, not an address.
    /// A box over it converts as before and keeps its own refusal at the dereference.
    /// </summary>
    [TestMethod]
    public void AnOrderTokenAtANamedArrayPointeeKeepsItsOwnRefusal()
    {
        ж<@string> text = new StandardBox<@string>((@string)"text");
        uintptr token = (uintptr)text;

        ж<FloorSmallMem> carrier = (ж<FloorSmallMem>)token;

        Assert.IsNotNull(carrier, "an order token at a named array pointee still converts");

        PanicException panic = Assert.ThrowsException<PanicException>(() => carrier.Value[0] = 1,
            "and its dereference is still refused");

        Assert.IsFalse((panic.Message ?? "").Contains("cannot view native memory"),
            "by the order-token refusal, not by this floor");

        GC.KeepAlive(text);
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
