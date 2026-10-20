using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using @unsafe = go.unsafe_package;

namespace GolibTests;

[TestClass]
public unsafe class UnsafeStringAliasingTests
{
    // Go's unsafe.String(ptr, len) returns a string whose bytes ARE the pointed-to memory. The
    // contract is stated as a prohibition — "the bytes passed to String must not be modified as
    // long as the returned string value exists" — and a prohibition is only meaningful because the
    // aliasing is OBSERVABLE: a write through ptr shows up in the string. The runtime's own suite
    // observes it (runtime.rawstring hands out a string and a byte slice "referring to the same
    // storage" and expects the caller to fill the slice; pinner_test's TestPinnerCgoCheckString
    // pins &b[0] and then requires the string's data pointer to name that same pinned object).
    //
    // golib's unsafe.String COPIED, and it was the ONLY member of the family that did:
    // unsafe.Slice aliases through ElementWindow.TryGet, unsafe.SliceData and unsafe.StringData both
    // hand back an ElemRefBox over the original backing. SliceData's own comment already claimed
    // "the round trip through unsafe.String/unsafe.Slice now ALIASES instead of snapshotting",
    // which was true of Slice and false of String.
    //
    // These arms pin BOTH directions. Arms 1–4 are the aliasing the cut adds — each one RED before
    // it. Arms 5–8 are the invariants the cut must not disturb: the zero-length early-out that
    // never dereferences (UnsafeStringEmpty's subject), the nil panic, the snapshot arm that still
    // serves a pointer with no element storage, and @string's instance-state footprint, asserted
    // rather than claimed because an aliasing representation is exactly the kind of change that
    // tempts a fourth field.

    private static byte[] NewBacking() => "abcdef"u8.ToArray();

    // ---- the four aliasing arms (RED before the cut) ----

    [TestMethod]
    public void MutationThroughThePointerIsObservedInTheString()
    {
        byte[] backing = NewBacking();
        slice<byte> source = new slice<byte>(backing);

        @string s = @unsafe.String(Ꮡ(source, 0), 6);

        Assert.AreEqual("abcdef", (string)s, "premise: the string reads the source's bytes");

        backing[0] = (byte)'X';

        Assert.AreEqual((byte)'X', s[0],
            "unsafe.String must ALIAS its source bytes — a write through the pointer is what Go's " +
            "\"must not be modified\" contract exists to forbid, and forbidding it is only " +
            "meaningful because it is observable");
        Assert.AreEqual("Xbcdef", (string)s);
    }

    [TestMethod]
    public void AnOffsetPointerNamesTheAbsoluteElement()
    {
        byte[] backing = NewBacking();
        slice<byte> source = new slice<byte>(backing);

        // &b[2], three bytes: the window must start at the ABSOLUTE index, not at the backing's
        // start — the same correction SliceData and StringData already carry.
        @string s = @unsafe.String(Ꮡ(source, 2), 3);

        Assert.AreEqual("cde", (string)s, "premise: the pointer names element 2, not element 0");

        backing[3] = (byte)'#';

        Assert.AreEqual((byte)'#', s[1], "the alias is off by the window's low bound");
        Assert.AreEqual("c#e", (string)s);
    }

    [TestMethod]
    public void StringDataOfUnsafeStringRoundTripsToTheSamePointer()
    {
        slice<byte> source = new slice<byte>(NewBacking());
        ж<byte> pointer = Ꮡ(source, 2);

        @string s = @unsafe.String(pointer, 3);

        // Go's identity: unsafe.StringData(unsafe.String(p, n)) == p. It holds only if the string
        // kept p's storage — a copy names a fresh allocation and compares unequal, which is the
        // shape TestPinnerCgoCheckString reads as an unpinned pointer.
        Assert.AreEqual(pointer, @unsafe.StringData(s),
            "the round trip must return the SAME element, which it can only do if unsafe.String " +
            "kept the source backing");
    }

    [TestMethod]
    public void AWriteThroughTheRebuiltWindowReachesTheSourceBacking()
    {
        byte[] backing = NewBacking();
        slice<byte> source = new slice<byte>(backing);

        @string s = @unsafe.String(Ꮡ(source, 2), 3);

        // Out through StringData and back through unsafe.Slice — the full family round trip. Every
        // other member already aliases, so this arm fails only on the String hop.
        slice<byte> rebuilt = @unsafe.Slice(@unsafe.StringData(s), 3);

        Assert.AreEqual((byte)'c', rebuilt[0], "premise: the rebuilt window reads the same bytes");

        rebuilt[0] = (byte)'Z';

        Assert.AreEqual((byte)'Z', backing[2],
            "the string's own data pointer must name the SOURCE backing, not a private copy");
        Assert.AreEqual((byte)'Z', s[0]);
    }

    // ---- the four invariance arms (GREEN before and after) ----

    [TestMethod]
    public void ZeroLengthReturnsTheEmptyStringWithoutDereferencing()
    {
        // A non-nil pointer into a ZERO-capacity backing: there is no element 0 to reach, so the
        // length check has to come first — the UnsafeStringEmpty subject (syscall.UTF16ToString
        // over an all-NUL WCHAR buffer reaches exactly this). The aliasing arm must sit BEHIND it.
        slice<byte> empty = new slice<byte>(new byte[0]);

        @string s = @unsafe.String(@unsafe.SliceData(empty), 0);

        Assert.AreEqual(0, s.Length);
        Assert.AreEqual("", (string)s);

        // Zero length over REAL storage keeps answering "" too, rather than a one-byte window.
        slice<byte> source = new slice<byte>(NewBacking());
        Assert.AreEqual("", (string)@unsafe.String(Ꮡ(source, 2), 0));
    }

    [TestMethod]
    public void NilPointerWithANonZeroLengthStillPanics()
    {
        ж<byte> nilPointer = nil;

        Assert.ThrowsException<PanicException>(() => @unsafe.String(nilPointer, 5));

        // ...and a nil pointer with a zero length is Go's legal case, not a panic.
        Assert.AreEqual("", (string)@unsafe.String(nilPointer, 0));
    }

    [TestMethod]
    public void APointerWithNoElementStorageStillReadsExactBytes()
    {
        // A heap box is not element storage, so the documented snapshot arm serves it — reads are
        // exact, writes do not reach back. The aliasing arm must not swallow this shape.
        ж<byte> box = new StandardBox<byte>((byte)0xA5);

        @string s = @unsafe.String(box, 1);

        Assert.AreEqual(1, s.Length);
        Assert.AreEqual((byte)0xA5, s[0], "the snapshot arm must still read the referent's real byte");
    }

    [TestMethod]
    public void StringCarriesNoNewInstanceState()
    {
        // The byte-cost assertion for this cut, stated as a test rather than as a claim: aliasing
        // is representable in @string's EXISTING header (backing + offset + length), so the change
        // costs +0 B per string. A fourth instance field here would be a corpus-wide byte cost on
        // every string value in the process.
        FieldInfo[] fields = typeof(go.@string)
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.AreEqual(3, fields.Length,
            "@string's instance state is the Go string header — backing, offset, length. Found: " +
            string.Join(", ", fields.Select(f => $"{f.FieldType.Name} {f.Name}")));

        CollectionAssert.AreEquivalent(
            new[] { "m_value", "m_offset", "m_length" },
            fields.Select(f => f.Name).ToArray());
    }
}
