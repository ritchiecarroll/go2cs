using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using atomic = go.@internal.runtime.atomic_package;
using @unsafe = go.unsafe_package;

namespace GolibTests;

[TestClass]
public class UnsafePointerRetentionTests
{
    // internal/runtime/atomic's four unsafe.Pointer primitives split exactly on signature
    // (the I5 ruling, 2026-08-26): the `*unsafe.Pointer` members (Casp1, storePointer,
    // casPointer) carry an aliasing ж<unsafe.Pointer> and are correct; the BARE
    // `unsafe.Pointer` members (StorepNoWB, Loadp) received a pointer whose alias the mint had
    // flattened — `unsafe.Pointer(&p[i])` emitted `FromRef(ref …)`, a transient numeric address
    // in a fresh box, so the hand-own's store landed in the argument box's own uintptr slot and
    // the memory the pointer NAMES was never written. Failing-first: the landing test below was
    // run against that emission and failed exactly as the banked 14/15 measured (both elements
    // stayed nil — recorded 2026-08-27 before the fix was cut).
    //
    // The fix is the retention family: the mint (`@unsafe.Pointer.FromBox`, which the converter
    // now emits for `unsafe.Pointer(&x)`) CARRIES the source box, and the two bare-Pointer
    // primitives recover it (StoreThrough/LoadThrough) to reach the very slot the pointer names.
    // A pointer with no recoverable referent panics by name instead of losing the write.

    // The mint the converter emits for `unsafe.Pointer(&x)` — one place, so the guard tracks the
    // emission form the corpus actually uses.
    //
    // The corpus has a SECOND door for the same Go construct, and this helper takes the one whose
    // property is under test here: FromBox retains its box (which is what StoreThrough/LoadThrough
    // recover through, the whole subject of this file) and takes its address inside a `fixed`, so the
    // number is transient. Where the operand reaches the emission as a bare box the converter mints
    // through `FromPinnedBox` instead, which retains AND takes the address from the pinning
    // conversion — see PointerMintRetentionTests, whose subject is the pin rather than the recovery.
    private static @unsafe.Pointer Mint<T>(ж<T> box)
    {
        return @unsafe.Pointer.FromBox(box);
    }

    [TestMethod]
    public void StorepNoWBLandsInTheNamedLocation()
    {
        // var p [2]*int; for i := range p { atomic.StorepNoWB(unsafe.Pointer(&p[i]), unsafe.Pointer(new(int))) }
        var p = new array<ж<int>>(2);
        var allocated = new ж<int>[2];

        for (nint i = 0; i < 2; i++)
        {
            allocated[i] = Ꮡ<int>(0);
            atomic.StorepNoWB(Mint(Ꮡ(p, i)), Mint(allocated[i]));
        }

        // The direct landing assert: the store wrote the array element itself.
        Assert.AreSame(allocated[0], p[0], "StorepNoWB must write the location the pointer names — the store was lost");
        Assert.AreSame(allocated[1], p[1], "StorepNoWB must write the location the pointer names — the store was lost");

        // Go's own assert (`p[0] == p[1]` fails the test): two distinct allocations stored.
        Assert.IsTrue(p[0] != p[1], "distinct stored pointers must read back distinct — bad escape analysis of StorepNoWB");
    }

    [TestMethod]
    public void StorepNoWBServesAPointerTypedSlotAndItsNilStore()
    {
        // A *unsafe.Pointer location reached through a BARE pointer — the slot holds the Pointer
        // VALUE itself (runtime's atomic_storep shape). Then the nil store, which must land as
        // the nil form rather than being refused.
        ref @unsafe.Pointer cell = ref heap(new @unsafe.Pointer((uintptr)0x1234), out ж<@unsafe.Pointer> Ꮡcell);
        _ = cell;

        var stored = new @unsafe.Pointer((uintptr)0x5678);
        atomic.StorepNoWB(Mint(Ꮡcell), stored);
        Assert.AreSame(stored, ~Ꮡcell, "a *unsafe.Pointer slot must receive the Pointer value itself");

        // The nil store lands as the null form, so the read-back must use the structural deref
        // (`~`), not `.Value` — a non-nil pointer HOLDING nil is exactly the value-peek case.
        atomic.StorepNoWB(Mint(Ꮡcell), nil);
        Assert.IsTrue(~Ꮡcell == nil, "the nil store must land as the nil pointer form");
    }

    [TestMethod]
    public void LoadpReadsTheStoredPointerBackAcrossACollection()
    {
        // Loadp is *(*unsafe.Pointer)(ptr). Store through the CORRECT sibling (Casp1, the
        // *unsafe.Pointer signature), then load through the bare-Pointer form. The collection in
        // the middle is what retires the old accidental aliasing: the raw transient address kept
        // working only until the collector moved the slot's storage. (The CAS runs from a non-nil
        // initial: the latched siblings' number-compare used to read `.Value`, which the nil-marked
        // form refuses — fixed since, and guarded by AtomicPointerNilCompareTests.)
        ref @unsafe.Pointer cell = ref heap(new @unsafe.Pointer((uintptr)0x1), out ж<@unsafe.Pointer> Ꮡcell);
        @unsafe.Pointer initial = cell;

        var target = Ꮡ<int>(42);
        Assert.IsTrue(atomic.Casp1(Ꮡcell, initial, Mint(target)), "fixture is inert: the CAS sibling must install the value");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        @unsafe.Pointer loaded = atomic.Loadp(Mint(Ꮡcell));

        Assert.AreSame(Ꮡcell.Value, loaded, "Loadp must return the Pointer value the slot holds");
        Assert.AreSame(target, loaded.RetainedSource, "the loaded pointer must still carry the referent the stored one carried");
    }

    [TestMethod]
    public void LoadedPointerSurvivesTheTypedCastRoundTrip()
    {
        // The consumer shape one step past Loadp: `(*int)(atomic.Loadp(…))` emits
        // `(ж<int>)(uintptr)(loaded)` — the numeric round-trip must resolve back to the very box
        // that was stored, aliasing the same storage.
        var p = new array<ж<int>>(1);
        var target = Ꮡ<int>(7);

        atomic.StorepNoWB(Mint(Ꮡ(p, 0)), Mint(target));

        @unsafe.Pointer loaded = atomic.Loadp(Mint(Ꮡ(p, 0)));
        var roundTripped = (ж<int>)(uintptr)loaded;

        Assert.AreSame(target, roundTripped, "the typed cast of a loaded pointer must recover the stored box");
        roundTripped.Value = 11;
        Assert.AreEqual(11, target.Value, "the recovered pointer must alias the original storage");
    }

    [TestMethod]
    public void NumericOnlyPointerFailsLoudNotSilent()
    {
        // The recorded residual: a pointer that is ONLY a number (no retained referent, nothing
        // the registry can recover) cannot be stored through in the managed model. Before the fix
        // this was a SILENT lost write — the exact shape the I5 ruling exists to end — so the
        // contract is a named panic, never quiet success.
        var numeric = new @unsafe.Pointer((uintptr)0x1234);
        var val = new @unsafe.Pointer((uintptr)0x5678);

        PanicException stored = Assert.ThrowsException<PanicException>(
            () => atomic.StorepNoWB(numeric, val),
            "a numeric-only pointer must refuse the store loudly");
        StringAssert.Contains(stored.Message, "no recoverable managed referent");

        PanicException loaded = Assert.ThrowsException<PanicException>(
            () => atomic.Loadp(numeric),
            "a numeric-only pointer must refuse the load loudly");
        StringAssert.Contains(loaded.Message, "no recoverable managed referent");
    }

    [TestMethod]
    public void NilBoxMintsTheZeroAddress()
    {
        // Go: `unsafe.Pointer(p)` with p nil is the 0 address — the FromRef form panicked on the
        // nil deref instead (the same defect the pointer-parameter emission fixed for the syscall
        // wrappers' idiomatic nil out-pointers; FromBox makes the general mint match).
        @unsafe.Pointer minted = Mint(go.ж<int>.NilBox);

        Assert.IsTrue(minted == nil, "the nil pointer must mint as nil");
        Assert.AreEqual((uintptr)0, (uintptr)minted, "the nil pointer's address is 0");
    }
}
