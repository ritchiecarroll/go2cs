using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo Phase 3, increment 3.2, seat 1b (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18 and 18.12): the PINNED class.
//
// GoCgoArgs.PinVoidPointer hands C Go memory when the BOX a pointer carries names its storage (M2), and never on a
// number (M3). Its answer is a pin the wrapper holds with a `fixed` for the call. These arms score prediction P-2 (the
// flip set is 18.1's I-2 list, exactly) and P-3 (B-10) as worded. VoidPointer, the classify-only door an earlier
// wrapper calls, is unchanged and keeps refusing Go memory (GoCgoArgsTests.Phase3).
public unsafe partial class GoCgoArgsTests
{
    private const string GoPointerToUnpinned = "argument of cgo function has Go pointer to unpinned Go pointer";

    private enum Pinned { Crosses, CgoRefusal, NumberRefusal }

    // What the door made of one hand-over: crossed (with the address, read under the wrapper's own `fixed`), or refused
    // with cgo's text, or refused as Go memory with go2cs's text.
    private static (Pinned outcome, nint address, string message) PinOf(Func<@unsafe.Pointer> make)
    {
        try
        {
            GoCgoPin pin = GoCgoArgs.PinVoidPointer(make(), "fill");

            fixed (byte* pinned = pin)
                return (pin.IsGoMemory ? Pinned.Crosses : Pinned.NumberRefusal, pin.Address(pinned), pin.IsGoMemory ? "" : "crossed as a number");
        }
        catch (PanicException refused)
        {
            if (refused.Message.Contains(GoPointerToUnpinned))
                return (Pinned.CgoRefusal, 0, refused.Message);

            StringAssert.Contains(refused.Message, GoMemoryOwner);
            return (Pinned.NumberRefusal, 0, refused.Message);
        }
    }

    [TestMethod]
    public void PinnedNilAndCMemoryPinNothingAndCrossAsTheirNumber()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            nuint address = (nuint)memory;
            ж<int32> native = new NativeBox<int32>(address);

            foreach ((string form, @unsafe.Pointer? pointer, nuint expected) in new (string, @unsafe.Pointer?, nuint)[]
            {
                ("null", null, 0),
                ("nil", @unsafe.Pointer.Nil, 0),
                ("a number", new @unsafe.Pointer((uintptr)address), address),
                ("the pinning mint over C memory", @unsafe.Pointer.FromPinnedBox(native), address),
                ("the retaining mint over C memory", @unsafe.Pointer.FromBox(native), address),
            })
            {
                GoCgoPin pin = GoCgoArgs.PinVoidPointer(pointer, "f");
                Assert.IsFalse(pin.IsGoMemory, form);
                Assert.IsTrue(System.Runtime.CompilerServices.Unsafe.IsNullRef(ref pin.GetPinnableReference()), $"{form}: pins nothing");

                fixed (byte* pinned = pin)
                    Assert.AreEqual((nint)expected, pin.Address(pinned), form);
            }
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // 18.1's I-2, AVoidPointerIntoGoMemoryIsRefusedBeforeTheCallInEveryFormItArrivesIn's nine forms: the pinning and
    // retaining mints of a scalar cross (2 of 9). A box that holds references is a token (cgo's text). Every bare mint
    // and every number stays refused.
    [TestMethod]
    public void PinnedTheNineFormsOfAGoScalarCrossOnlyThroughTheMintsThatCarryTheBox()
    {
        ж<int32> scalar = new StandardBox<int32>(7);
        ж<cgoPoint> whole = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<cgoHolder> references = new StandardBox<cgoHolder>(default(cgoHolder));
        array<int32> elements = new array<int32>(4);
        ж<int32> element = new ElemRefBox<int32>(elements, 2);
        uintptr scalarNumber = (uintptr)scalar;
        uintptr elementNumber = (uintptr)element;

        (string form, Func<@unsafe.Pointer> mint, Pinned expected)[] forms =
        [
            ("a pointer minted from a box of one scalar", () => new @unsafe.Pointer(scalar), Pinned.NumberRefusal),
            ("the retaining mint", () => @unsafe.Pointer.FromBox(scalar), Pinned.Crosses),
            ("the pinning mint", () => @unsafe.Pointer.FromPinnedBox(scalar), Pinned.Crosses),
            ("a pointer minted from a box of a struct", () => new @unsafe.Pointer(whole), Pinned.NumberRefusal),
            ("a pointer minted from a box that holds Go references", () => new @unsafe.Pointer(references), Pinned.CgoRefusal),
            ("a pointer minted from an element", () => new @unsafe.Pointer(element), Pinned.NumberRefusal),
            ("a scalar box's number from a variable", () => new @unsafe.Pointer(scalarNumber), Pinned.NumberRefusal),
            ("an element's number from a variable", () => new @unsafe.Pointer(elementNumber), Pinned.NumberRefusal),
            ("a number minted and converted back in one expression", () => new @unsafe.Pointer((uintptr)new @unsafe.Pointer(whole)), Pinned.NumberRefusal),
        ];

        AssertOutcomes(forms);

        GC.KeepAlive(scalar);
        GC.KeepAlive(whole);
        GC.KeepAlive(references);
        GC.KeepAlive(elements);
    }

    // 18.1's I-2 over APointerOverManagedStorageNeverCrossesAVoidPointer's nine boxes and four mints: the pinning and
    // retaining mints of seven boxes cross (14 of 36). A view over a struct that holds references, and a native box that
    // aliases an order token, are tokens in every mint (cgo's text, 8). The bare mint and the number of every other box,
    // stay refused (14).
    [TestMethod]
    public void PinnedAPointerOverManagedStorageCrossesOnlyWhenItsBoxNamesTheStorage()
    {
        array<int32> wide = new array<int32>(4);
        ж<array<int32>> wideBox = new StandardBox<array<int32>>(wide);
        array<byte> bytes = new array<byte>(16);
        ж<byte> element = new ElemRefBox<byte>(bytes, 4);
        ж<cgoPoint> standard = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<cgoHolder> holder = new StandardBox<cgoHolder>(default(cgoHolder));
        byte[] pinnedBytes = new byte[32];
        PinnedBuffer pinned = PinnedBuffer.Of(pinnedBytes);
        @string text = new @string("go2cs"u8);
        slice<byte> sliced = new slice<byte>(new byte[8]);

        (string form, object box, Pinned carried, Pinned bare)[] boxes =
        [
            ("a wider view pinned over an element", element.Reinterpret<byte, int32>(), Pinned.Crosses, Pinned.NumberRefusal),
            ("a view aliasing a struct", standard.Reinterpret<cgoPoint, int32>(), Pinned.Crosses, Pinned.NumberRefusal),
            ("a view over a struct that holds references", holder.Reinterpret<cgoHolder, int32>(), Pinned.CgoRefusal, Pinned.CgoRefusal),
            ("a native box with a retained source", new NativeBox<int32>(((uintptr)standard).Value, retainedSource: standard), Pinned.Crosses, Pinned.NumberRefusal),
            ("a native box that aliases an order token", new NativeBox<int32>(((uintptr)holder).Value, aliasesAnOrderToken: true), Pinned.CgoRefusal, Pinned.CgoRefusal),
            ("a native box over a pinned managed buffer", new NativeBox<byte>((nuint)pinned.Pointer, pin: pinned), Pinned.Crosses, Pinned.NumberRefusal),
            ("unsafe.StringData", @unsafe.StringData(text), Pinned.Crosses, Pinned.NumberRefusal),
            ("unsafe.SliceData", @unsafe.SliceData(sliced), Pinned.Crosses, Pinned.NumberRefusal),
            // 18.1 scored this form as a number ("its native box has no source"). It is not: the number-to-pointer
            // conversion resolved to the live array box and the native box KEEPS it (NativeDerivation.KeptResolved,
            // d87e1dd863, before 18.1), so its box names the storage and it crosses (18.12 record, P-2's two misses).
            ("a pointer rebuilt from a Go array's number", (ж<byte>)(uintptr)wideBox, Pinned.Crosses, Pinned.NumberRefusal),
        ];

        var forms = new List<(string, Func<@unsafe.Pointer>, Pinned)>();

        foreach ((string form, object box, Pinned carried, Pinned bare) in boxes)
        {
            switch (box)
            {
                case ж<int32> typed:
                    forms.Add(($"{form}, the pinning mint", () => @unsafe.Pointer.FromPinnedBox(typed), carried));
                    forms.Add(($"{form}, the retaining mint", () => @unsafe.Pointer.FromBox(typed), carried));
                    forms.Add(($"{form}, the bare mint", () => new @unsafe.Pointer(typed), bare));
                    forms.Add(($"{form}, its number in a variable", () => { uintptr number = (uintptr)typed; return new @unsafe.Pointer(number); }, bare));
                    break;

                case ж<byte> typed:
                    forms.Add(($"{form}, the pinning mint", () => @unsafe.Pointer.FromPinnedBox(typed), carried));
                    forms.Add(($"{form}, the retaining mint", () => @unsafe.Pointer.FromBox(typed), carried));
                    forms.Add(($"{form}, the bare mint", () => new @unsafe.Pointer(typed), bare));
                    forms.Add(($"{form}, its number in a variable", () => { uintptr number = (uintptr)typed; return new @unsafe.Pointer(number); }, bare));
                    break;

                default:
                    Assert.Fail($"{form}: a {box.GetType().Name} is not a box this arm hands over");
                    break;
            }
        }

        AssertOutcomes(forms.ToArray());

        GC.KeepAlive(bytes);
        GC.KeepAlive(standard);
        GC.KeepAlive(holder);
        GC.KeepAlive(pinned);
        GC.KeepAlive(pinnedBytes);
        GC.KeepAlive(boxes);
        GC.KeepAlive(wideBox);
    }

    // What C receives is the address of the very bytes the box names, under the wrapper's pin, and a write through it
    // is what Go then reads: an element of a Go array, a whole scalar, and an element reached by unsafe.Add.
    [TestMethod]
    public void PinnedTheAddressCReceivesIsTheBoxsOwnBytes()
    {
        array<byte> bytes = new array<byte>(16);
        ж<byte> element = new ElemRefBox<byte>(bytes, 4);
        ж<int32> scalar = new StandardBox<int32>(7);

        foreach ((string form, Func<@unsafe.Pointer> make, Func<byte> read) in new (string, Func<@unsafe.Pointer>, Func<byte>)[]
        {
            ("an element, the pinning mint", () => @unsafe.Pointer.FromPinnedBox(element), () => bytes[4]),
            ("an element, the retaining mint", () => @unsafe.Pointer.FromBox(element), () => bytes[4]),
            ("an element reached by unsafe.Add", () => @unsafe.Add(@unsafe.Pointer.FromPinnedBox(new ElemRefBox<byte>(bytes, 1)), 5), () => bytes[6]),
            ("a scalar, the pinning mint", () => @unsafe.Pointer.FromPinnedBox(scalar), () => (byte)scalar.Value),
            ("a scalar, the retaining mint", () => @unsafe.Pointer.FromBox(scalar), () => (byte)scalar.Value),
        })
        {
            GoCgoPin pin = GoCgoArgs.PinVoidPointer(make(), "fill");
            Assert.IsTrue(pin.IsGoMemory, form);

            fixed (byte* pinned = pin)
            {
                byte* c = (byte*)pin.Address(pinned);
                *c = 0x5A;
            }

            Assert.AreEqual((byte)0x5A, read(), $"{form}: C's write is what Go reads");
        }

        GC.KeepAlive(bytes);
        GC.KeepAlive(scalar);
    }

    // B-10 (18.6 finding 3, 18.7 finding 2): Go storage that holds a published unsafe.Pointer word is a Go pointer to an
    // unpinned Go pointer, refused with cgocheck's text. The control is the same storage before the store: it crosses.
    // After the word is overwritten with nil, the storage crosses again.
    [TestMethod]
    public void PinnedB10_StorageHoldingAPublishedWordIsRefusedWithCgochecksText()
    {
        ж<uintptr> word = new StandardBox<uintptr>(0);
        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromBox(word));
        ж<cgoPoint> referent = new StandardBox<cgoPoint>(default(cgoPoint));

        Assert.AreEqual(Pinned.Crosses, PinOf(() => @unsafe.Pointer.FromBox(word)).outcome, "the control: the word's storage, before any store");

        @unsafe.Pointer.StoreWord(slot, @unsafe.Pointer.FromBox(referent));

        (Pinned stored, _, string message) = PinOf(() => @unsafe.Pointer.FromPinnedBox(word));
        Assert.AreEqual(Pinned.CgoRefusal, stored, $"a storage holding a published word: {message}");
        Assert.AreEqual(Pinned.CgoRefusal, PinOf(() => @unsafe.Pointer.FromBox(word)).outcome, "the retaining mint too");

        @unsafe.Pointer.StoreWord(slot, @unsafe.Pointer.Nil);
        Assert.AreEqual(Pinned.Crosses, PinOf(() => @unsafe.Pointer.FromBox(word)).outcome, "after the word is overwritten with nil");

        GC.KeepAlive(referent);
    }

    // A pointer to one C scalar (18.12 item 2): an element of a Go array, and a typed view over Go storage, take the
    // pinned class, and C's write through the address is what Go reads. A whole scalar in a box keeps its mirror, C
    // memory its address. A view over storage that holds references is cgo's refusal.
    [TestMethod]
    public void PinnedAScalarPointerPinsAnElementAndAViewAndKeepsTheMirrorForAWholeBox()
    {
        array<byte> bytes = new array<byte>(16);
        ж<byte> element = new ElemRefBox<byte>(bytes, 3);
        array<byte> wide = new array<byte>(16);
        ж<int32> view = new ElemRefBox<byte>(wide, 4).Reinterpret<byte, int32>();

        foreach ((string form, Func<GoCgoPin, nint> write, Func<long> read, Func<(GoCgoStructPointer, GoCgoPin)> hand) in new (string, Func<GoCgoPin, nint>, Func<long>, Func<(GoCgoStructPointer, GoCgoPin)>)[]
        {
            ("an element of a Go array", _ => 0, () => bytes[3], () => { GoCgoStructPointer k = GoCgoArgs.ScalarPointer(element, "fill", out _, out GoCgoPin g); return (k, g); }),
            ("a wider view pinned over an element", _ => 0, () => wide[4], () => { GoCgoStructPointer k = GoCgoArgs.ScalarPointer(view, "fill", out _, out GoCgoPin g); return (k, g); }),
        })
        {
            (GoCgoStructPointer kind, GoCgoPin pin) = hand();
            Assert.AreEqual(GoCgoStructPointer.Pinned, kind, form);

            fixed (byte* pinned = pin)
                *(byte*)pin.Address(pinned) = 0x2A;

            Assert.AreEqual(0x2AL, read(), $"{form}: C's write is what Go reads");
        }

        ж<int32> whole = new StandardBox<int32>(5);
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.ScalarPointer(whole, "fill", out _, out GoCgoPin none), "a whole scalar keeps its mirror");
        Assert.IsFalse(none.IsGoMemory, "and pins nothing");

        ж<cgoHolder> holder = new StandardBox<cgoHolder>(default(cgoHolder));
        string refusal = RefusalOf(() => GoCgoArgs.ScalarPointer(holder.Reinterpret<cgoHolder, int32>(), "fill", out _, out _));
        StringAssert.Contains(refusal, GoPointerToUnpinned, "a view over a struct that holds references");

        GC.KeepAlive(bytes);
        GC.KeepAlive(wide);
    }

    // cmd/cgo's helpers over Go memory (18.12 item 3, P3's confirm (A)): Go functions, so they read Go memory as Go does.
    // C.GoString reads to the NUL, C.GoStringN and C.GoBytes read their length. A read Go would make past the end of
    // the object, and a C string with no NUL inside the storage, are refused by name: the managed object is all there is.
    [TestMethod]
    public void PinnedTheHelpersReadGoMemoryAsGoDoes()
    {
        array<byte> bytes = new array<byte>(16);
        ReadOnlySpan<byte> content = "go2cs\0tail"u8;

        for (int i = 0; i < content.Length; i++)
            bytes[2 + i] = content[i];
        ж<byte> text = new ElemRefBox<byte>(bytes, 2);

        Assert.AreEqual("go2cs", GoCgoArgs.GoStringOf(text, "GoString").ToString(), "GoString, the element's box");
        Assert.AreEqual("go2cs", GoCgoArgs.GoStringOf(@unsafe.Pointer.To<byte>(@unsafe.Pointer.FromPinnedBox(text)), "GoString").ToString(), "GoString, through a view");
        Assert.AreEqual("go2", GoCgoArgs.GoStringNOf(text, 3, "GoStringN").ToString(), "GoStringN");
        Assert.AreEqual("cs\0ta", System.Text.Encoding.ASCII.GetString(GoCgoArgs.GoBytesOf(@unsafe.Pointer.FromPinnedBox(new ElemRefBox<byte>(bytes, 5)), 5, "GoBytes").ToSpan()), "GoBytes, the retaining mint");

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.GoBytesOf(@unsafe.Pointer.FromPinnedBox(text), 15, "GoBytes")), "past the end", "a read past the object");
        array<byte> unterminated = new array<byte>(4);

        for (int i = 0; i < 4; i++)
            unterminated[i] = (byte)'x';

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.GoStringOf(new ElemRefBox<byte>(unterminated, 1), "GoString")), "no NUL", "no NUL before the end");

        uintptr number = (uintptr)text;
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.GoBytesOf(new @unsafe.Pointer(number), 3, "GoBytes")), GoMemoryOwner, "a number stays refused");

        void* memory = NativeMemory.AllocZeroed(8);

        try
        {
            "abc"u8.CopyTo(new Span<byte>(memory, 3));
            Assert.AreEqual("abc", GoCgoArgs.GoStringOf(new NativeBox<byte>((nuint)memory), "GoString").ToString(), "C memory, as before");
            Assert.AreEqual(3, GoCgoArgs.GoBytesOf(new @unsafe.Pointer((uintptr)(nuint)memory), 3, "GoBytes").ToSpan().Length, "C memory, GoBytes");
        }
        finally
        {
            NativeMemory.Free(memory);
        }

        GC.KeepAlive(bytes);
    }

    private struct wordHost
    {
        public uintptr W;
        public int32 X;
    }

    // Publishes a Go pointer into the word a box names, as `*(*unsafe.Pointer)(unsafe.Pointer(p)) = unsafe.Pointer(r)`.
    private static void PublishInto<T>(ж<T> word, object referent) =>
        @unsafe.Pointer.StoreWord(@unsafe.Pointer.To<@unsafe.Pointer>(@unsafe.Pointer.FromBox(word)), @unsafe.Pointer.FromBoxObject(referent));

    // B-10's other doors (18.7 finding 2): a struct the wrapper mirrors, whose uintptr field or inline array member would
    // be copied raw into the mirror, and C.CBytes, whose bytes are copied into C memory. Each is asked before it copies;
    // each control is the same storage before the store, and C.CBytes asks only the bytes it copies.
    [TestMethod]
    public void PinnedB10_TheMirrorAndCBytesAskTheBytesTheyCopy()
    {
        ж<cgoPoint> referent = new StandardBox<cgoPoint>(default(cgoPoint));

        ж<wordHost> host = new StandardBox<wordHost>(default(wordHost));
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.StructPointer(host, "f", out _), "the control: the struct, before any store");
        PublishInto(new FieldRefBox<uintptr>(host, static p => ref ((ж<wordHost>)p).Value.W), referent);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(host, "f", out _)), GoPointerToUnpinned, "a struct whose uintptr field holds a published word");

        array<uintptr> elements = new array<uintptr>(4);
        GoCgoArgs.RequireNoPublishedWordIn(elements);
        PublishInto(new ElemRefBox<uintptr>(elements, 1), referent);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.RequireNoPublishedWordIn(elements)), GoPointerToUnpinned, "an inline array member holding a published word");

        byte[] storage = new byte[32];
        slice<byte> whole = new slice<byte>(storage);
        Assert.AreEqual((nuint)32, GoCgoArgs.BytesSize(whole), "the control: C.CBytes, before any store");
        PublishInto(new ElemRefBox<byte>(new array<byte>(storage), 16), referent);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.BytesSize(whole)), "storing a Go pointer into C memory is refused", "C.CBytes over the word");
        Assert.AreEqual((nuint)8, GoCgoArgs.BytesSize(whole[..8]), "C.CBytes over bytes before the word: crosses");

        GC.KeepAlive(referent);
        GC.KeepAlive(storage);
    }

    // The hosted Phase 3 arms (goviewfield, goviewagain, goslice, gofieldtyped, goviewname*, goindexfill, goslicefill):
    // a box DERIVED from a pinned view over a Go slice's bytes, which the view's pin holds as well. A field of the view,
    // a view of the view, an element of unsafe.Slice over it and unsafe.Add over a field each retain the view they were
    // cut from (NativeDerivation.Kept), and the door walks to it. 3.2's first hosted run read each refused, with the
    // view itself crossing; H's probe reproduced every one before the walk and none after it.
    private static ref int32 ᏑY(ref cgoPoint v) => ref v.Y;

    [TestMethod]
    public void PinnedABoxDerivedFromAPinnedViewCrossesThroughTheViewsPin()
    {
        slice<byte> bytes = new slice<byte>(new byte[16]);

        (string, Func<@unsafe.Pointer>, Pinned)[] forms =
        [
            ("the view itself", () => @unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>()), Pinned.Crosses),
            ("a field of the view", () => @unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>().of(ᏑY)), Pinned.Crosses),
            ("a view of the view", () => @unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>().Reinterpret<cgoPoint, byte>()), Pinned.Crosses),
            ("an element of unsafe.Slice over the view", () => { slice<int32> s = @unsafe.Slice(Ꮡ(bytes, 0).Reinterpret<byte, int32>(), 4); return @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 1)); }, Pinned.Crosses),
            ("unsafe.Add over a field of the view", () => @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>().of(ᏑY)), 1), Pinned.Crosses),
        ];

        AssertOutcomes(forms);

        // The read door (C.GoString) and the typed door (a char * C writes) take the same boxes.
        Assert.AreEqual(0, GoCgoArgs.GoStringOf(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>().Reinterpret<cgoPoint, byte>(), "GoString").Length);
        GoCgoArgs.ScalarPointer(Ꮡ(bytes, 0).Reinterpret<byte, cgoPoint>().of(ᏑY).Reinterpret<int32, byte>(), "fill", out _, out GoCgoPin typed);
        Assert.IsTrue(typed.IsGoMemory, "a typed pointer over a field of the view is pinned Go memory");

        GC.KeepAlive(bytes);
    }

    // A typed pointer that is a REINTERPRET view of Go storage, `(*C.char)(unsafe.Pointer(&b[4]))` and
    // `(*C.char)(unsafe.Pointer(unsafe.SliceData(b[4:])))` (the hosted arms goindexfill and goslicefill), is pinned at
    // its root's storage: C may write past one char into the slice. It took the one-scalar stack mirror before, so C
    // wrote its bytes into the stack and one came back ("g" and six zeros on all three hosted OSes). The write lands at
    // the root's own offset, and C.GoString reads through the same root.
    [TestMethod]
    public void PinnedAReinterpretViewOfGoStoragePinsItsRootSoCWritesLand()
    {
        slice<byte> bytes = new slice<byte>(new byte[16]);

        (string form, Func<ж<sbyte>> make)[] forms =
        [
            ("a reinterpret of an element", () => Ꮡ(bytes, 4).Reinterpret<byte, sbyte>()),
            ("a reinterpret of unsafe.SliceData of a reslice", () => @unsafe.SliceData(bytes[4..]).Reinterpret<byte, sbyte>()),
        ];

        foreach ((string form, Func<ж<sbyte>> make) in forms)
        {
            for (int i = 0; i < 16; i++)
                bytes[i] = 0;

            GoCgoStructPointer kind = GoCgoArgs.ScalarPointer(make(), "fill", out _, out GoCgoPin pin);
            Assert.AreEqual(GoCgoStructPointer.Pinned, kind, $"{form}: pinned at its root, never a one-char mirror");

            fixed (byte* pinned = pin)
            {
                byte* at = (byte*)pin.Address(pinned);

                for (int i = 0; i < 4; i++)
                    at[i] = (byte)('A' + i);
            }

            for (int i = 0; i < 16; i++)
                Assert.AreEqual(i is >= 4 and < 8 ? (byte)('A' + i - 4) : (byte)0, (byte)bytes[i], $"{form}: byte {i} after C writes four");

            Assert.AreEqual("ABCD", GoCgoArgs.GoStringOf(make(), "GoString").ToString(), $"{form}: C.GoString reads through the root");
        }

        GC.KeepAlive(bytes);
    }

    // P3's review of 1b at 85cdef62df (COORD inbox 20261124T173809Z), item 1 (MUST): the out-parameter mirrors its slot
    // as ONE stack word. An element of a Go array of C pointers, `C.count(&argv[0])`, would hand C a stack copy it reads or
    // fills past, silently. Refused by name; the near-miss, a plain variable (`&end`), still crosses as the word.
    [TestMethod]
    public void PinnedAnOutParameterOverAnArrayElementIsRefusedByName()
    {
        array<ж<sbyte>> argv = new array<ж<sbyte>>(3);

        StringAssert.Contains(RefusalOf(() => GoCgoArgs.OutParameter(new ElemRefBox<ж<sbyte>>(argv, 0), "count", out _, out _)),
            "an element of a Go array", "&argv[0]: C would read or fill past one stack word");

        ж<ж<sbyte>> end = new StandardBox<ж<sbyte>>((ж<sbyte>)null!);
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.OutParameter(end, "strtod", out _, out _), "the near-miss: &end crosses as one word");
    }

    private struct refHost
    {
        public object? P;
        public int32 N;
        public sbyte C;
    }

    // Item 3: a reinterpret view whose root no door can pin, over a value WIDER than the pointee, would take the one-T
    // stack mirror, and C's bytes past the first would land on the stack (the r4 class). Refused by name. The near-miss, a
    // same-size reinterpret of a one-byte field, keeps the mirror: there is nothing past the one byte for C to write.
    [TestMethod]
    public void PinnedAWideReinterpretViewNoDoorCanPinIsRefusedNotMirrored()
    {
        ж<refHost> host = new StandardBox<refHost>(default(refHost));

        ж<sbyte> wide = new FieldRefBox<int32>(host, static p => ref ((ж<refHost>)p).Value.N).Reinterpret<int32, sbyte>();
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.ScalarPointer(wide, "fill", out _, out _)),
            "C may write past one", "(*C.char)(unsafe.Pointer(&s.n)), s holding a Go reference");

        ж<byte> same = new FieldRefBox<sbyte>(host, static p => ref ((ж<refHost>)p).Value.C).Reinterpret<sbyte, byte>();
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.ScalarPointer(same, "fill", out _, out _), "the near-miss: a same-size view keeps the mirror");
    }

    // Item 4 (residual 1, endptr): inside the wrapper's `fixed`, a word C returned that lies in a pinned argument's
    // storage is refused by name. Minted as a native box, it would name Go memory that moves once the `fixed` ends.
    // Controls: the array's end (one past it), a C address, and a number argument (nothing pinned).
    [TestMethod]
    public void PinnedAWordCReturnsInsidePinnedGoMemoryIsRefusedByName()
    {
        slice<byte> bytes = new slice<byte>(new byte[16]);
        Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.ScalarPointer(Ꮡ(bytes, 0).Reinterpret<byte, sbyte>(), "strtod", out _, out GoCgoPin pin));

        string inside = "";

        fixed (byte* pinned = pin)
        {
            nuint first = (nuint)pin.Address(pinned);
            nint held = (nint)pinned;
            inside = RefusalOf(() => GoCgoArgs.RequireWordOutsidePin(first + 3, pin, (byte*)held, "strtod"));

            // The controls cross: one past the array's end is not inside it, and a C address is C memory.
            GoCgoArgs.RequireWordOutsidePin(first + 16, pin, pinned, "strtod");
            GoCgoArgs.RequireWordOutsidePin(0x1000, pin, pinned, "strtod");
        }

        StringAssert.Contains(inside, "inside the Go memory it was handed", "endptr into the pinned buffer");

        // A number argument pins nothing: its fixed is a null reference.
        GoCgoArgs.RequireWordOutsidePin(0x1000, default(GoCgoPin), null, "strtod");

        GC.KeepAlive(bytes);
    }

    // Item 5 (residual 2): C.CString asks the string's own window for a published word, the CBytes row's storage viewed
    // as a string. Over the word it refuses; over the bytes before it, it crosses.
    [TestMethod]
    public void PinnedB10_CStringAsksTheStringsOwnWindow()
    {
        ж<cgoPoint> referent = new StandardBox<cgoPoint>(default(cgoPoint));
        byte[] storage = new byte[32];
        slice<byte> whole = new slice<byte>(storage);
        @string text = @string.AliasOf(whole);
        Assert.AreSame(storage, text.Backing.array, "premise: the string is a window over the same storage");
        Assert.AreEqual((nuint)33, GoCgoArgs.StringSize(text), "the control: C.CString, before any store");

        PublishInto(new ElemRefBox<byte>(new array<byte>(storage), 16), referent);
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.StringSize(text)), "storing a Go pointer into C memory is refused", "C.CString over the word");
        Assert.AreEqual((nuint)9, GoCgoArgs.StringSize(@string.AliasOf(whole[..8])), "C.CString over bytes before the word: crosses");

        GC.KeepAlive(referent);
        GC.KeepAlive(storage);
    }

    // Item 6 (NIT): a word is pointer-sized, so a range that starts inside a word holds it. HoldsAnyEntry tested the
    // word's first byte only.
    [TestMethod]
    public void PinnedB10_ARangeStartingInsideAWordHoldsIt()
    {
        ж<cgoPoint> referent = new StandardBox<cgoPoint>(default(cgoPoint));
        byte[] storage = new byte[32];
        PublishInto(new ElemRefBox<byte>(new array<byte>(storage), 16), referent);

        Assert.IsTrue(GoPointerWords.HoldsAnyEntry(storage, 16, 24), "the control: the word's own range");
        Assert.IsTrue(GoPointerWords.HoldsAnyEntry(storage, 20, 28), "a range that starts inside the word");
        Assert.IsFalse(GoPointerWords.HoldsAnyEntry(storage, 24, 32), "the range after the word");
        Assert.IsFalse(GoPointerWords.HoldsAnyEntry(storage, 8, 16), "the range before the word");

        GC.KeepAlive(referent);
        GC.KeepAlive(storage);
    }

    private static void AssertOutcomes((string form, Func<@unsafe.Pointer> mint, Pinned expected)[] forms)
    {
        var wrong = new List<string>();

        foreach ((string form, Func<@unsafe.Pointer> mint, Pinned expected) in forms)
        {
            (Pinned outcome, _, string message) = PinOf(mint);

            if (outcome != expected)
                wrong.Add($"{form}: expected {expected}, read {outcome} ({message})");
        }

        Assert.AreEqual(0, wrong.Count, $"{wrong.Count} of {forms.Length} hand-overs read otherwise:\n" + string.Join("\n", wrong));
    }
}
