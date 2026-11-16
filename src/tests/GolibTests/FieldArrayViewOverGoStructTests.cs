using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The field view over a GO struct: Go's <c>(*[N]T)(unsafe.Pointer(&amp;p.f))</c>, f not a T, where p points at an
/// ordinary Go value and not at native memory (docs/phase4/DESIGN-array-pointer-doors.md, section 18). The
/// converter emits <c>@unsafe.ArrayPointer&lt;T&gt;.Of(field, N)</c> for every root.
/// </summary>
/// <remarks>
/// <para>
/// A native root takes golib's native door (NativeFieldArrayViewTests). Every other root took the pointer
/// conversion to the array type, which answered a native box of <c>array&lt;T&gt;</c> over the field: its
/// <c>Value</c> read the field's bytes as the three fields of <c>array&lt;T&gt;</c> itself, a managed reference made
/// out of content. A probe of every form read 13 panics and one silent wrong answer out of 15 over such a root.
/// </para>
/// <para>
/// The route now answers a native array pointer over the bytes the field's root holds still, which is the view
/// doors' second answer (V2): the elements are read and written in place, the view ends where the root's storage
/// ends, and it keeps the root. A root with no address, a struct that holds a reference, is viewed through the
/// field's own bytes (section 19, ReinterpretedFieldArrayViewTests): it was refused by name until then.
/// </para>
/// </remarks>
[TestClass]
public class FieldArrayViewOverGoStructTests
{
    // A Go struct with no reference in it: 16 bytes, laid out as Go lays it out.
    private struct Header
    {
        public ushort Family;
        public ushort Pad;
        public ushort Port;
        public ushort Tail;
        public ulong Word;

        // Converter-named, as the generated accessors are.
        internal static ref ushort ᏑPort(ref Header instance) => ref instance.Port;

        internal static ref ulong ᏑWord(ref Header instance) => ref instance.Word;
    }

    // A Go struct that holds a reference: its storage has no address.
    private struct Named
    {
        public @string Name;
        public ushort Port;

        internal static ref ushort ᏑPort(ref Named instance) => ref instance.Port;
    }

    private static ж<Header> NewHeader() =>
        new StandardBox<Header>(new Header { Family = 2, Port = 0x1234, Tail = 0x5678, Word = 0x1111111122222222 });

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

    /// <summary>
    /// The index form, as the converter emits it for a local and for the direct form: both bytes of the field
    /// are read in place, and a store through the view is a store to the field.
    /// </summary>
    [TestMethod]
    public void TheViewReadsAndWritesTheFieldsBytesInPlace()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        Assert.AreEqual((byte)0x34, p.ElementRef(0), "the low byte of Port");
        Assert.AreEqual((byte)0x12, p.ElementRef(1), "the high byte of Port");

        p.ElementRef(0) = 0xAB;
        p.ElementRef(1) += 2;

        Assert.AreEqual((ushort)0x14AB, header.Value.Port, "a store through the view is a store to the field");
        Assert.AreEqual((ushort)0x5678, header.Value.Tail, "and to nothing else");
    }

    /// <summary>
    /// runtime's cheaprand shape: a uint64 field viewed as two uint32.
    /// </summary>
    [TestMethod]
    public void AWordFieldIsViewedAsItsTwoHalves()
    {
        ж<Header> header = NewHeader();
        ж<array<uint>> t = @unsafe.ArrayPointer<uint>.Of(header.of(Header.ᏑWord), 2);

        Assert.AreEqual(0x22222222u, t.ElementRef(0));
        Assert.AreEqual(0x11111111u, t.ElementRef(1));

        (t.ElementRef(0), t.ElementRef(1)) = (t.ElementRef(1), 7u);

        Assert.AreEqual(0x0000000711111111ul, header.Value.Word);
    }

    /// <summary>
    /// The slice of the view and the address of one of its elements alias the field too.
    /// </summary>
    [TestMethod]
    public void TheSliceAndTheElementAddressAliasTheField()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);
        slice<byte> s = p.slice();

        Assert.AreEqual(2, len(s));
        Assert.AreEqual(2, cap(s));

        s[0] = 0x11;
        p.at<byte>(1).Value = 0xCD;

        Assert.AreEqual((ushort)0xCD11, header.Value.Port);
    }

    /// <summary>
    /// The defect itself: the view has no <c>array&lt;T&gt;</c> to hand out, and it says so. Before, <c>Value</c>
    /// answered the field's bytes read as an array header.
    /// </summary>
    [TestMethod]
    public void TheViewNeverAnswersAnArrayHeaderMadeOfTheFieldsBytes()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        StringAssert.Contains(PanicText(() => Touch(p)), "has no array<T> to dereference");
        GC.KeepAlive(header);
    }

    // A reference to the slot and no copy of it: a copy would put the fabricated reference where the collector reads.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Touch(ж<array<byte>> p)
    {
        ref array<byte> slot = ref p.Value;

        GC.KeepAlive(Unsafe.IsNullRef(ref slot));
    }

    /// <summary>
    /// Go's N is no bound on memory, and the struct is: a view longer than its field reads the fields after it,
    /// as Go does, and a view that would pass the end of the struct is refused by name.
    /// </summary>
    [TestMethod]
    public void TheViewEndsWhereTheStructEnds()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> four = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 4);

        Assert.AreEqual((byte)0x78, four.ElementRef(2), "the low byte of Tail, the field after Port");
        Assert.AreEqual((byte)0x56, four.ElementRef(3));

        // Port is at offset 4 of 16: twelve bytes remain from it.
        ж<array<byte>> rest = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 12);

        Assert.AreEqual((byte)0x11, rest.ElementRef(11), "the last byte of the struct");

        string text = PanicText(() => @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 13));

        StringAssert.Contains(text, "(*[13]uint8)(unsafe.Pointer(&p.f))");
        StringAssert.Contains(text, "would pass the end of the Go value");
    }

    /// <summary>
    /// A struct that is an element of a slice: the root that is held still is the slice's backing array, and the
    /// view ends with it.
    /// </summary>
    [TestMethod]
    public void AStructInASliceIsViewedInPlace()
    {
        slice<Header> headers = new slice<Header>(new Header[3]);

        headers[1] = new Header { Port = 0x1234 };

        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(Ꮡ(headers, 1).of(Header.ᏑPort), 2);

        p.ElementRef(1) = 0x99;

        Assert.AreEqual((ushort)0x9934, headers[1].Port);
        Assert.AreEqual((ushort)0, headers[0].Port);
        Assert.AreEqual((ushort)0, headers[2].Port);
    }

    /// <summary>
    /// The view is the only reference to the struct: collections leave the struct alive and where it was, and
    /// the view still reads and writes its field.
    /// </summary>
    [TestMethod]
    public void TheViewKeepsTheStructAndItsBytesStill()
    {
        (ж<array<byte>> p, WeakReference root) = PortOfAStructNothingElseHolds();

        Collect();

        Assert.IsTrue(root.IsAlive, "the view must keep the struct its field is in");
        Assert.AreEqual((byte)0x12, p.ElementRef(1));

        p.ElementRef(0) = 0x77;

        Assert.AreEqual((ushort)0x1277, ((ж<Header>)root.Target!).Value.Port);
        GC.KeepAlive(p);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (ж<array<byte>>, WeakReference) PortOfAStructNothingElseHolds()
    {
        ж<Header> header = NewHeader();

        return (@unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2), new WeakReference(header));
    }

    /// <summary>
    /// A struct that holds a reference has no address, so no native array pointer can be made over its field. The
    /// view is over the field's own bytes instead (section 19; its arms are ReinterpretedFieldArrayViewTests), and
    /// what cannot be viewed, here a view longer than the field, is refused by name in words that are true: such a
    /// root was refused as "native memory", which it is not.
    /// </summary>
    [TestMethod]
    public void AStructThatHoldsAReferenceIsViewedThroughItsFieldAndRefusedInWordsThatAreTrue()
    {
        ж<Named> named = new StandardBox<Named>(new Named { Name = "x", Port = 0x1234 });
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑPort), 2);

        Assert.AreEqual((nuint)0, p.NativeAddress, "no native array pointer: nothing holds the field's bytes still");
        Assert.AreEqual((byte)0x34, p.ElementRef(0));

        string text = PanicText(() => @unsafe.ArrayPointer<byte>.Of(named.of(Named.ᏑPort), 4));

        StringAssert.Contains(text, "(*[4]uint8)(unsafe.Pointer(&p.f))");
        StringAssert.Contains(text, "holds references");
        Assert.IsFalse(text.Contains("native memory"), text);
        Assert.AreEqual((ushort)0x1234, named.Value.Port);
    }

    // ---- the whole array through the view (section 18, part C) ----
    //
    // The view's Value refuses by name: there is no array<T> behind it. The converter emits these two helpers for
    // the forms that need all N elements, and `p.slice()` for a range that reads the values.

    /// <summary>
    /// Go's <c>v := *p</c>: the N elements, copied. A store through the view after the load does not reach the
    /// copy, and a store to the copy does not reach the field.
    /// </summary>
    [TestMethod]
    public void TheWholeArrayIsLoadedAsACopy()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        array<byte> v = @unsafe.ArrayPointer<byte>.Load(p);

        Assert.AreEqual(2, (int)v.Length);
        Assert.AreEqual((byte)0x34, v[0]);
        Assert.AreEqual((byte)0x12, v[1]);

        p.ElementRef(0) = 0xAB;
        Assert.AreEqual((byte)0x34, v[0], "the load is a copy");

        v[1] = 0xEE;
        Assert.AreEqual((ushort)0x12AB, header.Value.Port, "a store to the copy stays in the copy");
    }

    /// <summary>A <c>uint64</c> field loaded as its two halves, the low word first.</summary>
    [TestMethod]
    public void AWordFieldIsLoadedAsItsTwoHalves()
    {
        ж<Header> header = NewHeader();

        array<uint> v = @unsafe.ArrayPointer<uint>.Load(@unsafe.ArrayPointer<uint>.Of(header.of(Header.ᏑWord), 2));

        Assert.AreEqual(0x22222222u, v[0]);
        Assert.AreEqual(0x11111111u, v[1]);
    }

    /// <summary>
    /// Go's <c>*p = v</c>: N element stores into the field, and nothing but the field.
    /// </summary>
    [TestMethod]
    public void TheWholeArrayIsStoredInPlace()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        @unsafe.ArrayPointer<byte>.Store(p, new array<byte>(new byte[] { 0xA1, 0xA2 }));

        Assert.AreEqual((ushort)0xA2A1, header.Value.Port);
        Assert.AreEqual((ushort)0x5678, header.Value.Tail, "the field after it");
        Assert.AreEqual((ushort)0, header.Value.Pad, "the field before it");
    }

    /// <summary>
    /// An array value that was never given storage is Go's zero value: the store writes zeros.
    /// </summary>
    [TestMethod]
    public void TheZeroValueStoresZeros()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        @unsafe.ArrayPointer<byte>.Store(p, default);

        Assert.AreEqual((ushort)0, header.Value.Port);
        Assert.AreEqual((ushort)0x5678, header.Value.Tail);
    }

    /// <summary>
    /// A range that reads the values ranges the view's window, so each element is read when its round comes: a
    /// store made in round 0 is seen in round 1, as Go's range over a pointer to an array sees it.
    /// </summary>
    [TestMethod]
    public void ARangeOverTheWindowReadsEachElementInItsRound()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);
        int sum = 0, rounds = 0;

        foreach (var (i, b) in range(p.slice()))
        {
            if (i == 0)
                p.ElementRef(1) = 0x40;

            sum += b;
            rounds++;
        }

        Assert.AreEqual(2, rounds, "the range ran no rounds over the box the view was");
        Assert.AreEqual(0x34 + 0x40, sum);
    }

    /// <summary>
    /// The same two helpers over native memory, where the view is golib's native array pointer.
    /// </summary>
    [TestMethod]
    public unsafe void TheWholeArrayLoadsAndStoresOverNativeMemory()
    {
        byte* block = (byte*)System.Runtime.InteropServices.NativeMemory.AllocZeroed(8);

        try
        {
            block[2] = 0x34;
            block[3] = 0x12;

            ж<array<byte>> p = builtin.NativeArrayPointer<byte>((nuint)(block + 2), 2);
            array<byte> v = @unsafe.ArrayPointer<byte>.Load(p);

            Assert.AreEqual((byte)0x34, v[0]);
            Assert.AreEqual((byte)0x12, v[1]);

            @unsafe.ArrayPointer<byte>.Store(p, new array<byte>(new byte[] { 0xB1, 0xB2 }));

            Assert.AreEqual((byte)0xB1, block[2]);
            Assert.AreEqual((byte)0xB2, block[3]);
            Assert.AreEqual((byte)0, block[1], "the byte before the view");
            Assert.AreEqual((byte)0, block[4], "the byte after the view");
            Assert.AreEqual((byte)0x34, v[0], "the load is a copy");
        }
        finally
        {
            System.Runtime.InteropServices.NativeMemory.Free(block);
        }
    }

    /// <summary>
    /// The view's own Value still refuses by name: nothing here made an array out of the field's bytes.
    /// </summary>
    [TestMethod]
    public void TheHelpersDoNotGoThroughValue()
    {
        ж<Header> header = NewHeader();
        ж<array<byte>> p = @unsafe.ArrayPointer<byte>.Of(header.of(Header.ᏑPort), 2);

        _ = @unsafe.ArrayPointer<byte>.Load(p);

        StringAssert.Contains(PanicText(() => _ = p.Value), "has no array<T> to dereference");
    }
}
