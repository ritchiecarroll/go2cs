using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// C45's header views (docs/phase4/DESIGN-unsafe-header-views.md): a string header whose length is a uint (zeebo/xxh3's
/// <c>str{p unsafe.Pointer; l uint}</c>), a slice read through that two-word header (<c>*(*str)(unsafe.Pointer(&amp;b))</c>)
/// and a <c>[]byte</c> read as a string (<c>*(*string)(unsafe.Pointer(&amp;b))</c>). The behavioral program
/// UnsafeHeaderViewFamily compares the shapes these views let through with Go; these arms hold the refusals by name,
/// which have no Go output to compare with (Go stores through the views).
/// </summary>
/// <remarks>
/// A store reaches a header box through the generated accessor's SETTER, which writes the field and then calls
/// <see cref="ж{T}.OnHeaderStore"/>; these arms do the two steps by hand, as HeaderWriteThroughTests does.
/// </remarks>
[TestClass]
public class HeaderViewSliceOriginTests
{
    // zeebo/xxh3's header: an unsafe.Pointer and an UNSIGNED length.
    private struct UintStringHeader
    {
        public @unsafe.Pointer Data;
        public nuint Len;
    }

    [TestMethod]
    public void AUintLengthStoreCommitsTheString()
    {
        ref @string s = ref heap((@string)"abcdefgh", out ж<@string> Ꮡs);
        ж<UintStringHeader> hdr = Ꮡs.Reinterpret<@string, UintStringHeader>();

        Assert.AreEqual((nuint)8, hdr.Value.Len, "Len reads the string's length as a uint");

        hdr.Value.Len = 3;
        hdr.OnHeaderStore();

        Assert.IsTrue(s == "abc", "the store commits through the uint length, as Go's does");
    }

    [TestMethod]
    public void AUintLengthAboveTheIntRangeIsHeldNotWrapped()
    {
        ref @string s = ref heap((@string)"abcdefgh", out ж<@string> Ꮡs);
        ж<UintStringHeader> hdr = Ꮡs.Reinterpret<@string, UintStringHeader>();

        hdr.Value.Len = nuint.MaxValue;
        hdr.OnHeaderStore();

        Assert.IsTrue(s == "abcdefgh", "a length no string has is held (L2), never wrapped into a length");
        Assert.AreEqual(nuint.MaxValue, hdr.Value.Len, "and the header keeps the words it was given while the string is unchanged");

        hdr.Value.Len = 2;
        hdr.OnHeaderStore();

        Assert.IsTrue(s == "ab", "a consistent length then commits");
    }

    [TestMethod]
    public void ASliceThroughAStringHeaderReadsItsDataAndLength()
    {
        ref slice<byte> b = ref heap(new slice<byte>("ABCDEFGH"u8.ToArray())[2..6], out ж<slice<byte>> Ꮡb);
        ж<UintStringHeader> hdr = Ꮡb.Reinterpret<slice<byte>, UintStringHeader>();

        Assert.AreEqual((nuint)4, hdr.Value.Len, "Len is the slice's length, not its capacity");
        Assert.AreEqual((byte)'C', @unsafe.Pointer.To<byte>(hdr.Value.Data).Value, "Data names the slice's element 0");

        b = b[1..2];

        Assert.AreEqual((nuint)1, hdr.Value.Len, "the header reads the live slice");
        Assert.AreEqual((byte)'D', @unsafe.Pointer.To<byte>(hdr.Value.Data).Value);
    }

    [TestMethod]
    public void ANilSliceThroughAStringHeaderReadsNilAndZero()
    {
        ref slice<byte> b = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡb);
        ж<UintStringHeader> hdr = Ꮡb.Reinterpret<slice<byte>, UintStringHeader>();

        Assert.IsTrue(hdr.Value.Data.IsNull, "a nil slice's Data is nil");
        Assert.AreEqual((nuint)0, hdr.Value.Len);
    }

    [TestMethod]
    public void AStoreThroughASliceStringHeaderIsRefusedByName()
    {
        ref slice<byte> b = ref heap(new slice<byte>("ABCDEFGH"u8.ToArray()), out ж<slice<byte>> Ꮡb);
        ж<UintStringHeader> hdr = Ꮡb.Reinterpret<slice<byte>, UintStringHeader>();

        hdr.Value.Len = 3;

        PanicException refused = Assert.ThrowsException<PanicException>(hdr.OnHeaderStore);
        StringAssert.Contains(refused.Message, "slice header not committed");
        StringAssert.Contains(refused.Message, "read-only");
        Assert.AreEqual(8, (int)len(b), "the refused store never reached the slice");
        Assert.AreEqual((nuint)8, hdr.Value.Len, "and the header reads the slice again");
    }

    [TestMethod]
    public void AStoreThatBypassesTheSliceStringHeaderSetterIsRefusedOnTheNextAccess()
    {
        ref slice<byte> b = ref heap(new slice<byte>("ABCDEFGH"u8.ToArray()), out ж<slice<byte>> Ꮡb);
        ж<UintStringHeader> hdr = Ꮡb.Reinterpret<slice<byte>, UintStringHeader>();

        hdr.Value.Len = 3;

        PanicException refused = Assert.ThrowsException<PanicException>(() => _ = hdr.Value.Len);
        StringAssert.Contains(refused.Message, "slice header written through a reinterpretation");
        Assert.AreEqual(8, (int)len(b));
    }

    [TestMethod]
    public void AByteSliceReadAsAStringSharesTheLiveBytes()
    {
        ref slice<byte> c = ref heap(new slice<byte>("xyz"u8.ToArray()), out ж<slice<byte>> Ꮡc);
        ж<@string> view = Ꮡc.Reinterpret<slice<byte>, @string>();

        Assert.IsTrue(view.Value == "xyz");

        c[0] = (byte)'X';
        c = c[..2];

        Assert.IsTrue(view.Value == "Xy", "a write to the slice and a reslice are seen through the string, as Go's alias shows them");
    }

    [TestMethod]
    public void ANilByteSliceReadAsAStringIsEmpty()
    {
        ref slice<byte> c = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡc);

        Assert.AreEqual(0, (int)len(Ꮡc.Reinterpret<slice<byte>, @string>().Value));
    }

    [TestMethod]
    public void AStoreThroughAByteSliceReadAsAStringIsRefusedOnTheNextAccess()
    {
        ref slice<byte> c = ref heap(new slice<byte>("xyz"u8.ToArray()), out ж<slice<byte>> Ꮡc);
        ж<@string> view = Ꮡc.Reinterpret<slice<byte>, @string>();

        view.Value = "zz";

        PanicException refused = Assert.ThrowsException<PanicException>(() => _ = view.Value);
        StringAssert.Contains(refused.Message, "string written through a reinterpretation");
        Assert.AreEqual(3, (int)len(c), "the store never reached the slice");
    }
}
