using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The WRITE direction of the slice and string header views: a header built field by field through
/// <c>(*unsafeheader.Slice)(unsafe.Pointer(&amp;alias))</c> reaches <c>alias</c>, as Go's does. internal/unsafeheader's
/// TestWriteThroughHeader (Go 1.26) is the consumer: Go passes it, and the converted host failed both subtests on
/// go2cs's own refusals (the slice header's detached-copy write, and no string header box at all).
/// </summary>
/// <remarks>
/// A store reaches the box through the generated header accessor's SETTER, which writes the field and then calls
/// <see cref="ж{T}.OnHeaderStore"/>; these arms do the same two steps by hand. After each store the box commits the
/// header's words into the source when they name a value Go could build, and holds them while they do not (limit L2).
/// A Data the box cannot resolve to the source's own element type, or to native memory, is refused by name (L3).
/// </remarks>
[TestClass]
public class HeaderWriteThroughTests
{
    // unsafeheader.Slice and runtime.slice: Data unsafe.Pointer; Len, Cap int.
    private struct SliceHeader
    {
        public @unsafe.Pointer Data;
        public nint Len;
        public nint Cap;
    }

    // unsafeheader.String and runtime.stringStruct: Data unsafe.Pointer; Len int.
    private struct StringHeader
    {
        public @unsafe.Pointer Data;
        public nint Len;
    }

    private static slice<byte> Hello() => new slice<byte>("Hello, checkptr!"u8.ToArray())[..5];

    [TestMethod]
    public void ASliceHeaderStoredInGosOrderRebuildsTheSlice()
    {
        // TestWriteThroughHeader/Slice, statement for statement.
        slice<byte> s = Hello();
        ref slice<byte> alias = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡalias);

        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0));
        hdr.OnHeaderStore();
        hdr.Value.Cap = cap(s);
        hdr.OnHeaderStore();
        hdr.Value.Len = len(s);
        hdr.OnHeaderStore();

        Assert.AreEqual(5, (int)len(alias), "len(alias) is the header's Len");
        Assert.AreEqual(16, (int)cap(alias), "cap(alias) is the header's Cap, as Go's TestWriteThroughHeader requires");
        CollectionAssert.AreEqual(s.ToArray(), alias.ToArray(), "bytes.Equal(alias, s)");

        Assert.IsTrue(Ꮡ(alias, 0).Equals(Ꮡ(s, 0)), "&alias[0] == &s[0]: the managed backing itself, not a native view of its pinned address");

        alias[0] = (byte)'J';
        Assert.AreEqual((byte)'J', s[0], "alias SHARES s's backing, as the header's Data names it");
    }

    [TestMethod]
    public void AnInconsistentHeaderReachesTheSourceOnlyOnceItIsConsistent()
    {
        // The runtime's addrRanges shape: the header names a nil array with a non-zero capacity in between. Go shows
        // those broken words; the box holds them (L2) and commits the first consistent state. Each store is followed by
        // OnHeaderStore, as the generated setter does.
        slice<byte> s = Hello();
        slice<byte> before = new slice<byte>(new byte[] { 1, 2, 3 });
        ref slice<byte> alias = ref heap(before, out ж<slice<byte>> Ꮡalias);

        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

        hdr.Value.Data = default!;
        hdr.OnHeaderStore();
        hdr.Value.Cap = cap(s);
        hdr.OnHeaderStore();
        Assert.AreEqual(3, (int)len(alias), "a nil Data with Cap > 0 is not a slice Go can build; the source keeps its last consistent value");
        Assert.AreEqual(16, (int)hdr.Value.Cap, "the held header reads back its own words, as Go's does");

        hdr.Value.Len = len(s);
        hdr.OnHeaderStore();
        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0));
        hdr.OnHeaderStore();

        Assert.AreEqual(5, (int)len(alias));
        Assert.AreEqual(16, (int)cap(alias));
        CollectionAssert.AreEqual(s.ToArray(), alias.ToArray());
    }

    [TestMethod]
    public void TheReverseStoreOrderCommitsTheSameSlice()
    {
        // Len, Cap, then Data: the first two stores name a nil array with a non-zero length, held (L2); Data commits.
        slice<byte> s = Hello();
        ref slice<byte> alias = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡalias);
        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

        hdr.Value.Len = len(s);
        hdr.OnHeaderStore();
        hdr.Value.Cap = cap(s);
        hdr.OnHeaderStore();
        Assert.IsTrue(alias == nil, "a nil Data with Len 5 is held; the variable is still nil");

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(Ꮡ(s, 0));
        hdr.OnHeaderStore();

        Assert.AreEqual(5, (int)len(alias));
        Assert.AreEqual(16, (int)cap(alias));
        Assert.IsTrue(Ꮡ(alias, 0).Equals(Ꮡ(s, 0)));
    }

    [TestMethod]
    public void AHeaderOverNativeMemoryIsTheSliceOverIt()
    {
        nint block = System.Runtime.InteropServices.Marshal.AllocHGlobal(8);

        try
        {
            System.Runtime.InteropServices.Marshal.Copy(new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 }, 0, block, 8);

            ref slice<byte> alias = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡalias);
            ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

            hdr.Value.Data = (@unsafe.Pointer)(uintptr)(nuint)block;
            hdr.OnHeaderStore();
            hdr.Value.Cap = 8;
            hdr.OnHeaderStore();
            hdr.Value.Len = 4;
            hdr.OnHeaderStore();

            Assert.AreEqual(4, (int)len(alias));
            Assert.AreEqual(8, (int)cap(alias));
            CollectionAssert.AreEqual(new byte[] { 10, 20, 30, 40 }, alias.ToArray());

            alias[1] = 99;
            Assert.AreEqual((byte)99, System.Runtime.InteropServices.Marshal.ReadByte(block, 1), "the slice ALIASES the native memory");
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void AZeroHeaderIsTheNilSlice()
    {
        ref slice<byte> alias = ref heap(new slice<byte>(new byte[] { 1, 2, 3 }), out ж<slice<byte>> Ꮡalias);
        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

        hdr.Value = default;
        hdr.OnHeaderStore();

        Assert.IsTrue(alias == nil, "a nil Data with Len and Cap 0 is Go's nil slice");
    }

    [TestMethod]
    public void AStringHeaderReadsTheStringsWords()
    {
        @string s = "Hello, checkptr!";
        ref @string src = ref heap(s, out ж<@string> Ꮡsrc);

        ж<StringHeader> hdr = Ꮡsrc.Reinterpret<@string, StringHeader>();

        Assert.IsFalse(hdr.Value.Data.IsNull, "a non-empty string's Data is non-nil");
        Assert.AreEqual(16, (int)hdr.Value.Len, "Len is the string's length");
    }

    [TestMethod]
    public void AStringHeaderStoredInGosOrderRebuildsTheString()
    {
        // TestWriteThroughHeader/String, statement for statement.
        @string s = "Hello, checkptr!";
        ref @string sv = ref heap(s, out ж<@string> Ꮡs);
        ref @string alias = ref heap(default(@string), out ж<@string> Ꮡalias);

        ж<StringHeader> hdr = Ꮡalias.Reinterpret<@string, StringHeader>();

        hdr.Value.Data = Ꮡs.Reinterpret<@string, StringHeader>().Value.Data;
        hdr.OnHeaderStore();
        hdr.Value.Len = len(s);
        hdr.OnHeaderStore();

        Assert.IsTrue(alias == s, "alias == s, as Go's TestWriteThroughHeader requires");
    }

    [TestMethod]
    public void AnElementOfAnotherElementTypeIsRefusedByName()
    {
        // L3: Go would reinterpret the int array's memory as bytes; a managed int[] cannot back a []byte.
        slice<int> ints = new slice<int>(new int[] { 1, 2, 3 });
        ref slice<byte> alias = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡalias);
        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(Ꮡ(ints, 0));

        PanicException refused = Assert.ThrowsException<PanicException>(hdr.OnHeaderStore);
        StringAssert.Contains(refused.Message, "slice header not committed");
        StringAssert.Contains(refused.Message, "Int32[]", "the refusal names what the Data names");
        Assert.IsTrue(alias == nil);
    }

    [TestMethod]
    public void AStringHeaderOverAHeapBoxIsRefusedByName()
    {
        ref @string alias = ref heap(default(@string), out ж<@string> Ꮡalias);
        ж<StringHeader> hdr = Ꮡalias.Reinterpret<@string, StringHeader>();

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(new StandardBox<byte>(7));

        PanicException refused = Assert.ThrowsException<PanicException>(hdr.OnHeaderStore);
        StringAssert.Contains(refused.Message, "string header not committed");
        Assert.AreEqual(0, (int)len(alias));
    }

    [TestMethod]
    public void AStringHeaderStoreThatBypassesTheSetterIsRefusedOnTheNextAccess()
    {
        // L1: the store lands on the header's copy and is never committed; the next access says so by name.
        @string s = "Hello";
        ref @string sv = ref heap(s, out ж<@string> Ꮡs);
        ж<StringHeader> hdr = Ꮡs.Reinterpret<@string, StringHeader>();

        hdr.Value.Len = 2;

        PanicException refused = Assert.ThrowsException<PanicException>(() => _ = hdr.Value.Len);
        StringAssert.Contains(refused.Message, "string header written through a reinterpretation");
        Assert.IsTrue(sv == "Hello");
    }

    [TestMethod]
    public void ADataOfAnotherElementTypeIsRefusedByName()
    {
        // L3: a Data naming an int, stored into a []byte's header, is not a slice the box can build.
        ref slice<byte> alias = ref heap(default(slice<byte>), out ж<slice<byte>> Ꮡalias);
        ж<SliceHeader> hdr = Ꮡalias.Reinterpret<slice<byte>, SliceHeader>();
        ж<int> other = new StandardBox<int>(7);

        hdr.Value.Data = @unsafe.Pointer.FromPinnedBox(other);

        PanicException refused = Assert.ThrowsException<PanicException>(hdr.OnHeaderStore);
        StringAssert.Contains(refused.Message, "slice header not committed", "the refusal names the header it could not commit");
        Assert.IsTrue(alias == nil, "the refused store never reached the variable");
        Assert.IsTrue(hdr.Value.Data.IsNull, "and the header reads the variable again");
    }
}
