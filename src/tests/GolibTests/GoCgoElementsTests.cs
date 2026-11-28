using System;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// Door 1 (docs/phase4/DESIGN-cgo-struct-array-element.md): an element of a Go array or slice of C structs handed to C.
// The door's golib half, row by row: the classification (Elements, with the backing array and the absolute index), the
// claim refused against any other claim on the same backing array (P3's F2), the check window per call form (section 4),
// the whole-array mirror with its snapshot copy-back (F1, d1), and the result mapping (F4, d3). The wrapper half is the
// e2e program's slicecols* arms (testdata/cgo/p3) and CgoStructWrapperTests' AnElementOfAGoArrayCrossesAsTheWholeArray.
[TestClass]
public unsafe class GoCgoElementsTests
{
    private struct rec
    {
        public int32 A;
        public int32 B;
    }

    private static string RefusalOf(Action call)
    {
        try
        {
            call();
        }
        catch (PanicException panic)
        {
            return panic.Message;
        }

        Assert.Fail("the call was not refused");
        return "";
    }

    private static array<rec> Records()
    {
        array<rec> records = new(3);

        for (int i = 0; i < 3; i++)
            records[i] = new rec { A = i + 1, B = 10 * (i + 1) };

        return records;
    }

    private static void CopyIn(ref rec m, in rec v, GoCgoCheck check) => m = v;

    private static void CopyOut(ref rec v, in rec m, in rec s, ReadOnlySpan<GoCgoStackCopy> copies, string function) => v = m;

    [TestMethod]
    public void AnElementOfManagedStorageAnswersElementsWithItsBackingArrayAndIndex()
    {
        array<rec> records = Records();
        GoCgoImages images = default;

        GoCgoStructPointer kind = GoCgoArgs.StructPointer<rec>(new ElemRefBox<rec>(records, 1), "fill", ref images, out _, out _, out rec[]? backing, out nint index);

        Assert.AreEqual(GoCgoStructPointer.Elements, kind);
        Assert.AreEqual(1, (int)index);
        Assert.IsNotNull(backing);
        Assert.AreEqual(2, backing![index].A, "the backing array is the program's own");
    }

    [TestMethod]
    public void AWholeStructStillTakesTheMirror()
    {
        GoCgoImages images = default;

        GoCgoStructPointer kind = GoCgoArgs.StructPointer<rec>(new StandardBox<rec>(new rec { A = 1 }), "fill", ref images, out _, out _, out rec[]? backing, out _);

        Assert.AreEqual(GoCgoStructPointer.Mirror, kind);
        Assert.IsNull(backing);
    }

    // F2: two elements of one array in one call are refused by name: go2cs would hand C two copies of one Go array.
    [TestMethod]
    public void TwoElementsOfOneArrayInOneCallAreRefusedByName()
    {
        array<rec> records = Records();
        GoCgoImages images = default;

        GoCgoArgs.StructPointer<rec>(new ElemRefBox<rec>(records, 0), "swap", ref images, out _, out _, out _, out _);
        string message = RefusalOf(() => GoCgoArgs.StructPointer<rec>(new ElemRefBox<rec>(records, 2), "swap", ref images, out _, out _, out _, out _));

        StringAssert.Contains(message, "C.swap");
        StringAssert.Contains(message, "name the same Go array");
    }

    // F2, each order against another door's claim on the same allocation.
    [TestMethod]
    public void AnElementsClaimAndAnotherClaimOnOneArrayAreRefusedInEitherOrder()
    {
        object backing = new rec[3];

        GoCgoImages first = default;
        first.ClaimElements([backing], "f");
        StringAssert.Contains(RefusalOf(() => first.ClaimOther([backing], "f")), "name the same Go array");

        GoCgoImages second = default;
        second.ClaimOther([backing], "f");
        StringAssert.Contains(RefusalOf(() => second.ClaimElements([backing], "f")), "name the same Go array");

        GoCgoImages unrelated = default;
        unrelated.ClaimElements([backing], "f");
        unrelated.ClaimOther([new rec[3]], "f");
    }

    // Section 4: the check per call form. The address form names the member's kind, the variable form says "pointer"
    // for the whole object, and the element form checks its slice from its start through its capacity.
    [TestMethod]
    public void TheWindowGivesEachFormItsCheck()
    {
        array<rec> records = Records();
        slice<rec> all = records.slice();
        slice<rec> rest = all.slice(1, 2, 3);
        object storage = all.m_array;

        Assert.AreEqual(GoCgoCheck.Kind, default(GoCgoWindow).Single);
        Assert.AreEqual(GoCgoCheck.Kind, default(GoCgoWindow).At(storage, 0));
        Assert.AreEqual(GoCgoCheck.Whole, GoCgoWindow.Variable.Single);
        Assert.AreEqual(GoCgoCheck.Whole, GoCgoWindow.Variable.At(storage, 0));

        GoCgoWindow window = GoCgoWindow.Of(rest);
        Assert.AreEqual(GoCgoCheck.Outside, window.At(storage, 0), "before the slice: Go checks nothing there");
        Assert.AreEqual(GoCgoCheck.Kind, window.At(storage, 1));
        Assert.AreEqual(GoCgoCheck.Kind, window.At(storage, 2), "through the slice's capacity");
        Assert.AreEqual(GoCgoCheck.Kind, window.At(new rec[3], 0), "another backing array is not this window's");
    }

    [TestMethod]
    public void AnElementBeforeTheWindowCarriesAGoPointerAcrossAsZero()
    {
        ж<int32> goPointer = new StandardBox<int32>(5);

        Assert.AreEqual((nuint)0, GoCgoArgs.PointerMember(goPointer, GoCgoCheck.Outside));
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.PointerMember(goPointer, GoCgoCheck.Kind)), "Go pointer to unpinned Go pointer");
        StringAssert.Contains(RefusalOf(() => GoCgoArgs.PointerMember(goPointer, GoCgoCheck.Whole)), "Go pointer to unpinned Go pointer");
    }

    // F1: only an element whose mirror bytes C changed is copied back, so a goroutine's write to another element during
    // the call survives.
    [TestMethod]
    public void OnlyTheElementsCChangedAreCopiedBackSoAConcurrentGoWriteSurvives()
    {
        array<rec> records = Records();
        rec[] backing = records.m_array;
        GoCgoElements<rec, rec> elements = new(backing, 1, default, CopyIn);
        GoCgoPin pin = elements.Pin;

        fixed (byte* pinned = pin)
        {
            rec* element = (rec*)pin.Address(pinned);
            element->A = 99;      // C writes the element it was handed
            element[1].B = 77;    // and the next one, which Go lets it touch
        }

        backing[0].A = 42;        // a goroutine's write while C blocks

        elements.CopyBack(CopyOut, default, "fill");

        Assert.AreEqual(42, backing[0].A, "an element C left alone is not copied back over the Go write");
        Assert.AreEqual(99, backing[1].A);
        Assert.AreEqual(77, backing[2].B);
    }

    // F4 and d3: a result on an element's boundary is that element, the program's own &a[k]; an interior word and the
    // address one past the end are refused by name.
    [TestMethod]
    public void AResultOnAnElementBoundaryIsThatElementAndAnyOtherIsRefused()
    {
        array<rec> records = Records();
        rec[] backing = records.m_array;
        GoCgoElements<rec, rec> elements = new(backing, 0, default, CopyIn);
        GoCgoPin pin = elements.Pin;
        nuint first;

        fixed (byte* pinned = pin)
            first = (nuint)pin.Address(pinned);

        ж<rec> second = elements.ElementAt(first + (nuint)sizeof(rec), "at");
        Assert.AreEqual(new ElemRefBox<rec>(records, 1), second, "the box equals &records[1]");
        Assert.AreEqual(2, second.Value.A);

        Assert.IsTrue(elements.Contains(first + (nuint)(3 * sizeof(rec))));
        StringAssert.Contains(RefusalOf(() => elements.ElementAt(first + 4, "at")), "inside an element");
        StringAssert.Contains(RefusalOf(() => elements.ElementAt(first + (nuint)(3 * sizeof(rec)), "at")), "one past the end");
    }
}
