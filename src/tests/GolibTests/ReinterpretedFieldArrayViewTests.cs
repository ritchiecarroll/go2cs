using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The field view over a Go struct that HOLDS A REFERENCE: Go's <c>(*[N]E)(unsafe.Pointer(&amp;x.f))</c>, f not an
/// E, where x's converted struct has no address (docs/phase4/DESIGN-array-pointer-doors.md, section 19). The
/// converter emits <c>@unsafe.ArrayPointer&lt;E&gt;.Of(field, N)</c> for every root, and this root was refused by
/// name (FieldArrayViewOverGoStructTests, V4).
/// </summary>
/// <remarks>
/// The answer is golib's view of the FIELD's own bytes through the field's pointer
/// (<c>ReinterpretedArrayBox</c>): every element access reads the field's live slot, so a store is a store to the
/// field, the field's own accesses and the view's see each other, and the collector may move the struct between
/// any two accesses. It has no storage and no identity of its own: its number, token and equality are the field
/// pointer's. A view longer than its field and a field that holds a reference are refused by name.
/// </remarks>
[TestClass]
public class ReinterpretedFieldArrayViewTests
{
    // A Go struct with no reference in it, used as a FIELD.
    private struct Pair
    {
        public ushort A;
        public ushort B;
    }

    // A Go struct that holds a reference: its storage has no address. x/sys's sockaddr shape, with a name beside it.
    private struct Named
    {
        public @string Name;
        public ushort Family;
        public ushort Port;
        public ushort Tail;
        public uint Addr;
        public Pair Both;

        // Converter-named, as the generated accessors are.
        internal static ref @string ᏑName(ref Named instance) => ref instance.Name;

        internal static ref ushort ᏑFamily(ref Named instance) => ref instance.Family;

        internal static ref ushort ᏑPort(ref Named instance) => ref instance.Port;

        internal static ref uint ᏑAddr(ref Named instance) => ref instance.Addr;

        internal static ref Pair ᏑBoth(ref Named instance) => ref instance.Both;
    }

    private static ж<Named> NewNamed() =>
        new StandardBox<Named>(new Named { Name = "x", Family = 2, Port = 0x1234, Tail = 0x5678, Addr = 0x0A0B0C0D, Both = new Pair { A = 0x1111, B = 0x2222 } });

    private static ж<array<byte>> PortOf(ж<Named> named) => @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑPort), 2);

    private static string PanicText(Action action)
    {
        try
        {
            action();
        }
        catch (PanicException panic)
        {
            return panic.Message ?? "";
        }

        Assert.Fail("no panic");
        return "";
    }

    private static void Collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    // The field's own number: where the collector has the field now (an address nothing holds still).
    private static nuint NumberOf<T>(ж<T> box) => (nuint)((uintptr)box).Value;

    /// <summary>
    /// The control: the struct has no address. Its pointer's number is an order token and it cannot be pinned, which
    /// is why no native array pointer can be made over its field.
    /// </summary>
    [TestMethod]
    public void TheStructHasNoAddress()
    {
        ж<Named> named = NewNamed();

        Assert.AreEqual(PointerStorage.None, named.StorageKind);
        Assert.IsNull(named.PinnableStorage);
        Assert.AreEqual(PointerStorage.Unpinnable, named.of(Named.ᏑPort).StorageKind, "the field has an address nothing can hold still");
    }

    /// <summary>
    /// The index form, as the converter emits it: both bytes of the field are read in place, and a store through
    /// the view is a store to the field and to nothing else.
    /// </summary>
    [TestMethod]
    public void TheViewReadsAndWritesTheFieldsBytesInPlace()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        Assert.AreEqual((byte)0x34, p.ElementRef(0), "the low byte of Port");
        Assert.AreEqual((byte)0x12, p.ElementRef(1), "the high byte of Port");
        Assert.AreEqual((byte)0x12, p.ElementRef((nint)1));
        Assert.AreEqual((byte)0x12, p.ElementRef(1UL));

        p.ElementRef(0) = 0xAB;
        p.ElementRef(1) += 2;

        Assert.AreEqual((ushort)0x14AB, named.Value.Port, "a store through the view is a store to the field");
        Assert.AreEqual((ushort)2, named.Value.Family, "and to nothing before it");
        Assert.AreEqual((ushort)0x5678, named.Value.Tail, "and to nothing after it");
        Assert.AreEqual("x", (string)named.Value.Name);
    }

    /// <summary>
    /// x/sys's port: <c>p[0] = byte(port &gt;&gt; 8); p[1] = byte(port)</c> writes the port in network order, and the
    /// field reads back as the host's number for those two bytes.
    /// </summary>
    [TestMethod]
    public void APortIsEncodedThroughTheViewAndDecodedBack()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);
        int port = 8080;

        p.ElementRef(0) = (byte)(port >> 8);
        p.ElementRef(1) = (byte)port;

        Assert.AreEqual(BitConverter.IsLittleEndian ? (ushort)0x901F : (ushort)8080, named.Value.Port);
        Assert.AreEqual(8080, p.ElementRef(0) << 8 | p.ElementRef(1), "and it decodes back");
    }

    /// <summary>
    /// The tuple and range stores, <c>*p, n = v, 9</c> and <c>for _, *p = range vs</c>, which the converter emits as
    /// <c>p.Assign(v)</c> (ArrayExtensions.Assign): through this box that is the box's own store, N element stores to
    /// the field and to nothing else. Without the box's INativeArrayStore forward, Assign fell to Value, which refuses
    /// by name.
    /// </summary>
    [TestMethod]
    public void AssignThroughTheViewIsTheBoxsStore()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        p.Assign(new byte[] { 0xCD, 0xAB }.array());

        Assert.AreEqual(BitConverter.IsLittleEndian ? (ushort)0xABCD : (ushort)0xCDAB, named.Value.Port, "an Assign through the view is a store to the field");
        Assert.AreEqual((ushort)2, named.Value.Family, "and to nothing before it");
        Assert.AreEqual((ushort)0x5678, named.Value.Tail, "and to nothing after it");

        p.Assign(new array<byte>(2));

        Assert.AreEqual((ushort)0, named.Value.Port, "the zero array stores zeros");
        Assert.AreEqual((ushort)0x5678, named.Value.Tail);
    }

    /// <summary>
    /// The field's own accesses and the view's name one storage: each sees the other at once, in both directions.
    /// </summary>
    [TestMethod]
    public void TheFieldsOwnAccessesAndTheViewSeeEachOther()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        named.Value.Port = 0xBEEF;

        Assert.AreEqual((byte)0xEF, p.ElementRef(0), "a store to the field is read through the view");
        Assert.AreEqual((byte)0xBE, p.ElementRef(1));

        p.ElementRef(1) = 0x01;

        Assert.AreEqual((ushort)0x01EF, named.Value.Port, "a store through the view is read from the field");

        // A second view of the same field, made later, is over the same bytes.
        ж<array<byte>> q = PortOf(named);

        q.ElementRef(0) = 0x77;
        Assert.AreEqual((byte)0x77, p.ElementRef(0));
    }

    /// <summary>
    /// An address word as four bytes, and a field that is a struct with no reference in it: its bytes are
    /// contiguous, so it is viewed the same way, as bytes and as its two halves.
    /// </summary>
    [TestMethod]
    public void AWordAndAReferenceFreeStructFieldAreViewed()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> addr = @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑAddr), 4);

        Assert.AreEqual((byte)0x0D, addr.ElementRef(0));
        Assert.AreEqual((byte)0x0A, addr.ElementRef(3));

        addr.ElementRef(3) = 0x7F;
        Assert.AreEqual(0x7F0B0C0Du, named.Value.Addr);

        ж<array<ushort>> halves = @unsafe.ArrayPointer<ushort>.Of(named.of(Named.ᏑBoth), 2);

        Assert.AreEqual((ushort)0x1111, halves.ElementRef(0));
        Assert.AreEqual((ushort)0x2222, halves.ElementRef(1));

        halves.ElementRef(1) = 0x3333;
        Assert.AreEqual((ushort)0x3333, named.Value.Both.B);

        ж<array<byte>> bytes = @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑBoth), 4);

        Assert.AreEqual((byte)0x33, bytes.ElementRef(3));
    }

    /// <summary>
    /// Go's <c>v := *p</c> and <c>*p = v</c>, as part C emits them: the load is a copy of the N elements, the
    /// store is N element stores in place, and a value shorter than N is zeros from where it ends.
    /// </summary>
    [TestMethod]
    public void TheWholeArrayLoadsAsACopyAndStoresInPlace()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        array<byte> v = @unsafe.ArrayPointer<byte>.Load(p);

        Assert.AreEqual(2, (int)v.Length);
        Assert.AreEqual((byte)0x34, v[0]);
        Assert.AreEqual((byte)0x12, v[1]);

        p.ElementRef(0) = 0xAB;
        Assert.AreEqual((byte)0x34, v[0], "the load is a copy");

        v[1] = 0xEE;
        Assert.AreEqual((ushort)0x12AB, named.Value.Port, "a store to the copy stays in the copy");

        array<byte> w = new(2);

        w[0] = 0x01;
        w[1] = 0x02;
        @unsafe.ArrayPointer<byte>.Store(p, w);

        Assert.AreEqual((ushort)0x0201, named.Value.Port, "the store is to the field");
        Assert.AreEqual((ushort)0x5678, named.Value.Tail, "and to nothing after it");

        @unsafe.ArrayPointer<byte>.Store(p, default(array<byte>));

        Assert.AreEqual((ushort)0, named.Value.Port, "an array that was never given storage is zeros");
    }

    /// <summary>
    /// The collector may move the struct between any two accesses: a store before a forced compacting collection is
    /// read after it, through the view and through the field, and a store after it lands in the field where it is
    /// now. The view was made before the move.
    /// </summary>
    /// <remarks>
    /// The witness is the field's own number, an address nothing holds still: it must have changed in some rounds,
    /// or the rounds say nothing about a struct that did not move.
    /// </remarks>
    [TestMethod]
    public void AStoreIsReadAfterTheCollectorMovesTheStruct()
    {
        int moved = 0;

        for (int round = 0; round < 200; round++)
        {
            // Garbage in front of the struct, so a compacting collection has somewhere to slide it.
            byte[][] garbage = new byte[64][];

            for (int i = 0; i < garbage.Length; i++)
                garbage[i] = new byte[128 + round % 7];

            ж<Named> named = NewNamed();
            ж<array<byte>> p = PortOf(named);
            ж<array<byte>> addr = @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑAddr), 4);
            nuint before = NumberOf(named.of(Named.ᏑPort));
            byte seed = (byte)(round * 5 + 1);

            p.ElementRef(0) = seed;
            p.ElementRef(1) = (byte)(seed + 1);
            @unsafe.ArrayPointer<byte>.Store(addr, new byte[] { seed, 2, 3, 4 }.array());

            GC.KeepAlive(garbage);
            garbage = null!;
            Collect();

            if (NumberOf(named.of(Named.ᏑPort)) != before)
                moved++;

            Assert.AreEqual(seed, p.ElementRef(0), $"round {round}: the view reads what was stored before the collection");
            Assert.AreEqual((byte)(seed + 1), p.ElementRef(1), $"round {round}");
            Assert.AreEqual((ushort)(seed | (seed + 1) << 8), named.Value.Port, $"round {round}: and so does the field");
            Assert.AreEqual(seed, @unsafe.ArrayPointer<byte>.Load(addr)[0], $"round {round}: a whole store before it is loaded after it");

            p.ElementRef(1) = 0x5A;

            Assert.AreEqual((ushort)(seed | 0x5A << 8), named.Value.Port, $"round {round}: a store after the collection lands in the field where it is now");
            Assert.AreEqual("x", (string)named.Value.Name, $"round {round}");
            Assert.AreEqual((ushort)0x5678, named.Value.Tail, $"round {round}");
        }

        if (moved == 0)
            Assert.Inconclusive("the collector moved the struct in none of 200 rounds, so this run cannot tell a view that follows the field from one that does not");
    }

    /// <summary>
    /// The view keeps the struct: nothing else holds it, and the field's bytes are still read after a collection.
    /// </summary>
    [TestMethod]
    public void TheViewKeepsTheStructItViews()
    {
        (ж<array<byte>> p, WeakReference holder) = PortOfAStructNothingElseHolds();

        Collect();

        Assert.IsTrue(holder.IsAlive, "the view keeps the value its field is in");
        Assert.AreEqual((byte)0x34, p.ElementRef(0));
        Assert.AreEqual((byte)0x12, p.ElementRef(1));
        GC.KeepAlive(p);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<array<byte>>, WeakReference) PortOfAStructNothingElseHolds()
    {
        ж<Named> named = NewNamed();

        return (PortOf(named), new WeakReference(named));
    }

    /// <summary>
    /// A view longer than its field: the fields after it have no address to read, so it is refused by name where
    /// it is made, with both sizes. Over a struct with no reference the same view is legal (V2 reads on inside the
    /// root's storage); the difference is stated in the reference page.
    /// </summary>
    [TestMethod]
    public void AViewLongerThanItsFieldIsRefusedByName()
    {
        ж<Named> named = NewNamed();
        string text = PanicText(() => @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑPort), 4));

        StringAssert.Contains(text, "(*[4]uint8)(unsafe.Pointer(&p.f))");
        StringAssert.Contains(text, "the view is 4 bytes and the field holds 2");
        StringAssert.Contains(text, "holds references");
        Assert.IsFalse(text.Contains("native memory"), text);
        Assert.AreEqual((ushort)0x1234, named.Value.Port);

        text = PanicText(() => @unsafe.ArrayPointer<ushort>.Of(named.of(Named.ᏑPort), 2));

        StringAssert.Contains(text, "the view is 4 bytes and the field holds 2");
    }

    /// <summary>
    /// A field whose converted form holds a reference is not contiguous bytes: refused by name.
    /// </summary>
    [TestMethod]
    public void AFieldThatHoldsAReferenceIsRefusedByName()
    {
        ж<Named> named = NewNamed();
        string text = PanicText(() => @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑName), 8));

        StringAssert.Contains(text, "(*[8]uint8)(unsafe.Pointer(&p.f))");
        StringAssert.Contains(text, "holds a reference");
        Assert.AreEqual("x", (string)named.Value.Name);
    }

    /// <summary>
    /// There is no <c>array&lt;T&gt;</c> behind the view: its Value refuses by name, and so do the forms that need
    /// storage a slice or a pointer can name (a slice of it, the address of an element). The refusal says which
    /// forms are served.
    /// </summary>
    [TestMethod]
    public void TheValueAndTheFormsThatNeedNamedStorageRefuseByName()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        foreach (Action form in new Action[] { () => _ = p.Value, () => _ = p.ValueSlot, () => _ = p.slice(), () => _ = p.slice(0, 1), () => _ = p.at<byte>(0) })
        {
            string text = PanicText(form);

            StringAssert.Contains(text, "a *[2]uint8 over a field of a Go value that has no address");
            StringAssert.Contains(text, "has no array<T> to dereference");
            StringAssert.Contains(text, "Index it (p[i]), load it (*p) or store it (*p = v)");
        }

        Assert.AreEqual((ushort)0x1234, named.Value.Port);
    }

    /// <summary>
    /// An index outside N is Go's panic, for every index type, and an unsigned one is checked at its full value.
    /// </summary>
    [TestMethod]
    public void AnIndexOutsideTheViewIsGosPanic()
    {
        ж<Named> named = NewNamed();
        ж<array<byte>> p = PortOf(named);

        StringAssert.Contains(PanicText(() => _ = p.ElementRef(2)), "index out of range [2] with length 2");
        StringAssert.Contains(PanicText(() => _ = p.ElementRef((nint)(-1))), "index out of range [-1]");
        StringAssert.Contains(PanicText(() => _ = p.ElementRef(ulong.MaxValue)), "index out of range");
        Assert.AreEqual((ushort)0x1234, named.Value.Port);
    }

    /// <summary>
    /// Identity is the field's: two views of one field are one Go pointer, views of two fields of one struct are
    /// not, and neither are views of one field of two structs. A view is never nil.
    /// </summary>
    [TestMethod]
    public void TwoViewsAreEqualExactlyWhenTheirFieldsAre()
    {
        ж<Named> x = NewNamed(), y = NewNamed();
        ж<array<byte>> p = PortOf(x), again = PortOf(x);
        ж<array<byte>> family = @unsafe.ArrayPointer<byte>.Of(x.of(Named.ᏑFamily), 2);
        ж<array<byte>> other = PortOf(y);

        Assert.IsTrue(p == again, "two views of one field");
        Assert.IsTrue(p.Equals(again));
        Assert.AreEqual(p.GetHashCode(), again.GetHashCode());
        Assert.IsFalse(p == family, "two fields of one struct");
        Assert.IsFalse(p == other, "one field of two structs");
        Assert.IsFalse(p == nil, "a view is never nil");
        Assert.IsFalse(p.IsNilPointer);
    }

    /// <summary>
    /// Go's <c>unsafe.Pointer(p) == unsafe.Pointer(&amp;x.f)</c> is true: the view's address is the field's. It is
    /// true here through the field pointer's identity, and false against another field and against another struct.
    /// </summary>
    [TestMethod]
    public void TheViewAsAnUnsafePointerIsTheFieldsPointer()
    {
        ж<Named> x = NewNamed(), y = NewNamed();
        ж<array<byte>> p = PortOf(x);

        @unsafe.Pointer view = @unsafe.Pointer.FromBox(p);

        Assert.IsTrue(view == @unsafe.Pointer.FromPinnedBox(x.of(Named.ᏑPort)), "unsafe.Pointer(p) == unsafe.Pointer(&x.f)");
        Assert.AreEqual(view.GetHashCode(), @unsafe.Pointer.FromPinnedBox(x.of(Named.ᏑPort)).GetHashCode(), "and they hash alike, as a map key must");
        Assert.IsTrue(view == @unsafe.Pointer.FromBox(PortOf(x)), "and equal to a second view's");
        Assert.IsFalse(view == @unsafe.Pointer.FromPinnedBox(x.of(Named.ᏑFamily)), "another field of x");
        Assert.IsFalse(view == @unsafe.Pointer.FromBox(@unsafe.ArrayPointer<byte>.Of(x.of(Named.ᏑFamily), 2)), "a view of another field of x");
        Assert.IsFalse(view == @unsafe.Pointer.FromPinnedBox(y.of(Named.ᏑPort)), "the same field of another struct");
        Assert.IsFalse(view == @unsafe.Pointer.FromBox(PortOf(y)), "a view of the same field of another struct");
        Assert.IsFalse(view == nil);

        // The equality holds across a move, because it is the field's identity and not a number.
        Collect();
        Assert.IsTrue(@unsafe.Pointer.FromBox(p) == @unsafe.Pointer.FromPinnedBox(x.of(Named.ᏑPort)), "after a collection");
        Assert.IsTrue(view == @unsafe.Pointer.FromBox(p), "a pointer minted before the collection equals one minted after it");
    }

    /// <summary>
    /// <c>uintptr(unsafe.Pointer(p))</c> is the field pointer's own number: nothing is minted for the view, and its
    /// order token is the field's token. Read with no collection between, two reads are equal and equal the
    /// field's. The field of a struct that holds references has an address nothing holds still, so the number is
    /// that address at the moment it is read, as <c>uintptr(unsafe.Pointer(&amp;x.f))</c> is.
    /// </summary>
    [TestMethod]
    public void TheViewsNumberIsTheFieldsOwn()
    {
        ж<Named> x = NewNamed();
        ж<Named> y = NewNamed();
        ж<array<byte>> p = PortOf(x);
        ж<ushort> field = x.of(Named.ᏑPort);

        Assert.AreEqual(field.PointerOrderToken, p.PointerOrderToken, "the view's token is the field's");
        Assert.AreNotEqual(x.of(Named.ᏑFamily).PointerOrderToken, p.PointerOrderToken, "and not another field's");
        Assert.AreNotEqual(y.of(Named.ᏑPort).PointerOrderToken, p.PointerOrderToken, "nor another struct's");
        Assert.AreEqual(PointerStorage.None, p.StorageKind, "the view has no storage of its own");
        Assert.AreEqual((nuint)0, p.NativeAddress, "and is no native pointer");

        Assert.IsTrue(GC.TryStartNoGCRegion(1 << 20), "no collection may run between the reads below");

        try
        {
            nuint first = (nuint)((uintptr)@unsafe.Pointer.FromBox(p)).Value;
            nuint second = (nuint)((uintptr)@unsafe.Pointer.FromBox(p)).Value;
            nuint fields = (nuint)((uintptr)@unsafe.Pointer.FromPinnedBox(field)).Value;
            nuint direct = NumberOf(p);
            nuint directField = NumberOf(field);

            Assert.AreNotEqual((nuint)0, first);
            Assert.AreEqual(first, second, "stable across two reads");
            Assert.AreEqual(fields, first, "uintptr(unsafe.Pointer(p)) == uintptr(unsafe.Pointer(&x.f))");
            Assert.AreEqual(directField, direct, "the pointer's own conversion answers the field's number too");
            Assert.AreEqual(fields, direct);
        }
        finally
        {
            GC.EndNoGCRegion();
        }
    }

    /// <summary>
    /// A nil pointer to the struct: the field's pointer is nil, and so is the view, as
    /// <c>(*[2]byte)(unsafe.Pointer(nil))</c> is.
    /// </summary>
    [TestMethod]
    public void AViewOfANilFieldPointerIsNil()
    {
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(go.ж<ushort>.NilBox, 2);

        Assert.IsTrue(p == nil);
    }
}
