using System;
using System.Runtime.InteropServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo Phase 3 increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16): the void pointer's door and cmd/cgo's
// own helpers.
//
// GoCgoArgs.VoidPointer is 3.1's safety boundary. It hands C a number only when that number is nil or C memory, and
// refuses a pointer into Go memory in every form one arrives in: an unsafe.Pointer that carries its box, a number
// minted from a box and held in a variable, a token, and a view reinterpreted over a Go struct. Each refusal arm has
// its control beside it: the same door lets C memory through, so a refusal is the classification and not the door.
//
// The copy functions move BYTES. Their arms use text that is not UTF-8 and text with a NUL inside, which a text
// encoding on the path would change.
public unsafe partial class GoCgoArgsTests
{
    private struct cgoHolder
    {
        public @string Name;
        public int32 N;
    }

    private const string GoMemoryOwner = "increment 3.2";

    [TestMethod]
    public void AVoidPointerThatIsNilCrossesAsZero()
    {
        Assert.AreEqual((nuint)0, GoCgoArgs.VoidPointer(null, "f"));
        Assert.AreEqual((nuint)0, GoCgoArgs.VoidPointer(@unsafe.Pointer.Nil, "f"));
        Assert.AreEqual((nuint)0, GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)0), "f"));
    }

    // The control of every refusal arm below: C memory crosses, as a number, as a pointer minted from a native box,
    // and as that pointer's number held in a variable.
    [TestMethod]
    public void AVoidPointerToCMemoryCrossesAsItsAddress()
    {
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            nuint address = (nuint)memory;
            ж<int32> native = new NativeBox<int32>(address);

            Assert.AreEqual(address, GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)address), "f"), "a number");
            Assert.AreEqual(address, GoCgoArgs.VoidPointer(new @unsafe.Pointer(native), "f"), "a native box");

            // The converter's own mints carry the box: unsafe.Pointer(cs) over what C.CString returned is one of these.
            Assert.AreEqual(address, GoCgoArgs.VoidPointer(@unsafe.Pointer.FromPinnedBox(native), "f"), "the pinning mint");
            Assert.AreEqual(address, GoCgoArgs.VoidPointer(@unsafe.Pointer.FromBox(native), "f"), "the retaining mint");
            Assert.AreEqual(address, GoCgoArgs.HelperVoidPointer(@unsafe.Pointer.FromPinnedBox(native), "GoBytes"), "a helper's");

            uintptr number = (uintptr)native;
            Assert.AreEqual(address, GoCgoArgs.VoidPointer(new @unsafe.Pointer(number), "f"), "a native box's number");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // A C sentinel has every bit set ((void *)-1: MAP_FAILED, RTLD_NEXT). It is not a token, and it crosses.
    [TestMethod]
    public void AVoidPointerThatIsACSentinelCrosses()
    {
        nuint sentinel = unchecked((nuint)(nint)(-1));

        Assert.AreEqual(sentinel, GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)sentinel), "f"));
    }

    [TestMethod]
    public void AVoidPointerIntoGoMemoryIsRefusedBeforeTheCallInEveryFormItArrivesIn()
    {
        ж<int32> scalar = new StandardBox<int32>(7);
        ж<cgoPoint> whole = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<cgoHolder> references = new StandardBox<cgoHolder>(default(cgoHolder));
        array<int32> elements = new array<int32>(4);
        ж<int32> element = new ElemRefBox<int32>(elements, 2);

        // The number alone, held in a variable: nothing of the box travels with it.
        uintptr scalarNumber = (uintptr)scalar;
        uintptr elementNumber = (uintptr)element;

        (string form, Func<@unsafe.Pointer> mint)[] forms =
        [
            ("a pointer minted from a box of one scalar", () => new @unsafe.Pointer(scalar)),
            ("the retaining mint", () => @unsafe.Pointer.FromBox(scalar)),
            ("the pinning mint", () => @unsafe.Pointer.FromPinnedBox(scalar)),
            ("a pointer minted from a box of a struct", () => new @unsafe.Pointer(whole)),
            ("a pointer minted from a box that holds Go references", () => new @unsafe.Pointer(references)),
            ("a pointer minted from an element", () => new @unsafe.Pointer(element)),
            ("a scalar box's number from a variable", () => new @unsafe.Pointer(scalarNumber)),
            ("an element's number from a variable", () => new @unsafe.Pointer(elementNumber)),
            ("a number minted and converted back in one expression", () => new @unsafe.Pointer((uintptr)new @unsafe.Pointer(whole))),
        ];

        foreach ((string form, Func<@unsafe.Pointer> mint) in forms)
        {
            string message = "";

            try
            {
                nuint crossed = GoCgoArgs.VoidPointer(mint(), "fill");
                Assert.Fail($"{form}: crossed as {crossed:x}, and it is Go memory");
            }
            catch (PanicException refused)
            {
                message = refused.Message;
            }

            StringAssert.Contains(message, "C.fill", form);
            StringAssert.Contains(message, GoMemoryOwner, form);
        }

        GC.KeepAlive(scalar);
        GC.KeepAlive(whole);
        GC.KeepAlive(references);
        GC.KeepAlive(elements);
    }

    // §13.3 (M2): a view reinterpreted over a Go struct is a native box minted from managed storage. Its address is
    // never handed to C, whether it arrives as the view's own pointer or as the number that pointer holds.
    [TestMethod]
    public void AVoidPointerToAViewReinterpretedOverGoMemoryIsRefused()
    {
        ж<cgoPoint> source = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<int32> view = source.Reinterpret<cgoPoint, int32>();

        string asPointer = RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(view), "fill"));
        StringAssert.Contains(asPointer, GoMemoryOwner);

        uintptr number = (uintptr)view;
        string asNumber = RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(number), "fill"));
        StringAssert.Contains(asNumber, GoMemoryOwner);

        GC.KeepAlive(source);
        GC.KeepAlive(view);
    }

    // The review's question of the classifier (§16.9, COORD's ruling): can ANY box answer "C memory alone" when its
    // storage is managed? Every way golib has a pointer over managed storage is handed to the door through each of the
    // converter's own mints, through the bare mint, and as a number held in a variable.
    //
    // NO for a box: every hand-over that carries its box is refused. For a bare NUMBER there is a residue, ruled and
    // disclosed (stop line S3.1-E, docs/Limitations.md): two pin moments register nothing, so a number minted from a
    // pointer whose storage golib pinned for a reinterpret view, or from a native box over a pinned buffer, crosses.
    // This arm asserts exactly those four crossings and no others, and that every other hand-over is refused: a fifth
    // crossing is a red, and so is one of these four closing without this arm being told.
    [TestMethod]
    public void APointerOverManagedStorageCrossesAVoidPointerOnlyAsTheFourDisclosedNumbers()
    {
        array<int32> wide = new array<int32>(4);
        ж<array<int32>> wideBox = new StandardBox<array<int32>>(wide);
        array<byte> bytes = new array<byte>(16);
        ж<byte> element = new ElemRefBox<byte>(bytes, 4);
        ж<cgoPoint> standard = new StandardBox<cgoPoint>(default(cgoPoint));
        ж<cgoHolder> holder = new StandardBox<cgoHolder>(default(cgoHolder));
        byte[] pinnedBytes = new byte[32];
        PinnedBuffer pinned = new PinnedBuffer(pinnedBytes);
        @string text = new @string("go2cs"u8);
        slice<byte> sliced = new slice<byte>(new byte[8]);

        (string form, object box)[] boxes =
        [
            // A wider view of an element: not an alias, so the reinterpret pins the array and mints a native box.
            ("a wider view pinned over an element", element.Reinterpret<byte, int32>()),
            // A narrower view of a struct: an alias of the same managed storage.
            ("a view aliasing a struct", standard.Reinterpret<cgoPoint, int32>()),
            // A view of a struct that holds references: no storage to pin, so its number is a token.
            ("a view over a struct that holds references", holder.Reinterpret<cgoHolder, int32>()),
            // The two fallback arms as golib mints them: the number is the source's own, an address or a token.
            ("a native box with a retained source", new NativeBox<int32>(((uintptr)standard).Value, retainedSource: standard)),
            ("a native box that aliases an order token", new NativeBox<int32>(((uintptr)holder).Value, aliasesAnOrderToken: true)),
            ("a native box over a pinned managed buffer", new NativeBox<byte>((nuint)pinned.Pointer, pin: pinned)),
            ("unsafe.StringData", @unsafe.StringData(text)),
            ("unsafe.SliceData", @unsafe.SliceData(sliced)),
            // (*byte)(unsafe.Pointer(&array)) over a Go array of another element type: through the number, and back as
            // a native box minted with no source, no token and no pin. Its number is the registry's.
            ("a pointer rebuilt from a Go array's number", (ж<byte>)(uintptr)wideBox),
        ];

        var crossed = new System.Collections.Generic.List<string>();
        int refusals = 0;

        void Hand(string form, string mint, Func<@unsafe.Pointer> make)
        {
            try
            {
                nuint address = GoCgoArgs.VoidPointer(make(), "fill");
                Assert.AreNotEqual((nuint)0, address, $"{form}, {mint}: nil");
                crossed.Add($"{form}, {mint}");
            }
            catch (PanicException refused)
            {
                StringAssert.Contains(refused.Message, GoMemoryOwner, $"{form}, {mint}");
                refusals++;
            }
        }

        foreach ((string form, object box) in boxes)
        {
            switch (box)
            {
                case ж<int32> typed:
                    Hand(form, "the pinning mint", () => @unsafe.Pointer.FromPinnedBox(typed));
                    Hand(form, "the retaining mint", () => @unsafe.Pointer.FromBox(typed));
                    Hand(form, "the bare mint", () => new @unsafe.Pointer(typed));
                    Hand(form, "its number in a variable", () => { uintptr number = (uintptr)typed; return new @unsafe.Pointer(number); });
                    break;

                case ж<byte> typed:
                    Hand(form, "the pinning mint", () => @unsafe.Pointer.FromPinnedBox(typed));
                    Hand(form, "the retaining mint", () => @unsafe.Pointer.FromBox(typed));
                    Hand(form, "the bare mint", () => new @unsafe.Pointer(typed));
                    Hand(form, "its number in a variable", () => { uintptr number = (uintptr)typed; return new @unsafe.Pointer(number); });
                    break;

                default:
                    Assert.Fail($"{form}: a {box.GetType().Name} is not a box this arm hands over");
                    break;
            }
        }

        GC.KeepAlive(bytes);
        GC.KeepAlive(standard);
        GC.KeepAlive(holder);
        GC.KeepAlive(pinned);
        GC.KeepAlive(pinnedBytes);
        GC.KeepAlive(boxes);
        GC.KeepAlive(wideBox);

        string[] disclosed =
        [
            "a wider view pinned over an element, the bare mint",
            "a wider view pinned over an element, its number in a variable",
            "a native box over a pinned managed buffer, the bare mint",
            "a native box over a pinned managed buffer, its number in a variable",
        ];

        CollectionAssert.AreEquivalent(disclosed, crossed, "the hand-overs that crossed as C memory:\n" + string.Join("\n", crossed));
        Assert.AreEqual(boxes.Length * 4 - disclosed.Length, refusals, "every other hand-over is refused");
    }

    // The disclosed residue's other half: an INTERIOR address. The registry's entries are exact addresses, so a number
    // built by arithmetic over a Go pointer's number misses it and crosses, for every kind of pin. The exact number
    // beside it is the control, and is refused. This arm pins the gap, so the closure that ends it is told here.
    [TestMethod]
    public void ANumberBuiltByArithmeticOverAGoPointerCrossesAsDisclosed()
    {
        ж<cgoPoint> standard = new StandardBox<cgoPoint>(default(cgoPoint));
        uintptr number = (uintptr)standard;

        string exact = RefusalOf(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer(number), "fill"));
        StringAssert.Contains(exact, GoMemoryOwner);

        nuint interior = number.Value + 4;
        Assert.AreEqual(interior, GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)interior), "fill"), "DISCLOSED: an interior address crosses");

        GC.KeepAlive(standard);
    }

    // What C.free leaves behind is still a pointer to C memory, as it is in Go: Go has no check for a C pointer used
    // after it was freed, and go2cs adds none. The door classifies and never dereferences, so the freed pointer crosses.
    [TestMethod]
    public void AVoidPointerToFreedCMemoryStillCrossesAsGoLetsIt()
    {
        void* memory = NativeMemory.AllocZeroed(16);
        nuint address = (nuint)memory;
        ж<sbyte> native = new NativeBox<sbyte>(address);
        @unsafe.Pointer carried = @unsafe.Pointer.FromPinnedBox(native);

        NativeMemory.Free(memory);

        Assert.AreEqual(address, GoCgoArgs.VoidPointer(carried, "free"));
        Assert.AreEqual(address, GoCgoArgs.VoidPointer(@unsafe.Pointer.FromPinnedBox(native), "free"));
    }

    [TestMethod]
    public void AHelpersPointerIsNilOrCMemoryAndGoMemoryIsRefusedNamingTheHelper()
    {
        void* memory = NativeMemory.AllocZeroed(8);

        try
        {
            nuint address = (nuint)memory;
            ж<sbyte> native = new NativeBox<sbyte>(address);

            Assert.AreEqual((nuint)0, GoCgoArgs.HelperPointer<sbyte>(null, "GoString"));
            Assert.AreEqual(address, GoCgoArgs.HelperPointer(native, "GoString"));
            Assert.AreEqual((nuint)0, GoCgoArgs.HelperVoidPointer(null, "GoBytes"));
            Assert.AreEqual(address, GoCgoArgs.HelperVoidPointer(new @unsafe.Pointer(native), "GoBytes"));
        }
        finally
        {
            NativeMemory.Free(memory);
        }

        ж<sbyte> managed = new StandardBox<sbyte>(0);

        string typed = RefusalOf(() => GoCgoArgs.HelperPointer(managed, "GoString"));
        StringAssert.Contains(typed, "C.GoString over Go memory");
        StringAssert.Contains(typed, GoMemoryOwner);

        // (*C.char)(unsafe.Pointer(&array)): the pointer comes back from the array's number as a native box, and is
        // still Go memory (read first on the hosted end-to-end leg 43330710728, where this was read and not refused).
        ж<array<sbyte>> text = new StandardBox<array<sbyte>>(new array<sbyte>(4));
        ж<sbyte> rebuilt = (ж<sbyte>)(uintptr)text;
        Assert.IsTrue(rebuilt is NativeBox<sbyte>, "the arm's premise: the pointer is a native box");

        string overArray = RefusalOf(() => GoCgoArgs.HelperPointer(rebuilt, "GoString"));
        StringAssert.Contains(overArray, "C.GoString over Go memory");
        GC.KeepAlive(text);

        string untyped = RefusalOf(() => GoCgoArgs.HelperVoidPointer(new @unsafe.Pointer(managed), "GoBytes"));
        StringAssert.Contains(untyped, "C.GoBytes over Go memory");
        StringAssert.Contains(untyped, GoMemoryOwner);

        GC.KeepAlive(managed);
    }

    // §13.7, S5: bytes, never text. 0xFF and 0xC0 0x80 are not UTF-8, and the NUL is inside the string.
    [TestMethod]
    public void TheHelpersCopyBytesBothWays()
    {
        byte[] raw = [(byte)'a', 0xFF, (byte)'b', 0x00, 0xC0, 0x80, (byte)'c'];
        @string text = new @string(raw);

        Assert.AreEqual((nuint)(raw.Length + 1), GoCgoArgs.StringSize(text));
        Assert.AreEqual((nuint)raw.Length, GoCgoArgs.BytesSize(new slice<byte>(raw)));

        byte* memory = (byte*)NativeMemory.Alloc((nuint)(raw.Length + 2));

        try
        {
            memory[raw.Length + 1] = 0x5A;
            GoCgoArgs.CopyString(text, (nuint)memory);

            CollectionAssert.AreEqual(raw, new ReadOnlySpan<byte>(memory, raw.Length).ToArray(), "the string's own bytes");
            Assert.AreEqual((byte)0, memory[raw.Length], "the NUL after them");
            Assert.AreEqual((byte)0x5A, memory[raw.Length + 1], "and nothing past it");

            // C.GoString reads to the first NUL, which is inside this string; C.GoStringN reads the count.
            CollectionAssert.AreEqual(raw[..3], GoCgoArgs.GoString((nuint)memory).ToSpan().ToArray());
            CollectionAssert.AreEqual(raw, GoCgoArgs.GoStringN((nuint)memory, raw.Length).ToSpan().ToArray());
            CollectionAssert.AreEqual(raw[..5], GoCgoArgs.GoBytes((nuint)memory, 5).ToSpan().ToArray());

            // What comes back is a copy: C's memory changing afterwards changes nothing Go holds.
            slice<byte> copied = GoCgoArgs.GoBytes((nuint)memory, 2);
            memory[0] = (byte)'z';
            Assert.AreEqual((byte)'a', copied.ToSpan()[0]);

            NativeMemory.Fill(memory, (nuint)(raw.Length + 2), 0x5A);
            GoCgoArgs.CopyBytes(new slice<byte>(raw), (nuint)memory);

            CollectionAssert.AreEqual(raw, new ReadOnlySpan<byte>(memory, raw.Length).ToArray());
            Assert.AreEqual((byte)0x5A, memory[raw.Length], "C.CBytes writes no NUL");
        }
        finally
        {
            NativeMemory.Free(memory);
        }
    }

    // As Go's runtime answers them: a nil C string is "", a count of zero reads nothing and gives an empty slice that
    // is not nil, and a negative count of bytes panics with Go's own text.
    [TestMethod]
    public void TheHelpersAnswerNilZeroAndNegativeAsGoDoes()
    {
        Assert.AreEqual(0, GoCgoArgs.GoString(0).Length);
        Assert.AreEqual(0, GoCgoArgs.GoStringN(0, 0).Length);

        slice<byte> none = GoCgoArgs.GoBytes(0, 0);
        Assert.AreEqual(0, none.ToSpan().Length);
        Assert.IsFalse(none == nil, "an empty slice that is not nil");

        string message = RefusalOf(() => GoCgoArgs.GoBytes(0x1000, -1));
        StringAssert.Contains(message, "gobytes: length out of range");

        Assert.AreEqual((nuint)0x1000, GoCgoArgs.Allocated(0x1000));
    }
}
