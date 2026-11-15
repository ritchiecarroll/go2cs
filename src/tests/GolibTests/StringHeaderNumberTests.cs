using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// reflect.StringHeader's shape: a header whose Data is a NUMBER (uintptr), not an unsafe.Pointer. ccgo, which
// modernc.org/sqlite and modernc.org/libc are generated with, takes the base of a package's C string literals once as
// `__ccgo_ts = (*reflect.StringHeader)(unsafe.Pointer(&__ccgo_ts1)).Data` and reads every literal at `__ccgo_ts + N` for
// the program's life (docs/phase4/DESIGN-sqlite-ccgo-ts.md, section 10 as amended by 10.1-10.3).
//
// (L) a header over PACKAGE-LEVEL storage reads the base of an immortal native copy of the backing. (H) every other
// header reads the number of the backing's element 0 through the element box's pin-only hold, as &b[0] does. QW-9: an
// (H) read whose backing already has an (L) copy takes that copy. Every arm here was red at the base, where the header
// read panicked with the address route's Q44 arm-2a refusal.
[TestClass]
public unsafe class StringHeaderNumberTests
{
    // reflect.StringHeader: Data uintptr; Len int.
    private struct NumberStringHeader
    {
        public uintptr Data;
        public nint Len;
    }

    private static nuint DataOf(ж<@string> source) => (nuint)source.Reinterpret<@string, NumberStringHeader>().Value.Data;

    private static string CString(nuint at)
    {
        int n = 0;

        while (((byte*)at)[n] != 0)
            n++;

        return System.Text.Encoding.ASCII.GetString((byte*)at, n);
    }

    private static string Bytes(nuint at, int length) => System.Text.Encoding.ASCII.GetString((byte*)at, length);

    private static void Churn()
    {
        for (int round = 0; round < 3; round++)
        {
            for (int i = 0; i < 4000; i++)
                _ = new byte[128];

            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    // W1: ccgo's shape. A package-level literal's NUL-terminated strings read at Data + 0, + 6 and + 12.
    [TestMethod]
    public void W1_APackageLevelLiteralsDataReadsItsCStringsAtEveryOffset()
    {
        ж<@string> Ꮡts1 = new StaticBox<@string>("hello\x00world\x00%s=%d\x00");
        nuint ts = DataOf(Ꮡts1);

        Assert.AreEqual("hello", CString(ts + 0));
        Assert.AreEqual("world", CString(ts + 6));
        Assert.AreEqual("%s=%d", CString(ts + 12));
    }

    // W1b: the vfs pair. A second package-level literal has its own copy, its own stable number and its own bytes.
    [TestMethod]
    public void W1b_ASecondPackageLevelLiteralHasItsOwnNumberAndBytes()
    {
        ж<@string> Ꮡccgo = new StaticBox<@string>("w1b-a\x00");
        ж<@string> Ꮡvfs = new StaticBox<@string>("w1b-b\x00");
        nuint a = DataOf(Ꮡccgo), b = DataOf(Ꮡvfs);

        Assert.AreNotEqual(a, b, "two literals, two numbers");
        Assert.AreEqual("w1b-a", CString(a));
        Assert.AreEqual("w1b-b", CString(b));
        Assert.AreEqual(a, DataOf(Ꮡccgo), "a second read of the same variable reads the same number");
    }

    // W1c: the gethostname shape through (H): a header over a LOCAL holding a heap string reads the string's live bytes.
    [TestMethod]
    public void W1c_AHeaderOverAHeapStringReadsItsBytes()
    {
        @string host = new(System.Text.Encoding.ASCII.GetBytes("gethostname"));
        ref @string s = ref heap(host, out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();

        Assert.AreEqual(11, (int)sh.Value.Len);
        Assert.AreEqual("gethostname", Bytes((nuint)sh.Value.Data, 11));
        GC.KeepAlive(sh);
    }

    // W2: stability. A second read, a second package-level variable over the same backing, and a substring.
    [TestMethod]
    public void W2_ThePackageLevelNumberIsStableAcrossReadsVariablesAndSubstrings()
    {
        @string literal = "w2-stable\x00";
        ж<@string> Ꮡfirst = new StaticBox<@string>(literal);
        ж<@string> Ꮡsecond = new StaticBox<@string>(literal);
        ж<@string> Ꮡsub = new StaticBox<@string>(literal[3..]);
        nuint base_ = DataOf(Ꮡfirst);

        Assert.AreEqual(base_, DataOf(Ꮡfirst), "the same number on a second read");
        Assert.AreEqual(base_, DataOf(Ꮡsecond), "a second variable sharing the backing reads the same number");
        Assert.AreEqual(base_ + 3, DataOf(Ꮡsub), "a substring reads base + offset");
        Assert.AreEqual("stable", CString(DataOf(Ꮡsub)));
    }

    // W3: a package-level copy's bytes survive forced compacting collections with finalizers.
    [TestMethod]
    public void W3_APackageLevelCopySurvivesForcedCollections()
    {
        ж<@string> Ꮡlit = new StaticBox<@string>("w3-survives\x00");
        nuint at = DataOf(Ꮡlit);

        Churn();

        Assert.AreEqual("w3-survives", CString(at));
    }

    // W4 (P3's item 5): the ORDER. A local copy read BEFORE the package variable gets (H)'s number, which is the
    // string's own bytes (StringData == Data, as in Go); the package read gets (L)'s; every read after that, of either,
    // gets (L)'s (QW-9). Both numbers read the literal's bytes.
    [TestMethod]
    public void W4_ALocalReadBeforeThePackageReadGetsHAndEveryLaterReadGetsL()
    {
        @string literal = "w4-order\x00";
        ref @string local = ref heap(literal, out ж<@string> Ꮡlocal);
        ж<@string> Ꮡpkg = new StaticBox<@string>(literal);

        nuint before = DataOf(Ꮡlocal);
        nuint stringData = (nuint)(uintptr)@unsafe.StringData(literal);
        Assert.AreEqual(stringData, before, "(H) before any (L) copy: Data is the string's own bytes, as in Go");

        nuint pkg = DataOf(Ꮡpkg);
        Assert.AreNotEqual(before, pkg, "the package read takes (L)'s copy");

        Assert.AreEqual(pkg, DataOf(Ꮡlocal), "a local read after the package read takes (L)'s copy too (QW-9)");
        Assert.AreEqual("w4-order", CString(before));
        Assert.AreEqual("w4-order", CString(pkg));
        GC.KeepAlive(Ꮡlocal);
    }

    // W5 (F4): an empty string reads Data 0, Len 0; under (L) a zero-length window of a non-empty backing reads
    // base + offset.
    [TestMethod]
    public void W5_AnEmptyStringReadsZeroAndAnLEndWindowReadsBasePlusOffset()
    {
        ж<@string> Ꮡempty = new StaticBox<@string>(default(@string));
        Assert.AreEqual((nuint)0, DataOf(Ꮡempty));
        Assert.AreEqual(0, (int)Ꮡempty.Reinterpret<@string, NumberStringHeader>().Value.Len);

        @string literal = "w5-end";
        ж<@string> Ꮡwhole = new StaticBox<@string>(literal);
        ж<@string> Ꮡend = new StaticBox<@string>(literal[6..]);
        Assert.AreEqual(DataOf(Ꮡwhole) + 6, DataOf(Ꮡend), "(L): a zero-length window at the end reads base + offset");
    }

    // W5b (P3's item 3): under (H) a zero-length window at the END of a non-empty backing has no element to box, and
    // reads 0, as an empty string does. Disclosed: Go keeps base + offset.
    [TestMethod]
    public void W5b_AnHEndWindowReadsZero()
    {
        @string full = new(System.Text.Encoding.ASCII.GetBytes("w5b"));
        ref @string end = ref heap(full[3..], out ж<@string> Ꮡend);

        Assert.AreEqual((nuint)0, DataOf(Ꮡend));
    }

    // W7: the cgo door crosses (L)'s number (native memory, as Go's rodata is not a Go pointer to cgoIsGoPointer) and
    // refuses (H)'s (Go heap memory, as &b[0] is refused under 3.1).
    [TestMethod]
    public void W7_TheDoorCrossesLAndRefusesH()
    {
        ж<@string> Ꮡlit = new StaticBox<@string>("w7-door\x00");
        nuint l = DataOf(Ꮡlit);
        GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)(l + 2)), "f");

        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes("w7-heap")), out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();
        nuint h = (nuint)sh.Value.Data;

        Assert.ThrowsException<PanicException>(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)(h + 2)), "f"));
        GC.KeepAlive(sh);
    }

    // W8 (D5): a header store with a native Data rebuilds the string, as a copy.
    [TestMethod]
    public void W8_AHeaderStoreWithANativeDataRebuildsTheStringAsACopy()
    {
        byte* block = (byte*)NativeMemory.Alloc(3);

        try
        {
            block[0] = (byte)'a';
            block[1] = (byte)'b';
            block[2] = (byte)'c';

            ref @string alias = ref heap(default(@string), out ж<@string> Ꮡalias);
            ж<NumberStringHeader> hdr = Ꮡalias.Reinterpret<@string, NumberStringHeader>();

            hdr.Value.Data = (uintptr)(nuint)block;
            hdr.OnHeaderStore();
            hdr.Value.Len = 3;
            hdr.OnHeaderStore();

            block[0] = (byte)'X';
            Assert.IsTrue(alias == "abc", "the string is a copy of the native bytes at the commit");
        }
        finally
        {
            NativeMemory.Free(block);
        }
    }

    // W12 (P3's F1): a string over a REUSED buffer. Through (H) the header reads the NEW bytes. Behind a package-level
    // variable the second read is refused by name, so the old bytes are never handed out again (R-a).
    [TestMethod]
    public void W12_AReusedBufferReadsNewBytesThroughHAndIsRefusedBehindAPackageVariable()
    {
        byte[] buffer = System.Text.Encoding.ASCII.GetBytes("old!");

        ref @string local = ref heap(@string.AliasOf(new slice<byte>(buffer)), out ж<@string> Ꮡlocal);
        nuint h = DataOf(Ꮡlocal);
        Assert.AreEqual("old!", Bytes(h, 4));

        ж<@string> Ꮡpkg = new StaticBox<@string>(@string.AliasOf(new slice<byte>(buffer)));
        Assert.AreEqual("old!", Bytes(DataOf(Ꮡpkg), 4));

        buffer[0] = (byte)'n';
        buffer[1] = (byte)'e';
        buffer[2] = (byte)'w';

        PanicException refused = Assert.ThrowsException<PanicException>(() => DataOf(Ꮡpkg));
        StringAssert.Contains(refused.Message, "changed after go2cs copied them");
        GC.KeepAlive(Ꮡlocal);
    }

    // W12's (H) half on a buffer no package variable has read: the header reads the live bytes after the write.
    [TestMethod]
    public void W12b_ThroughHAWrittenBufferReadsItsNewBytes()
    {
        byte[] buffer = System.Text.Encoding.ASCII.GetBytes("aaaa");
        ref @string local = ref heap(@string.AliasOf(new slice<byte>(buffer)), out ж<@string> Ꮡlocal);
        ж<NumberStringHeader> sh = Ꮡlocal.Reinterpret<@string, NumberStringHeader>();
        nuint h = (nuint)sh.Value.Data;

        buffer[0] = (byte)'b';

        Assert.AreEqual("baaa", Bytes(h, 4), "(H) names the live bytes");
        GC.KeepAlive(sh);
    }
}
