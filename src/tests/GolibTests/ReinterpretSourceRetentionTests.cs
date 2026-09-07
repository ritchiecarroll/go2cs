using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

[TestClass]
public class ReinterpretSourceRetentionTests
{
    // THE SEAM. `(*byte)(unsafe.Pointer(&record))` over a record that carries managed references is
    // the one struct-passing shape whose managed identity the address model discards entirely: the
    // reinterpret cannot alias (a reference-bearing pointee punned as bytes would fabricate managed
    // references), so it falls to the raw-address route — and the address route's own recovery
    // seam, the provenance record, CANNOT serve it, because RegisterPinned's validate-on-read asks
    // "alive AND still pinned there" and a reference-bearing pointee has no pinnable storage to pin.
    //
    // internal/syscall/windows.NetShareAdd is the worked consumer: netapi32 reads a native
    // SHARE_INFO_2 off that address, the CLR's reference-first auto-layout puts the integer 1 under
    // shi2_path, and the process dies with 0xC0000005. The hand-owned wrapper transcribes the record
    // into a blittable mirror instead — which it can only do if it can reach the record, which is
    // what these tests hold.
    //
    // AMENDED 2026-09-05 (Q44, the managed pointer token — docs/phase4/DESIGN-managed-pointer-token.md).
    // The address route SERVES this class now: an unpinnable box hands out its ORDER TOKEN instead of
    // a movable field's address, registered so the registry resolves the number to the source box.
    // The first arm below records that flip (it was the arm asserting the miss). The retention
    // STAYS — a wrapper handed a Pointer reads its retained source first and asks the registry only
    // for a bare number — and the two recoveries must agree.

    // The shape of internal/syscall/windows.SHARE_INFO_2 as the converter emits it. What is
    // load-bearing is only that it CONTAINS MANAGED REFERENCES, so the CLR lays it out itself.
    private struct ReferenceBearingRecord
    {
        public ж<ushort> Netname;
        public uint Type;
        public ж<ushort> Path;
    }

    // The prefix-downcast idiom's shape: reflect's `(*structType)(unsafe.Pointer(t))` derives one
    // reference-bearing record from another. Nothing hands THAT pointer to native code.
    private struct ReferenceBearingView
    {
        public ж<ushort> First;
    }

    [TestMethod]
    public void ByteReinterpretOfAReferenceBearingRecordRemembersItsSource()
    {
        ReferenceBearingRecord record = default;
        record.Netname = new StandardBox<ushort>((ushort)'a');
        record.Type = 0x40000000;

        ж<ReferenceBearingRecord> source = new StandardBox<ReferenceBearingRecord>(record);

        // The premise, asserted rather than assumed: this really is the class the PINNED provenance
        // record cannot serve. If a future change gives such a box pinnable storage, this assertion
        // fails FIRST and says so, instead of the retention quietly becoming redundant.
        Assert.IsNull(source.PinnableStorage,
            "a reference-bearing pointee must have no pinnable storage — that is why its address cannot be a pinned provenance key");

        ж<byte> derived = source.Reinterpret<ReferenceBearingRecord, byte>();

        // Q44: the number an unpinnable box hands out is its ORDER TOKEN, not an address, so the
        // derived view is a native box over the token, and the ADDRESS route resolves that number
        // to the SOURCE through the token registry — the branch this arm's old message anticipated
        // ("if this ever resolves, the retention below is no longer the only recovery"). A native
        // reader of the view faults at a non-canonical address instead of reading a stale copy,
        // which is the loud failure the design chose; a boundary wrapper never reads it, it
        // recovers the record.
        nuint number = (nuint)(uintptr)derived;

        Assert.IsTrue(derived.IsNative,
            "a reference-bearing source cannot alias as bytes; the derived view names the source by number");
        Assert.AreEqual(source.PointerOrderToken, number,
            "the number is the source's order token — never a heap address the collector was not asked to hold still");
        Assert.AreSame(source, ManagedPointerTokens.Resolve(number),
            "the ADDRESS route resolves the token to the record's own box (Q44); a null here is the pre-Q44 miss coming back");

        Assert.AreSame(source, ManagedPointerTokens.ReinterpretSource(derived),
            "a hand-owned boundary wrapper must be able to recover the record it was handed as a *byte — without it the only remaining move is to read managed references out of a raw address, which is a CLR type-safety break");

        GC.KeepAlive(source);
    }

    [TestMethod]
    public void AReferenceBearingDESTINATIONIsNotRemembered()
    {
        // The NARROWING, and the reason it exists: reflect's prefix downcast is this same
        // unpinnable-source fallback and it is HOT. It neither needs the source (nothing hands that
        // pointer to native code) nor should pay for it. Deleting the destination half of the gate
        // in PointerExtensions.RemembersReinterpretSource must make this test fail.
        ReferenceBearingRecord record = default;
        record.Netname = new StandardBox<ushort>((ushort)'a');

        ж<ReferenceBearingRecord> source = new StandardBox<ReferenceBearingRecord>(record);

        ж<ReferenceBearingView> derived = source.Reinterpret<ReferenceBearingRecord, ReferenceBearingView>();

        Assert.IsNull(ManagedPointerTokens.ReinterpretSource(derived),
            "a reference-bearing destination is the prefix-downcast idiom, not the boundary idiom — it must not pay for a retention it never reads");

        GC.KeepAlive(source);
    }

    [TestMethod]
    public void AReferenceFreeSourceIsNotRemembered()
    {
        // The other half of the narrowing. A reference-free pointee HAS pinnable storage, so the
        // provenance record already answers for it — and this reinterpret does not even reach the
        // fallback, because such a pair can alias. Recording it would be duplicate state whose two
        // copies could disagree.
        ж<uint> source = new StandardBox<uint>(7u);

        Assert.IsNotNull(source.PinnableStorage,
            "the control's premise: a reference-free pointee is pinnable, which is what makes the address a usable key");

        ж<byte> derived = source.Reinterpret<uint, byte>();

        Assert.IsNull(ManagedPointerTokens.ReinterpretSource(derived),
            "a reference-free source needs no retention — the provenance record serves it");

        // ...and this is what serves it. Note WHICH box comes back: a reference-free pair does not
        // even reach the fallback — it ALIASES, so `derived` is a FieldRefBox over the source's own
        // storage and the address it reports is registered to itself. That is the positive control
        // for the null above: the recovery seam answers on this class through the ordinary route,
        // so the retention has nothing to add here.
        Assert.AreSame(derived, ManagedPointerTokens.Resolve((nuint)(uintptr)derived),
            "the address route must resolve for a pinnable source, which is what makes the retention's absence above correct rather than merely unmeasured");

        GC.KeepAlive(source);
    }

    [TestMethod]
    public void RetentionDoesNotChangeTheDerivedPointersVALUE()
    {
        // The compatibility claim, stated as a test: the retention is purely additive. Every
        // existing consumer of this fallback sees the same number and the same box kind it saw
        // before — only a caller that asks by name sees anything new.
        ReferenceBearingRecord record = default;
        record.Netname = new StandardBox<ushort>((ushort)'a');

        ж<ReferenceBearingRecord> source = new StandardBox<ReferenceBearingRecord>(record);

        nuint sourceAddress = (nuint)(uintptr)source;
        ж<byte> derived = source.Reinterpret<ReferenceBearingRecord, byte>();

        Assert.AreEqual(sourceAddress, (nuint)(uintptr)derived,
            "the derived pointer must still name the source's storage by address, exactly as it did before the retention");
        Assert.IsTrue(derived.IsNative,
            "and must still be the native-address box kind — the retention rides beside the box, never inside it");

        GC.KeepAlive(source);
    }

    [TestMethod]
    public void ASecondReinterpretOfTheSameSourceDoesNotThrow()
    {
        // ConditionalWeakTable.Add throws on a duplicate KEY, and two reinterprets of one source
        // produce two distinct derived boxes — but a caller may also reinterpret the same source
        // twice into the same call, and a repeat must never be the thing that fails.
        ReferenceBearingRecord record = default;
        record.Netname = new StandardBox<ushort>((ushort)'a');

        ж<ReferenceBearingRecord> source = new StandardBox<ReferenceBearingRecord>(record);

        ж<byte> first = source.Reinterpret<ReferenceBearingRecord, byte>();
        ж<byte> second = source.Reinterpret<ReferenceBearingRecord, byte>();

        Assert.AreSame(source, ManagedPointerTokens.ReinterpretSource(first));
        Assert.AreSame(source, ManagedPointerTokens.ReinterpretSource(second));

        GC.KeepAlive(source);
    }

    [TestMethod]
    public void AnUnrememberedPointerAnswersNull()
    {
        // The seam's null contract, which the NetShareAdd wrapper leans on to fail LOUDLY rather
        // than read a scalar as a record: a pointer that never went through the unpinnable arm has
        // no source, and says so.
        ж<byte> native = new NativeBox<byte>(0x1000);

        Assert.IsNull(ManagedPointerTokens.ReinterpretSource(native),
            "a pointer that is genuinely native has no remembered source, and a wrapper must be able to tell");
    }
}
