using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The REFERENCE floor of the native box (docs/phase4/DESIGN-cgo-phase3-marshaling.md section 18, the s6next root): a
/// whole-value read or write of a native box whose pointee is a CLR REFERENCE type (a Go pointer <c>ж&lt;U&gt;</c>, a
/// func, an interface, and an <c>unsafe.Pointer</c> over C memory) over a real address is refused by name, beside the
/// struct floor.
/// </summary>
/// <remarks>
/// <para>
/// NOT a map, a channel, a string, a slice or an array value: golib's forms of those are VALUE types that hold a
/// reference, so the floor's test (the pointee is not a value type) does not reach them. They stay the census's B2a value
/// kinds, counted and not refused. NO EXEMPTION since seat S-B1 (W4): <c>unsafe.Pointer</c> over MANAGED Go storage is read and
/// written through the word doors (<c>unsafe.Pointer.LoadWord</c> / <c>StoreWord</c>), never through <c>Value</c>.
/// </para>
/// <para>
/// Native memory holds Go's words and never a managed reference, yet <c>Value</c> reads a <c>T</c> at the address with
/// <c>Unsafe.AsRef</c>. For a reference-type <c>T</c> that took the word for a reference: C's <c>entry.next</c>, a pointer to
/// C memory, became a managed reference to an object that does not exist, and the next dereference faulted (cgo-e2e
/// s6next, "invalid memory address or nil pointer dereference" on every hosted target). A write stored a managed
/// reference into C memory, where the collector cannot see it. The population, measured with the dereference census on
/// before the floor, is in the seat's BOARD row. In GolibTests the only reference-type reads were the census's own
/// positive controls (NativeBoxDereferenceCensusControlTests), which the floor re-points.
/// </para>
/// <para>
/// The arms keep the C word 0, so that today's unguarded read returns null rather than turning C bytes into a managed
/// reference inside the test host. They are red before the floor because nothing is refused. The cure for a nonzero
/// word, minting a native box for what it names, is the pointer-field read (section 18.2), not this floor.
/// </para>
/// </remarks>
[TestClass]
public unsafe class NativeBoxReferenceFloorTests
{
    private static nuint Zeroed(nuint bytes = 64) => (nuint)NativeMemory.AllocZeroed(bytes);

    private static PanicException Refused(Action dereference)
    {
        try
        {
            dereference();
        }
        catch (PanicException panic)
        {
            return panic;
        }

        Assert.Fail("the dereference was not refused: a native box read a managed reference out of native memory");
        return null!;
    }

    // The floor's text: the struct floor's prefix, so a reader searching a failing run uses one key, and its own reason.
    private static void AssertRefusedByName<T>(PanicException panic)
    {
        StringAssert.Contains(panic.Message, $"cannot read native memory as {GoReflect.GoTypeName(typeof(T))}:");
        StringAssert.Contains(panic.Message, "a managed reference where Go stores words");
    }

    [TestMethod]
    public void AReadOfAGoPointerOutOfNativeMemoryIsRefusedByName()
    {
        nuint memory = Zeroed();

        try
        {
            // s6next's shape: the address of a C struct's pointer field, minted as a native box of a Go pointer.
            AssertRefusedByName<ж<long>>(Refused(() => { ж<long> next = new NativeBox<ж<long>>(memory).Value; GC.KeepAlive(next); }));

            // Through a retained source too: the floor asks the pointee's type, not where the box came from.
            object source = new StandardBox<long>(1);
            AssertRefusedByName<ж<long>>(Refused(() => { ж<long> next = new NativeBox<ж<long>>(memory, retainedSource: source).Value; GC.KeepAlive(next); }));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void AReadOfAnyOtherReferencePointeeOutOfNativeMemoryIsRefusedByName()
    {
        nuint memory = Zeroed();

        try
        {
            AssertRefusedByName<Func<long, long>>(Refused(() => { Func<long, long> f = new NativeBox<Func<long, long>>(memory).Value; GC.KeepAlive(f); }));
            AssertRefusedByName<error>(Refused(() => { error e = new NativeBox<error>(memory).Value; GC.KeepAlive(e); }));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void AWriteOfAGoPointerIntoNativeMemoryIsRefusedByNameAndStoresNothing()
    {
        nuint memory = Zeroed();

        try
        {
            ж<long> target = new StandardBox<long>(9);

            AssertRefusedByName<ж<long>>(Refused(() => new NativeBox<ж<long>>(memory).Value = target));

            Assert.AreEqual(0UL, *(ulong*)memory, "the refused write left C memory as it was");
            GC.KeepAlive(target);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // unsafe.Pointer is golib's Pointer, a CLASS, so over C memory it is the reference floor's case. It is also the type
    // a cgo `void*` field has most often, and a raw C address read as a managed object reference is the read the floor
    // exists for. The exemption below is for managed storage only.
    [TestMethod]
    public void AnUnsafePointerOutOfNativeMemoryIsRefusedByName()
    {
        nuint memory = Zeroed();

        try
        {
            AssertRefusedByName<@unsafe.Pointer>(Refused(() => { @unsafe.Pointer p = new NativeBox<@unsafe.Pointer>(memory).Value; GC.KeepAlive(p); }));
            AssertRefusedByName<@unsafe.Pointer>(Refused(() => new NativeBox<@unsafe.Pointer>(memory).Value = null!));

            Assert.AreEqual(0UL, *(ulong*)memory, "the refused write left C memory as it was");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // W4 (seat S-B1, design 18.3-18.7): the exemption is DELETED. unsafe.Pointer over MANAGED Go storage was exempt while
    // the converter emitted `*p` as `p.Value`; the exempt read and write stored a CLR reference in a word the collector
    // does not scan. The converter now emits the word doors (unsafe.Pointer.LoadWord / StoreWord), which read and write
    // the word as the number it is, so `.Value` over such a box is refused by name like every other reference. The mint is
    // the converter's own: a pointer to a Go uintptr, reinterpreted as a pointer to an unsafe.Pointer.
    [TestMethod]
    public void AnUnsafePointerOverManagedGoStorageIsRefusedAndTheWordDoorsReadAndWriteIt()
    {
        ж<uintptr> backing = new StandardBox<uintptr>(0);
        @unsafe.Pointer p = @unsafe.Pointer.FromBox(backing);
        ж<@unsafe.Pointer> slot = @unsafe.Pointer.To<@unsafe.Pointer>(p);

        Assert.IsInstanceOfType(slot, typeof(NativeBox<@unsafe.Pointer>), "the premise: the reinterpret is a native box over the Go word");
        Assert.IsNotNull(((NativeBox<@unsafe.Pointer>)slot).RetainedSource, "the premise: it retains the Go storage it was derived from");

        AssertRefusedByName<@unsafe.Pointer>(Refused(() => _ = slot.Value));
        AssertRefusedByName<@unsafe.Pointer>(Refused(() => slot.Value = null!));

        Assert.IsTrue(@unsafe.Pointer.LoadWord(slot) == nil, "the word door reads a zero word as a nil unsafe.Pointer");
        @unsafe.Pointer.StoreWord(slot, @unsafe.Pointer.Nil);
        Assert.AreEqual((uintptr)0, backing.Value, "and a nil store writes a zero word");

        GC.KeepAlive(p);
    }

    // CONTROL: the floor is about the POINTEE's type. A scalar over the same memory reads and writes, and the pointer
    // word itself stays reachable as a number, which is how Go's bytes are read today.
    [TestMethod]
    public void AScalarPointeeOverTheSameMemoryStillReadsAndWrites()
    {
        nuint memory = Zeroed();

        try
        {
            new NativeBox<long>(memory).Value = 7;
            Assert.AreEqual(7L, new NativeBox<long>(memory).Value);
            Assert.AreEqual((nuint)7, new NativeBox<nuint>(memory).Value, "the word, read as a number");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    private struct Named
    {
        internal string name;
        internal nint scalar;
    }

    // CONTROL: the struct floor keeps its own reason. The two floors share a prefix and never a reason.
    [TestMethod]
    public void TheStructFloorKeepsItsOwnReason()
    {
        nuint memory = Zeroed();

        try
        {
            PanicException panic = Refused(() => { Named value = new NativeBox<Named>(memory).Value; GC.KeepAlive(value.name); });

            StringAssert.Contains(panic.Message, "its CLR layout is not its Go layout (field name");
            Assert.IsFalse(panic.Message.Contains("where Go stores words"), panic.Message);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // CONTROL: the number 0 is not an address. A nil pointer to a Go pointer answers what every nil pointer answers, the
    // fault of the read itself, never the floor's text (the named-array floor's nil conjunct, the same ruling).
    [TestMethod]
    public void ANilPointerToAGoPointerReadsAsANilPointer()
    {
        ж<ж<long>> nil = (ж<ж<long>>)(uintptr)0;
        ж<nint> control = (ж<nint>)(uintptr)0;

        Exception? expected = Caught(() => { nint read = control.Value; GC.KeepAlive(read); });
        Exception? caught = Caught(() => { ж<long> read = nil.Value; GC.KeepAlive(read); });

        Assert.IsNotNull(expected, "the control, a nil pointer to an int read through its box, must throw");
        Assert.IsNotNull(caught, "a read through a nil pointer must throw");
        Assert.AreEqual(expected.GetType(), caught.GetType(), $"a nil pointer to a Go pointer must answer what every nil pointer answers; it answered: {caught.Message}");
        Assert.IsFalse(caught.Message.Contains("where Go stores words"), "the number 0 is not an address, so the floor's text is not its answer");
    }

    private static Exception? Caught(Action read)
    {
        try
        {
            read();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}
