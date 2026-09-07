using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The REFUSAL, in R's three states. Go's own `setField` helper is
// `*(*V)(unsafe.Add(unsafe.Pointer(&in), offset)) = value`: mint a pointer, do ARITHMETIC on it,
// convert the result back, write through it. When `in` carries a reference field its box has no
// pinnable storage, so the mint is an order token -- 4 GiB-aligned, low half zero -- and the
// arithmetic lands inside that token's own block while resolving to nothing.
//
// Before this refusal the inbound conversion answered a native box over that number and the write
// took the process down with an ACCESS VIOLATION. That is why reflect's TestIsZero reported 167
// verdicts instead of 388: not more broken, just no longer alive to report. The failure MODE is the
// entire regression, and restoring it is the entire fix.
//
// R's criterion, ratified: the SEVEN reference kinds must fail CATCHABLY, the non-reference control
// must be UNTOUCHED, and a `SURVIVED` on the seven would be a FAILED fix -- it would mean the write
// was handed an address and went through, answering the model question by fabrication. The model
// question (a Go-layout offset landing on a managed reference slot) stays open as Q74.
[TestClass]
public class TokenArithmeticRefusalTests
{
    private struct WithReference { internal string reference; internal long scalar; }
    private struct WithoutReference { internal nuint first; internal nuint second; }

    // `unsafe.Add(unsafe.Pointer(&in), offset)` then a read back through the result.
    private static ж<long> OffsetInto<T>(ж<T> box, int offset) =>
        (ж<long>)(uintptr)((nuint)(uintptr)box + (nuint)offset);

    [TestMethod]
    public void ArithmeticOnATokenIsRefusedByName_NotAnsweredWithANativeBoxOverANonAddress()
    {
        ж<WithReference> box = new StandardBox<WithReference>(new WithReference { reference = "x", scalar = 7 });

        // The premise: no pinnable storage, so the mint is a token and not an address.
        Assert.AreEqual(PointerStorage.None, box.StorageKind, "a reference-bearing pointee has no address to take");
        nuint token = (nuint)(uintptr)box;
        Assert.AreEqual(box.PointerOrderToken, token, "so the conversion hands out its order token");

        // CAUGHT-PANIC, in R's vocabulary: it fails, and the process survives to say so.
        PanicException caught = Assert.ThrowsException<PanicException>(
            () => OffsetInto(box, 8),
            "arithmetic on a token must be refused by name, not answered with a native box over a non-address");

        StringAssert.Contains(caught.Message, "no address",
            "and the refusal must SAY what it refused -- a message nobody can read is a crash with extra steps");
    }

    [TestMethod]
    public void TheNonReferenceControlIsUNTOUCHED_TheGuardAgainstARefusalDrawnTooWide()
    {
        // R's eighth row and the reason it exists: a refusal drawn too wide starts refusing the
        // honest case, and nothing else in the tree would catch it -- the row would still report
        // and the package would still pass.
        ж<WithoutReference> box = new StandardBox<WithoutReference>(new WithoutReference { first = 1, second = 2 });

        Assert.AreEqual(PointerStorage.Pinnable, box.StorageKind, "a reference-free pointee gets a real, pinnable address");

        ж<long> derived = OffsetInto(box, 0);

        Assert.IsNotNull(derived, "a real address survives arithmetic and stays usable");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void AnEXACTTokenStillResolvesToItsBox_TheRefusalDoesNotEatTheRoundTrip()
    {
        // The mint is load-bearing for %p, for identity, and for this round trip, which is why the
        // refusal sits at the MISS and not at the mint. Refusing at the mint would have been the
        // literal reading of "refuse at the token arm" and would have broken the rows that just
        // came back.
        ж<WithReference> box = new StandardBox<WithReference>(new WithReference { reference = "y", scalar = 3 });
        nuint token = (nuint)(uintptr)box;

        Assert.AreSame(box, (ж<WithReference>)(uintptr)token, "an exact token is still its box, untouched by the refusal");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void AnOrdinaryNativeAddressIsNotRefused_TheRefusalIsScopedToLiveTokenBlocks()
    {
        // The scope clause: only a number inside a LIVE registered block is refused. An address that
        // was never a token is not in scope however it was arrived at.
        ж<long> native = (ж<long>)(uintptr)(nuint)0x7FFF_0000_1234UL;

        Assert.IsTrue(native.IsNative, "a number outside every token block keeps its native-box answer");
        Assert.AreEqual((nuint)0x7FFF_0000_1234UL, native.NativeAddress);
    }
}
