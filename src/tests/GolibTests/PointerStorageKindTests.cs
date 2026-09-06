using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// The FIELD-REFERENCE half of the Q44 token arm. It COMPLEMENTS PointerTokenConversionTests,
// whose three arms are the loud form of a token of another pointee type, a provenance entry of
// another type keeping the native route, and the void* in-operator's round trip -- and NONE of
// them could have caught this, for one structural reason: every one builds a box whose POINTEE
// is reference-bearing (`heap<ж<Pair>>`, a box of a POINTER), while this class is a field chain
// over a reference-bearing CONTAINER whose pointee is reference-FREE. The merged arm read `PinnableStorage is null` -- a question about whether
// storage can be HELD STILL -- as though it answered whether an address EXISTS, and those sets
// differ by exactly this class: FieldRefBox.PinnableStorage recurses to its parent, so a container
// with no pinnable slot answers null all the way down while the field itself names a perfectly real
// interior address.
//
// Measured consequence before the repair: `Ꮡfd.of(netFD.Ꮡpfd).of(poll.FD.ᏑSysfd)` at the
// SO_UPDATE_CONNECT_CONTEXT that ends netFD.connect handed the kernel an order token on EVERY
// Windows TCP dial -- WSAEFAULT, an ordinary error return, which is why nothing faulted and four
// banked rows went red instead. A silent sibling rode with it in the Windows `os` layer, where the
// same shape sets a volume serial to zero and returns with no error surfaced at all.
//
// The load-bearing assertion in each positive arm is the READ-THROUGH, not the inequality: a
// wrong-but-plausible number passes "is not the token" and fails only when something dereferences
// it, which is precisely how this reached a banked row instead of a guard.
[TestClass]
public class PointerStorageKindTests
{
    // A container in the shape the defect needs: a REFERENCE (so the container is reference-bearing
    // and its box gets no pinnable slot) beside reference-FREE scalars whose addresses are the ones
    // handed to native code.
    private struct Descriptor
    {
        internal string name;
        internal nint handle;

        internal static ref nint Ꮡhandle(ref Descriptor instance) => ref instance.handle;
    }

    private struct Connection
    {
        internal string network;
        internal Descriptor descriptor;
        internal nint scalar;

        internal static ref Descriptor Ꮡdescriptor(ref Connection instance) => ref instance.descriptor;
        internal static ref nint Ꮡscalar(ref Connection instance) => ref instance.scalar;
    }

    [TestMethod]
    public unsafe void AFieldReferenceOverAReferenceBearingContainerIsAnAddress_NotAnOrderToken()
    {
        ж<Connection> box = new StandardBox<Connection>(new Connection { network = "tcp", scalar = 0x5A5A });
        ж<nint> field = box.of(Connection.Ꮡscalar);

        // The container has no pinnable slot, and the field reference inherits that null by
        // recursion -- which is exactly the state that used to mint a token.
        Assert.IsNull(box.PinnableStorage, "the container is reference-bearing, so it has no pinnable slot");
        Assert.IsNull(field.PinnableStorage, "and the field reference propagates that null to its parent");

        nuint number = (nuint)(uintptr)field;

        Assert.AreNotEqual(field.PointerOrderToken, number,
            "a field reference names a real interior address; the order token is not an address");
        Assert.AreEqual((nint)0x5A5A, *(nint*)number,
            "and READING THROUGH it returns the field's value -- the assertion that a plausible wrong number fails");
    }

    [TestMethod]
    public unsafe void TwoFieldHopsAreStillAnAddress_TheExactShapeTheWindowsDialTakes()
    {
        ж<Connection> box = new StandardBox<Connection>(new Connection { network = "tcp" });
        box.Value.descriptor.handle = 0x1234;

        // `Ꮡfd.of(netFD.Ꮡpfd).of(poll.FD.ᏑSysfd)`, one hop at a time.
        ж<nint> handle = box.of(Connection.Ꮡdescriptor).of(Descriptor.Ꮡhandle);

        nuint number = (nuint)(uintptr)handle;

        Assert.AreNotEqual(handle.PointerOrderToken, number, "two hops do not stop it being an address");
        Assert.AreEqual((nint)0x1234, *(nint*)number, "and the address still reads the field the kernel would write");
    }

    [TestMethod]
    public void AReferenceBearingPointeeStaysTokenised_TheIntendedClassIsUnchanged()
    {
        // The NEGATIVE arm: the class the token arm exists for must still be tokenised. A box whose
        // POINTEE is reference-bearing has its value in m_val, a field of the box object, whose
        // address SUB-Q42's witness measured going stale.
        //
        // The pointee is made NON-NIL deliberately: the uintptr operator answers 0 for a nil
        // pointer several arms earlier, so a default-constructed box-of-pointer never reaches the
        // token arm at all and would assert nothing. (Caught by this arm failing 0-vs-token on its
        // first run -- the test's bug, not the operator's.)
        ref Connection target = ref heap(new Connection { network = "tcp" }, out ж<Connection> Сtarget);
        ref ж<Connection> inner = ref heap<ж<Connection>>(out ж<ж<Connection>> pointerToPointer);
        inner = Сtarget;

        Assert.AreEqual(pointerToPointer.PointerOrderToken, (nuint)(uintptr)pointerToPointer,
            "a reference-bearing pointee still hands out its order token, not an address");

        GC.KeepAlive(inner);
        GC.KeepAlive(target);
    }

    [TestMethod]
    public void TheThreeAnswersAreStatedPerKind_NotInferredFromPinnability()
    {
        // Assert the DECISION rather than the emitted artifact (route #8): StorageKind IS the
        // decision the operators read, so a kind answering the wrong question fails HERE rather
        // than in a banked row's dial.
        ж<Connection> container = new StandardBox<Connection>(new Connection { network = "tcp" });
        ref ж<Connection> inner = ref heap<ж<Connection>>(out ж<ж<Connection>> pointerToPointer);
        ж<long> pinnable = new StandardBox<long>(7L);

        Assert.AreEqual(PointerStorage.None, pointerToPointer.StorageKind,
            "reference-bearing pointee: no storage whose address means anything");
        Assert.AreEqual(PointerStorage.Unpinnable, container.of(Connection.Ꮡscalar).StorageKind,
            "field reference over a reference-bearing container: a real address that cannot be held still");
        Assert.AreEqual(PointerStorage.Pinnable, pinnable.StorageKind,
            "reference-free pointee: a real address, pinned before it is handed out");
    }
}
