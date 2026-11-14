using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// A pointer to a named array over real memory is a CARRIER until something reads the array through it.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="NamedArrayViewFloorTests"/> holds the refusal. These arms hold its place: at the READ of the whole
/// value (the native box's <c>Value</c> and <c>ValueSlot</c>), never where the box is made. A box of the wrapper
/// that is only an address reads no memory: its number, its equality, a conversion onward to another pointee,
/// and a struct copy-back that hands it to a caller who never dereferences it. Refusing those at the mint broke
/// conversions that worked before the floor and store nothing.
/// </para>
/// <para>
/// The second half is one arm per way the whole value is read. Each makes the box first, which must not panic,
/// then reads through it, which must be the named refusal, then reads the storage, which must be unchanged: the
/// store this floor exists for wrote the address of a managed object there.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NamedArrayCarrierTests
{
    private const int NativeBytes = 128;

    // Go layout: Count at 0, Mem at 4 (a byte array aligns at 1), size 68.
    private struct Holder
    {
        public uint32 Count;
        public FloorSmallMem Mem;
    }

    private const int GoOffsetOfMem = 4;

    // Spelled as go2cs-gen spells a field accessor (Ꮡ + the Go field name): golib reads the name off the delegate.
    private static ref FloorSmallMem ᏑMem(ref Holder value) => ref value.Mem;

    // A struct the deep funnel copies out and back: a count beside a pointer to a named array.
    // Go layout: Count at 0, Mem at 8; size 16.
    public struct OutParams
    {
        public uint32 Count;
        public ж<FloorSmallMem> Mem;
    }

    private sealed class Probe
    {
        public Func<ReadOnlySpan<nuint>, nuint>? Body;
    }

    private static nuint invoke(Probe probe, ReadOnlySpan<nuint> args) => probe.Body!(args);

    private static nint Zeroed()
    {
        nint block = Marshal.AllocHGlobal(NativeBytes);

        for (int i = 0; i < NativeBytes; i++)
            Marshal.WriteByte(block, i, 0);

        return block;
    }

    // `(*smallMem)(unsafe.Pointer(&b[0]))`: pinned Go storage of another type. The box is made here, outside any
    // Attempt, so an arm fails on a panic at the mint.
    private static ж<FloorSmallMem> NamedOver(slice<byte> bytes) =>
        @unsafe.Pointer.To<FloorSmallMem>(@unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0)));

    private static PanicException? Attempt(Action read)
    {
        try
        {
            read();
            return null;
        }
        catch (PanicException panic)
        {
            return panic;
        }
    }

    private static void AssertRefusedAtTheReadAndNothingStored(PanicException? panic, slice<byte> bytes, string path)
    {
        byte[] head = bytes[..8].ToArray();

        CollectionAssert.AreEqual(new byte[8], head, $"{path}: the Go storage was written: its first eight bytes read {BitConverter.ToString(head)}");
        Assert.IsNotNull(panic, $"{path}: reading a named array through a box over storage of another type must be refused");

        string message = panic.Message ?? "";

        StringAssert.Contains(message, "cannot view native memory as", $"{path}: by this floor");
        StringAssert.Contains(message, nameof(FloorSmallMem), $"{path}: the refusal must name the type it refused");
        StringAssert.Contains(message, "DESIGN-native-array-view.md", $"{path}: the refusal must point at the document");
    }

    // ---- carriers: none of these reads the array ----

    /// <summary>
    /// <c>(*byte)(unsafe.Pointer(&amp;s.mem))</c> with <c>s</c> a struct in native memory. The field's box is a
    /// native box of the wrapper at the field's Go address; it is converted onward and never read as an array.
    /// </summary>
    [TestMethod]
    public void TheAddressOfANamedArrayFieldInNativeMemoryConvertsOnward()
    {
        nint block = Zeroed();

        try
        {
            ж<Holder> holder = (ж<Holder>)(uintptr)(nuint)block;
            ж<FloorSmallMem> field = holder.of<FloorSmallMem>(ᏑMem);

            Assert.AreEqual((nuint)block + GoOffsetOfMem, ((uintptr)field).Value,
                "precondition: the field's pointer is the field's Go address, or this arm measures no native field box");

            ж<byte> first = @unsafe.Pointer.To<byte>(@unsafe.Pointer.FromBox(field));

            first.Value = 0x5A;

            Assert.AreEqual((byte)0x5A, Marshal.ReadByte(block, GoOffsetOfMem), "the byte lands at the field's Go offset");
            Assert.AreEqual((byte)0x5A, first.Value);
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    /// <summary>
    /// <c>p := (*smallMem)(unsafe.Pointer(&amp;b[0]))</c> used as an address only: its number is <c>&amp;b[0]</c>,
    /// it is not nil, and it equals itself converted again.
    /// </summary>
    [TestMethod]
    public void ANamedArrayPointerOverASliceElementIsAnAddress()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<byte> element = Ꮡ(bytes, 0);

        ж<FloorSmallMem> named = NamedOver(bytes);
        ж<FloorSmallMem> again = NamedOver(bytes);

        Assert.IsTrue(named != nil, "a pointer over real storage is not nil");
        Assert.AreEqual(((uintptr)element).Value, ((uintptr)named).Value, "its number is the element's address");
        Assert.IsTrue(named == again, "two conversions of one address are one pointer");
        CollectionAssert.AreEqual(new byte[8], bytes[..8].ToArray(), "nothing is stored by converting");
    }

    /// <summary>
    /// <c>(*byte)(unsafe.Pointer((*smallMem)(unsafe.Pointer(&amp;b[0]))))</c>: cast straight back, the bytes are
    /// the slice's own.
    /// </summary>
    [TestMethod]
    public void ANamedArrayPointerOverASliceElementCastsStraightBack()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        bytes[0] = 3;

        ж<byte> back = @unsafe.Pointer.To<byte>(@unsafe.Pointer.FromBox(NamedOver(bytes)));

        Assert.AreEqual((byte)3, back.Value, "the round trip reads the slice's first byte");

        back.Value = 8;

        Assert.AreEqual((byte)8, bytes[0], "and a store through it reaches the slice");

        // The same pair through the Reinterpret door: the element's box pins its backing and mints the native box
        // of the wrapper itself, and the conversion onward takes that box's address.
        ж<byte> again = Ꮡ(bytes, 1).Reinterpret<byte, FloorSmallMem>().Reinterpret<FloorSmallMem, byte>();

        again.Value = 5;

        Assert.AreEqual((byte)5, bytes[1], "the Reinterpret round trip reaches the element it started from");
    }

    /// <summary>
    /// The same round trip through the Reinterpret door, as the converter emits <c>(*smallMem)(unsafe.Pointer(&amp;arr))</c>
    /// and a conversion onward from it.
    /// </summary>
    [TestMethod]
    public void ANamedArrayReinterpretedOverAnArrayConvertsOnward()
    {
        array<byte> bytes = new(64);
        ж<array<byte>> boxed = new StandardBox<array<byte>>(bytes);
        bytes[0] = 4;

        ж<FloorSmallMem> named = boxed.Reinterpret<array<byte>, FloorSmallMem>();
        ж<byte> first = named.Reinterpret<FloorSmallMem, byte>();

        Assert.AreEqual((byte)4, first.Value, "the onward pointer reads the array's first byte");

        GC.KeepAlive(boxed);
    }

    /// <summary>
    /// A syscall copy-back: the callee writes a non-nil address into a <c>*smallMem</c> field. The copy-back makes
    /// a native box of the wrapper for it, and the caller never reads through it. The whole copy-back must still
    /// complete.
    /// </summary>
    [TestMethod]
    public void ACopyBackWithAnUnreadNamedArrayPointerCompletes()
    {
        ref OutParams parameters = ref heap(new OutParams(), out ж<OutParams> box);
        nuint number = ((uintptr)box).Value;

        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(number), "precondition: the struct crosses through the deep funnel");

        nint native = Zeroed();

        try
        {
            Probe probe = new()
            {
                Body = args =>
                {
                    byte* copy = (byte*)args[0];

                    Assert.AreEqual((nuint)0, *(nuint*)(copy + 8), "a nil pointer field crosses as 0");

                    *(uint*)copy = 3;
                    *(nuint*)(copy + 8) = (nuint)native;
                    return 0;
                }
            };

            NativeStructMarshal.CallDeep([number], probe, &invoke);

            Assert.AreEqual(3u, parameters.Count, "the field beside the pointer decodes back");
            Assert.IsTrue(parameters.Mem != nil, "the pointer the callee wrote is not nil");
            Assert.AreEqual((nuint)native, ((uintptr)parameters.Mem).Value, "and it is that exact address");
        }
        finally
        {
            Marshal.FreeHGlobal(native);
        }

        GC.KeepAlive(box);
    }

    /// <summary>
    /// Two named arrays of one shape, <c>(*B)(unsafe.Pointer(a))</c> over Go memory: runtime's
    /// <c>Reinterpret&lt;pallocBits, pageBits&gt;</c>. One field sequence, so the conversion is a view of the same
    /// box and reaches no native box.
    /// </summary>
    [TestMethod]
    public void ANamedArrayReinterpretedAsAnotherOfItsShapeAliasesTheSameArray()
    {
        ж<FloorSmallMem> box = new StandardBox<FloorSmallMem>(new FloorSmallMem());

        ж<FloorSmallTwin> twin = box.Reinterpret<FloorSmallMem, FloorSmallTwin>();

        Assert.IsFalse(twin.IsNative, "the twin is a view of the managed box, not a native box");

        twin.Value[5] = 6;

        Assert.AreEqual((byte)6, box.Value[5], "a store through the twin is seen through the original");
    }

    // ---- reads: one arm per way the whole value is reached ----

    /// <summary><c>x := p[0]</c>: the box's <c>Value</c>, then the wrapper's indexer.</summary>
    [TestMethod]
    public void AnIndexReadIsRefusedAtTheRead()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => _ = named.Value[0]), bytes, "index read");
    }

    /// <summary><c>p[7] = 9</c>: the store that filled the wrapper's holder and wrote its reference.</summary>
    [TestMethod]
    public void AnIndexStoreIsRefusedAtTheReadAndStoresNothing()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => named.Value[7] = 9), bytes, "index store");
    }

    /// <summary><c>v := *p</c>: the whole value, through <c>Value</c> and through the dereference operator.</summary>
    [TestMethod]
    public void AWholeValueCopyIsRefusedAtTheRead()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => { FloorSmallMem copy = named.Value; GC.KeepAlive(copy); }), bytes, "Value copy");
        AssertRefusedAtTheReadAndNothingStored(Attempt(() => { FloorSmallMem copy = ~named; GC.KeepAlive(copy); }), bytes, "dereference operator");
    }

    /// <summary><c>*p = v</c>: the whole value stored.</summary>
    [TestMethod]
    public void AWholeValueStoreIsRefusedAtTheReadAndStoresNothing()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);
        FloorSmallMem filled = new();
        filled[0] = 1;

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => named.Value = filled), bytes, "whole store");
    }

    /// <summary>The re-alias accessor a walked pointer parameter uses: <c>ValueSlot</c>.</summary>
    [TestMethod]
    public void TheSlotAccessorIsRefusedAtTheRead()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => named.DerefOrNil()[1] = 3), bytes, "DerefOrNil");
    }

    /// <summary><c>&amp;p[3]</c>: the element door reads the array to find its element.</summary>
    [TestMethod]
    public void TheElementDoorIsRefusedAtTheRead()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => _ = named.at<byte>(3)), bytes, "element door");
    }

    /// <summary>reflect and the untyped store: the box's slot read and written as an object.</summary>
    [TestMethod]
    public void TheReflectAndUntypedSlotsAreRefusedAtTheRead()
    {
        slice<byte> bytes = make<slice<byte>>(64);
        ж<FloorSmallMem> named = NamedOver(bytes);

        AssertRefusedAtTheReadAndNothingStored(Attempt(() => _ = ((IGoReflectBox)named).ReadBoxSlot()), bytes, "reflect slot read");
        AssertRefusedAtTheReadAndNothingStored(Attempt(() => ((IGoReflectBox)named).WriteBoxSlot(new FloorSmallMem())), bytes, "reflect slot write");
        AssertRefusedAtTheReadAndNothingStored(Attempt(() => ((IUntypedSlotAccess)named).TryLoadThrough(out _)), bytes, "untyped load");
        AssertRefusedAtTheReadAndNothingStored(Attempt(() => ((IUntypedSlotAccess)named).TryStoreThrough(new FloorSmallMem())), bytes, "untyped store");
    }
}

// A second named array of FloorSmallMem's shape, as runtime's pageBits is to pallocBits.
[GoType("[64]byte")]
public struct FloorSmallTwin : IArray<byte>
{
    private System.Runtime.CompilerServices.StrongBox<array<byte>>? m_value;

    public array<byte> Value => m_value is { } value ? value.Value : materializeValue();

    private array<byte> materializeValue()
    {
        System.Runtime.CompilerServices.StrongBox<array<byte>> created = new(new array<byte>(64));
        return (System.Threading.Interlocked.CompareExchange(ref m_value, created, null) ?? created).Value;
    }

    public byte[] Source => Value.Source;
    Array? IArray.Source => Value.Source;
    public nint Length => Value.Length;
    public ref byte this[nint index] => ref Value[index];
    object? IArray.this[nint index] { get => Value[index]; set => Value[index] = (byte)value!; }
    public Span<byte> ꓸꓸꓸ => Value.ꓸꓸꓸ;
    public Span<byte> ToSpan() => Value.ToSpan();
    public object Clone() => this;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((System.Collections.IEnumerable)Value).GetEnumerator();
    System.Collections.Generic.IEnumerator<byte> System.Collections.Generic.IEnumerable<byte>.GetEnumerator() => ((System.Collections.Generic.IEnumerable<byte>)Value).GetEnumerator();
}
