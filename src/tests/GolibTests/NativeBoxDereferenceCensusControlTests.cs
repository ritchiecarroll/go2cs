using System;
using System.Runtime.InteropServices;
using go;
using sync = go.sync_package;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The positive control for the native-box dereference census (docs/phase4/DESIGN-interior-pointer-root.md 10.5,
/// "B0"). A class that reads 0 over a banked row is a reading only if its counter can be made to move, so every
/// class is driven here on purpose and each assertion is that the counter MOVED across one dereference, by the row
/// that names it.
/// </summary>
/// <remarks>
/// The counters are off unless <c>GO2CS_NATIVEBOX_CENSUS</c> is set, and the variable is read once. With it off the
/// driven arms report NOT MEASURED, never green, and the one arm that runs asserts that nothing is counted.
/// </remarks>
[TestClass]
public unsafe class NativeBoxDereferenceCensusControlTests
{
    // A struct that holds a reference and whose Go layout IS known: its string field is a managed reference where Go
    // has 16 inline bytes, so a whole-value read of it out of native memory is refused by name, before the census.
    private struct Named
    {
        internal string name;
        internal nint scalar;

        internal static ref nint Ꮡscalar(ref Named instance) => ref instance.scalar;
    }

    // A struct that holds a reference the layout check does not refuse: an array field with no declared length reads
    // as zero bytes of Go memory, and a zero-size field is skipped, yet the CLR field holds the array's reference.
    // Nothing proves this layout wrong, so a whole-value read is not refused. That is B2b.
    private struct Unsized
    {
        internal nint scalar;
        internal array<long> items;
    }

    private static void RequireTheCensus()
    {
        if (!NativeBoxDereferenceCensus.Enabled)
            Assert.Inconclusive("NOT MEASURED: the native-box dereference census is off. Set GO2CS_NATIVEBOX_CENSUS to run this control.");
    }

    // Zeroed C memory: every reference read out of it is nil.
    private static nuint Zeroed(nuint bytes = 64) => (nuint)NativeMemory.AllocZeroed(bytes);

    [TestMethod]
    public void WithTheCensusOffNothingIsCounted()
    {
        if (NativeBoxDereferenceCensus.Enabled)
            Assert.Inconclusive("the census is on: this arm is the off half of the control.");

        nuint memory = Zeroed();

        try
        {
            _ = new NativeBox<Func<long, long>>(memory).Value;
            _ = new NativeBox<Unsized>(memory).Value;

            Assert.AreEqual((0L, 0L, 0L, 0L), NativeBoxDereferenceCensus.Snapshot());
            Assert.IsNull(NativeBoxDereferenceCensus.StartBlockPath, "and no file was started");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void APointeeThatHoldsNoReferenceIsCountedInTheTotalAndInNoClass()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            var before = NativeBoxDereferenceCensus.Snapshot();

            new NativeBox<long>(memory).Value = 7;
            Assert.AreEqual(7L, new NativeBox<long>(memory).Value);

            var after = NativeBoxDereferenceCensus.Snapshot();

            Assert.AreEqual(before.dereferences + 2, after.dereferences, "a write and a read");
            Assert.AreEqual((before.b2a, before.b2b, before.unpinnable), (after.b2a, after.b2b, after.unpinnable));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void APointeeThatIsItselfAReferenceIsB2a()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            long counter = NativeBoxDereferenceCensus.Snapshot().b2a;
            long func = NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int64,System.Int64>", "source=none");
            long pointer = NativeBoxDereferenceCensus.Rows("B2a", "go.ж<System.Int64>", "source=none");
            long text = NativeBoxDereferenceCensus.Rows("B2a", "kind=String", "T=go.string", "source=none");
            long array = NativeBoxDereferenceCensus.Rows("B2a", "kind=Array", "T=go.array<System.Int16>", "source=none");

            Assert.IsNull(new NativeBox<Func<long, long>>(memory).Value);
            Assert.IsNull(new NativeBox<ж<long>>(memory).Value);
            _ = new NativeBox<@string>(memory).Value;

            // An array VALUE: Go has its elements inline, the CLR form holds them by reference. Counted here by kind.
            _ = new NativeBox<array<short>>(memory).Value;

            Assert.AreEqual(counter + 4, NativeBoxDereferenceCensus.Snapshot().b2a);
            Assert.AreEqual(func + 1, NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int64,System.Int64>", "source=none"));
            Assert.AreEqual(pointer + 1, NativeBoxDereferenceCensus.Rows("B2a", "go.ж<System.Int64>", "source=none"));
            Assert.AreEqual(text + 1, NativeBoxDereferenceCensus.Rows("B2a", "kind=String", "T=go.string", "source=none"));
            Assert.AreEqual(array + 1, NativeBoxDereferenceCensus.Rows("B2a", "kind=Array", "T=go.array<System.Int16>", "source=none"));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void AStructThatHoldsAReferenceAndWasNotRefusedIsB2b()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            Assert.IsTrue(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<Unsized>(), "the premise: it holds a reference");
            Assert.IsNull(GoReflect.ClrLayoutMismatch(typeof(Unsized)), "and nothing proves its layout wrong, so nothing is refused");

            var before = NativeBoxDereferenceCensus.Snapshot();
            long row = NativeBoxDereferenceCensus.Rows("B2b", nameof(Unsized), "source=none");

            Assert.AreEqual((nint)0, new NativeBox<Unsized>(memory).Value.scalar);

            var after = NativeBoxDereferenceCensus.Snapshot();

            Assert.AreEqual(before.b2b + 1, after.b2b);
            Assert.AreEqual(before.b2a, after.b2a, "and it is not B2a");
            Assert.AreEqual(row + 1, NativeBoxDereferenceCensus.Rows("B2b", nameof(Unsized), "source=none"));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ARowSaysWhetherTheBoxKeepsASource()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            object source = new StandardBox<long>(1);
            long retained = NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int32>", "source=retained");
            long none = NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int32>", "source=none");

            _ = new NativeBox<Func<int>>(memory, retainedSource: source).Value;

            Assert.AreEqual(retained + 1, NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int32>", "source=retained"));
            Assert.AreEqual(none, NativeBoxDereferenceCensus.Rows("B2a", "System.Func<System.Int32>", "source=none"));
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // The census reads only what the box RETAINS, so the memory read here is C memory and the retained source is the
    // box whose storage cannot be held still: nothing in the arm depends on where the collector put anything.
    [TestMethod]
    public void ABoxDerivedFromStorageThatCannotBePinnedIsItsOwnClassWhateverThePointee()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            ж<Named> container = new StandardBox<Named>(new Named { name = "tcp", scalar = 5 });
            ж<nint> field = container.of(Named.Ꮡscalar);

            Assert.AreEqual(PointerStorage.Unpinnable, field.StorageKind, "the premise: a field of a struct that holds a reference");

            var before = NativeBoxDereferenceCensus.Snapshot();
            long row = NativeBoxDereferenceCensus.Rows("UNPINNABLE", "T=System.Int64");

            // Directly, and through a derivation (unsafe.Add's native box, which retains what the pointer carried).
            _ = new NativeBox<long>(memory, retainedSource: field).Value;
            _ = new NativeBox<long>(memory + 8, retainedSource: new NativeBox<byte>(memory + 8, retainedSource: field)).Value;

            // The twins that are NOT the class: a source that can be pinned, and no source.
            _ = new NativeBox<long>(memory, retainedSource: new StandardBox<long>(1)).Value;
            _ = new NativeBox<long>(memory).Value;

            var after = NativeBoxDereferenceCensus.Snapshot();

            Assert.AreEqual(before.unpinnable + 2, after.unpinnable);
            Assert.AreEqual(row + 2, NativeBoxDereferenceCensus.Rows("UNPINNABLE", "T=System.Int64"));
            Assert.AreEqual((before.b2a, before.b2b), (after.b2a, after.b2b), "a reference-free pointee is in neither B2 class");
            GC.KeepAlive(container);
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void ARefusedDereferenceIsNotCounted()
    {
        RequireTheCensus();

        nuint memory = Zeroed();

        try
        {
            var before = NativeBoxDereferenceCensus.Snapshot();

            // A struct whose CLR layout is provably not Go's: the layout refusal, which the census sits behind.
            Assert.IsNotNull(GoReflect.ClrLayoutMismatch(typeof(Named)), "the premise");
            Assert.ThrowsException<PanicException>(() => _ = new NativeBox<Named>(memory).Value);

            // A box over an order token names no memory: refused where the address is asked.
            Assert.ThrowsException<PanicException>(() => _ = new NativeBox<Func<int>>(memory, aliasesAnOrderToken: true).Value);

            Assert.AreEqual(before, NativeBoxDereferenceCensus.Snapshot());
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // The design named the hand-owned sync.Mutex (one C# reference field) as B2b's known member (10.5). Measured here:
    // it is not in the population. Its layout is known and its field is a managed reference where Go has inline
    // bytes, so the layout check refuses a whole-value read of it by name, and the census never sees it. The refusal
    // is asserted with the census on or off; the count only when it is on.
    [TestMethod]
    public void TheHandOwnedMutexIsRefusedByLayoutAndIsNotInThePopulation()
    {
        nuint memory = Zeroed();

        try
        {
            StringAssert.Contains(GoReflect.ClrLayoutMismatch(typeof(sync.Mutex)) ?? "", "managed reference");

            var before = NativeBoxDereferenceCensus.Snapshot();

            Assert.ThrowsException<PanicException>(() => _ = new NativeBox<sync.Mutex>(memory).Value);

            Assert.AreEqual(before, NativeBoxDereferenceCensus.Snapshot());
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    [TestMethod]
    public void TheCensusReportsToAFileAndItsRowsReconcile()
    {
        RequireTheCensus();

        nuint memory = Zeroed();
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nativebox-census-control-{Environment.ProcessId}.txt");

        try
        {
            _ = new NativeBox<Func<short>>(memory).Value;

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            NativeBoxDereferenceCensus.DumpTo(path);

            string[] lines = System.IO.File.ReadAllLines(path);

            StringAssert.StartsWith(lines[0], "NBCENSUS dereferences=", "the totals line is first");
            Assert.IsTrue(Array.Exists(lines, line => line.StartsWith("NBCENSUS-ROW", StringComparison.Ordinal) && line.Contains("B2a  kind=Func  T=System.Func<System.Int16>  source=none", StringComparison.Ordinal)), string.Join("\n", lines));
            Assert.IsTrue(Array.Exists(lines, line => line.StartsWith("NBCENSUS-FOLD", StringComparison.Ordinal)));
            Assert.IsFalse(Array.Exists(lines, line => line.StartsWith("NBCENSUS-BROKEN", StringComparison.Ordinal)), string.Join("\n", lines));

            // The START block, at the path the census itself recorded: what makes a zero a reading.
            Assert.IsNotNull(NativeBoxDereferenceCensus.StartBlockPath);
            StringAssert.Contains(System.IO.File.ReadAllText(NativeBoxDereferenceCensus.StartBlockPath!), "NBCENSUS-START");
        }
        finally
        {
            NativeMemory.Free((void*)memory);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
