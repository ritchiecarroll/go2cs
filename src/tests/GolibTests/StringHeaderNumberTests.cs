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
    // refuses (H)'s (Go heap memory, as &b[0] is refused under 3.1). (H)'s EXACT number is refused by the pin registry,
    // and an INTERIOR one (h + 2) by the pin-extent index: the element box's pin covers the whole backing.
    [TestMethod]
    public void W7_TheDoorCrossesLAndRefusesH()
    {
        ж<@string> Ꮡlit = new StaticBox<@string>("w7-door\x00");
        nuint l = DataOf(Ꮡlit);
        GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)l), "f");
        GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)(l + 2)), "f");

        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes("w7-heap")), out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();
        nuint h = (nuint)sh.Value.Data;

        Assert.ThrowsException<PanicException>(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)h), "f"), "(H)'s exact number");
        Assert.ThrowsException<PanicException>(() => GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)(h + 2)), "f"), "(H)'s interior number, h + 2");
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

    // reflect.SliceHeader: Data uintptr; Len, Cap int.
    private struct NumberSliceHeader
    {
        public uintptr Data;
        public nint Len;
        public nint Cap;
    }

    // W9: churn. 100,000 distinct heap strings read through (H) make NO copy and no compare (the (L) table is touched
    // only by package-level reads and their shared backings), and the heap returns near its start after collection; 64
    // distinct package-level literals make exactly 64 copies of exactly their bytes.
    [TestMethod]
    public void W9_HeapChurnMakesNoCopyAndPackageReadsCopyOncePerBacking()
    {
        long copies = StringLiteralCopies.Copies, bytes = StringLiteralCopies.CopiedBytes;
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        long heapBefore = GC.GetTotalMemory(forceFullCollection: true);

        for (int i = 0; i < 100_000; i++)
        {
            ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes($"heap-{i}")), out ж<@string> Ꮡs);
            _ = DataOf(Ꮡs);
        }

        Assert.AreEqual(copies, StringLiteralCopies.Copies, "(H) makes no copy");
        Churn();
        long heapAfter = GC.GetTotalMemory(forceFullCollection: true);
        Assert.IsTrue(heapAfter - heapBefore < 16 << 20, $"the heap returns near its start after the churn: {heapBefore} -> {heapAfter}");

        long expected = 0;

        for (int i = 0; i < 64; i++)
        {
            byte[] literal = System.Text.Encoding.ASCII.GetBytes($"w9-literal-{i}\x00");
            expected += literal.Length;
            _ = DataOf(new StaticBox<@string>(new @string(literal)));
        }

        Assert.AreEqual(copies + 64, StringLiteralCopies.Copies, "one copy per package-level backing");
        Assert.AreEqual(bytes + expected, StringLiteralCopies.CopiedBytes, "each copy is exactly its backing's bytes");
    }

    // W10: the (L) table's first read, raced by 4 threads on each of 2,000 fresh package-level strings. Every thread reads
    // the same number for a string, and every block a losing offer built was freed: the table accepted exactly one copy
    // per string.
    [TestMethod]
    public void W10_RacedFirstReadsAgreeOnOneNumberAndFreeEveryLoser()
    {
        const int strings = 2_000, threads = 4;
        long copies = StringLiteralCopies.Copies;
        var boxes = new ж<@string>[strings];
        nuint[,] got = new nuint[strings, threads];
        int[] arrived = new int[strings];

        for (int i = 0; i < strings; i++)
            boxes[i] = new StaticBox<@string>(new @string(System.Text.Encoding.ASCII.GetBytes($"w10-{i}")));

        var workers = new System.Threading.Thread[threads];

        for (int t = 0; t < threads; t++)
        {
            int me = t;
            workers[t] = new System.Threading.Thread(() =>
            {
                for (int i = 0; i < strings; i++)
                {
                    System.Threading.Interlocked.Increment(ref arrived[i]);
                    System.Threading.SpinWait spin = default;

                    while (System.Threading.Volatile.Read(ref arrived[i]) < threads)
                        spin.SpinOnce(-1);

                    got[i, me] = DataOf(boxes[i]);
                }
            });
            workers[t].Start();
        }

        foreach (System.Threading.Thread worker in workers)
            worker.Join();

        for (int i = 0; i < strings; i++)
        {
            for (int t = 1; t < threads; t++)
                Assert.AreEqual(got[i, 0], got[i, t], $"string {i}: two threads read different numbers");

            Assert.AreEqual($"w10-{i}", Bytes(got[i, 0], $"w10-{i}".Length));
        }

        Assert.AreEqual(copies + strings, StringLiteralCopies.Copies, "exactly one accepted copy per string; every loser was freed, not kept");
    }

    // W13's precursor and W15 (QW-8, P3's item 1): the pointer FromHeaderData answers CARRIES the element box, so a
    // pointee box derived from it alone, the LAST link, keeps the bytes across forced compacting collections with
    // finalizers, with the string, the header and the pointer all dropped. W13 proper (below) is the same chain through
    // the view door the converter emits for (*RawMem)(p)[:n:n].
    [TestMethod]
    public void W13a_AStringHeadersCarriedPointerKeepsTheBytesThroughTheLastLink()
    {
        ж<byte> last = LastLinkOfString();
        Churn();

        Assert.AreEqual((byte)'c', last.Value, "the bytes the header named are still read through the last link");
    }

    [TestMethod]
    public void W15_ASliceHeadersCarriedPointerKeepsTheBytesThroughTheLastLink()
    {
        ж<byte> last = LastLinkOfSlice();
        Churn();

        Assert.AreEqual((byte)'s', last.Value, "the slice's bytes are still read through the last link");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ж<byte> LastLinkOfString()
    {
        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes("carried")), out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();
        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(sh);

        Assert.IsNotNull(p.RetainedSource, "a header box's Data pointer carries its element box");
        return @unsafe.Pointer.To<byte>(p);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ж<byte> LastLinkOfSlice()
    {
        ref slice<byte> b = ref heap(new slice<byte>(System.Text.Encoding.ASCII.GetBytes("slice")), out ж<slice<byte>> Ꮡb);
        ж<NumberSliceHeader> h = Ꮡb.Reinterpret<slice<byte>, NumberSliceHeader>();
        Assert.AreEqual(5, (int)h.Value.Len);
        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(h);

        Assert.IsNotNull(p.RetainedSource, "a slice header box's Data pointer carries its element box");
        return @unsafe.Pointer.To<byte>(p);
    }

    // W13 (R-b), on the view door: Xgethostname's chain as the converter emits it, `copy(dst,
    // (*RawMem)(unsafe.Pointer(sh.Data))[:n:n])` = copy(dst, View<byte>(FromHeaderData(sh), 0, n, n, N)). Only the LAST
    // link, the slice, is held across forced compacting collections with finalizers; the string, the header and the
    // pointer are dropped. The carried pointer names the element box itself, so the door takes V1: a managed window on
    // the string's own backing, with no raw span to go stale. After the copy, once the chain is collected, the element
    // box's pin has left the pin-extent index.
    [TestMethod]
    public void W13_TheHostnameChainThroughTheViewDoorReadsTheBytesHoldingOnlyTheSlice()
    {
        (slice<byte> view, nuint h, nint n) = HostnameView("w13-hostname");
        Churn();

        Assert.IsFalse(view.IsNativeBacked, "V1: a managed window on the string's backing, no raw span");

        slice<byte> name = new(n);
        Assert.AreEqual(n, copy(name, view));
        Assert.AreEqual("w13-hostname", System.Text.Encoding.ASCII.GetString(name.ToSpan()));

        Churn();
        Assert.AreEqual(0, PinExtents.EntriesOver(h, h + (nuint)n), "after the copy and a collection, the element box's pin has left the index");
    }

    // W13b: what the carry buys at the view door. W13 does not need it: while the header box lives, the bare number
    // resolves through the pin registry to the same element box, and the door takes V1 either way. Here only the
    // POINTER outlives the chain (the string and the header are dropped, then compacting collections with finalizers),
    // and the view is cut afterwards: the carried element box is all that keeps the number resolvable, so the door still
    // takes V1 over the string's own bytes. A bare number would resolve to nothing and be cut as a raw native window.
    [TestMethod]
    public void W13b_TheCarriedPointerAloneStillCutsAManagedWindowAfterTheHeaderIsGone()
    {
        @unsafe.Pointer p = CarriedPointerOnly("w13b-carried", out nint n);
        Churn();

        slice<byte> view = @unsafe.Pointer.View<byte>(p, 0, n, n, 1L << 20);

        Assert.IsFalse(view.IsNativeBacked, "V1: the carried element box keeps the number resolvable, so no raw span is cut");

        slice<byte> name = new(n);
        Assert.AreEqual(n, copy(name, view));
        Assert.AreEqual("w13b-carried", System.Text.Encoding.ASCII.GetString(name.ToSpan()));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static @unsafe.Pointer CarriedPointerOnly(string text, out nint n)
    {
        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes(text)), out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();
        n = sh.Value.Len;

        return @unsafe.Pointer.FromHeaderData(sh);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (slice<byte> view, nuint h, nint n) HostnameView(string text)
    {
        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes(text)), out ж<@string> Ꮡs);
        ж<NumberStringHeader> sh = Ꮡs.Reinterpret<@string, NumberStringHeader>();
        nint n = sh.Value.Len;
        nuint h = (nuint)sh.Value.Data;

        // The one live entry is the ELEMENT BOX's pin-only hold, taken when (H) minted h at the Data read above and kept
        // by the header box; FromHeaderData below carries that same box. The slice the view door returns pins nothing.
        Assert.AreEqual(1, PinExtents.EntriesOver(h, h + (nuint)n), "while the chain lives, the element box's pin is one index entry over the backing");

        slice<byte> view = @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromHeaderData(sh), 0, n, n, 1L << 20);

        return (view, h, n);
    }

    // W16 (P3's item 2): a header built field by field is a PLAIN struct box; FromHeaderData answers exactly today's
    // number pointer for it: the same number, carrying nothing.
    [TestMethod]
    public void W16_APlainStructHeaderKeepsTodaysNumberPointer()
    {
        ref NumberStringHeader plain = ref heap(new NumberStringHeader { Data = (uintptr)(nuint)0x1234, Len = 1 }, out ж<NumberStringHeader> Ꮡplain);
        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(Ꮡplain);
        @unsafe.Pointer today = (uintptr)(nuint)0x1234;

        Assert.AreEqual((nuint)(uintptr)today, (nuint)(uintptr)p, "the same number as today's (@unsafe.Pointer)p.Data");
        Assert.IsNull(p.RetainedSource, "a plain struct's Data carries nothing, as today");

        ref NumberStringHeader zero = ref heap(default(NumberStringHeader), out ж<NumberStringHeader> Ꮡzero);
        Assert.IsTrue(@unsafe.Pointer.FromHeaderData(Ꮡzero).IsNull, "a zero Data is the nil pointer, as today");
    }

    // ---- P3's code read of fe14d5faf8: arms a (store orders), fix 2 and nit 5. Read red at fe14d5faf8 first. ----

    private static ж<NumberStringHeader> HeapHeader(string text, out ж<@string> source)
    {
        ref @string s = ref heap(new @string(System.Text.Encoding.ASCII.GetBytes(text)), out source);
        return source.Reinterpret<@string, NumberStringHeader>();
    }

    // A read through unsafe.Pointer(h.Data) after compacting collections with finalizers: the bytes the header names now.
    private static string ReadThroughData<THeader>(ж<THeader> header, int length)
    {
        Churn();
        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(header);
        string read = Bytes((nuint)(uintptr)p, length);
        GC.KeepAlive(p);
        return read;
    }

    // a1: Data THEN Len, the re-slice idiom `h.Data += 2; h.Len -= 2`: the Data store alone names a window past the
    // backing (held), the Len store commits the string over the SAME backing from byte 2.
    [TestMethod]
    public void A1_DataThenLenReSlicesOverTheSameBacking()
    {
        ж<NumberStringHeader> h = HeapHeader("abcdef", out ж<@string> Ꮡs);
        nuint d = (nuint)h.Value.Data;

        h.Value.Data = (uintptr)(d + 2);
        h.OnHeaderStore();
        h.Value.Len -= 2;
        h.OnHeaderStore();

        Assert.IsTrue(Ꮡs.Value == "cdef", $"the re-slice is the backing's bytes from 2, got {Ꮡs.Value}");
        Assert.AreEqual("cdef", ReadThroughData(h, 4), "unsafe.Pointer(h.Data) reads the window after a collection");
    }

    // ---- DETERMINISM (COORD's ask on A2 and Fix2): a held backing must not move, and the run must be able to show a move.

    // A young (gen0), unpinned backing behind a filler that dies first, so a compacting collection has room to slide it.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static byte[] YoungBacking(string text)
    {
        byte[] filler = new byte[16 * 1024];
        filler[0] = 1;
        return System.Text.Encoding.ASCII.GetBytes(text);
    }

    // The array's data address now, read under a `fixed` that ends at once.
    private static nuint AddressOf(byte[] array)
    {
        fixed (byte* data = array)
            return (nuint)data;
    }

    // Collect, run finalizers (a dead element box's pin is released by its finalizer), then compact, three times.
    private static void CompactingChurn()
    {
        for (int round = 0; round < 3; round++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            for (int i = 0; i < 4000; i++)
                _ = new byte[64];

            GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    private static ж<NumberStringHeader> HeaderOver(byte[] bytes, out ж<@string> source)
    {
        ref @string s = ref heap(new @string(bytes), out source);
        return source.Reinterpret<@string, NumberStringHeader>();
    }

    // The arm's verdict on the move: Inconclusive BY NAME when the unpinned twin did not move (this run could not have
    // shown one); otherwise the held backing must not have moved.
    private static void AssertHeldWhileTwinMoved(byte[] held, nuint heldBefore, byte[] twin, nuint twinBefore)
    {
        if (AddressOf(twin) == twinBefore)
            Assert.Inconclusive("the unpinned twin did not move in this run, so the arm could not have shown a move");

        Assert.AreEqual(heldBefore, AddressOf(held), "the backing under the header's word MOVED: nothing held it still");
    }

    // a2: Len THEN Data, deterministic. The backing under the stored word is held still by the header (fix 1 keeps the
    // minted box), and the bytes read through unsafe.Pointer(h.Data) are the window's, while an unpinned twin moved.
    [TestMethod]
    public void A2_LenThenDataReSlicesOverTheSameBacking()
    {
        byte[] bytes = YoungBacking("abcdef"), twin = YoungBacking("ABCDEF");
        ж<NumberStringHeader> h = HeaderOver(bytes, out ж<@string> Ꮡs);
        nuint d = (nuint)h.Value.Data;

        h.Value.Len = 4;
        h.OnHeaderStore();
        h.Value.Data = (uintptr)(d + 2);
        h.OnHeaderStore();
        Assert.IsTrue(Ꮡs.Value == "cdef", $"got {Ꮡs.Value}");

        nuint before = AddressOf(bytes), twinBefore = AddressOf(twin);
        CompactingChurn();
        AssertHeldWhileTwinMoved(bytes, before, twin, twinBefore);

        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(h);
        Assert.AreEqual("cdef", Bytes((nuint)(uintptr)p, 4));
        GC.KeepAlive(p);
        GC.KeepAlive(twin);
    }

    // a2's earlier, non-deterministic form, kept as a plain control.
    [TestMethod]
    public void A2b_LenThenDataControl()
    {
        ж<NumberStringHeader> h = HeapHeader("abcdef", out ж<@string> Ꮡs);
        nuint d = (nuint)h.Value.Data;

        h.Value.Len = 4;
        h.OnHeaderStore();
        Assert.IsTrue(Ꮡs.Value == "abcd");

        h.Value.Data = (uintptr)(d + 2);
        h.OnHeaderStore();

        Assert.IsTrue(Ꮡs.Value == "cdef", $"got {Ꮡs.Value}");
        Assert.AreEqual("cdef", ReadThroughData(h, 4));
    }

    // a3: Data alone, with its OWN word: the string is unchanged and still read after a collection.
    [TestMethod]
    public void A3_DataStoredWithItsOwnWordChangesNothing()
    {
        ж<NumberStringHeader> h = HeapHeader("abcdef", out ж<@string> Ꮡs);

        h.Value.Data = h.Value.Data;
        h.OnHeaderStore();

        Assert.IsTrue(Ꮡs.Value == "abcdef");
        Assert.AreEqual("abcdef", ReadThroughData(h, 6));
    }

    // a4: the slice twin of the re-slice, then a WRITE through the rebuilt slice: it lands in the shared backing after a
    // compacting collection, as Go's write through the re-sliced header does.
    [TestMethod]
    public void A4_ASliceReSlicedThroughItsHeaderSharesAndWritesTheBacking()
    {
        byte[] backing = System.Text.Encoding.ASCII.GetBytes("abcdef");
        ref slice<byte> b = ref heap(new slice<byte>(backing), out ж<slice<byte>> Ꮡb);
        ж<NumberSliceHeader> h = Ꮡb.Reinterpret<slice<byte>, NumberSliceHeader>();
        nuint d = (nuint)h.Value.Data;

        h.Value.Data = (uintptr)(d + 2);
        h.OnHeaderStore();
        h.Value.Len = 4;
        h.OnHeaderStore();
        h.Value.Cap = 4;
        h.OnHeaderStore();

        Churn();

        Assert.AreEqual(4, (int)len(Ꮡb.Value));
        Assert.AreEqual((byte)'c', Ꮡb.Value[0], "the rebuilt slice reads the backing's byte 2 after a collection");

        Ꮡb.Value[0] = (byte)'X';
        Assert.AreEqual((byte)'X', backing[2], "a write through the rebuilt slice lands in the shared backing");
    }

    // Fix 2: a word copied from ANOTHER header (`h2.Data = h1.Data`) commits, and the header keeps the box the registry
    // answered with, so the bytes are still read after h1, its string and every other hold are gone.
    [TestMethod]
    public void Fix2_AWordCopiedFromAnotherHeaderIsKeptAlive()
    {
        byte[] bytes = YoungBacking("xyz"), twin = YoungBacking("XYZ");
        ж<NumberStringHeader> h2 = CopiedHeaderOver(bytes, out ж<@string> Ꮡs2);
        Assert.IsTrue(Ꮡs2.Value == "xyz");

        nuint before = AddressOf(bytes), twinBefore = AddressOf(twin);
        CompactingChurn();
        AssertHeldWhileTwinMoved(bytes, before, twin, twinBefore);

        @unsafe.Pointer p = @unsafe.Pointer.FromHeaderData(h2);
        Assert.AreEqual("xyz", Bytes((nuint)(uintptr)p, 3), "the copied word still names the bytes after h1 is gone");
        GC.KeepAlive(p);
        GC.KeepAlive(twin);
    }

    // Fix 2's earlier, non-deterministic form, kept as a plain control.
    [TestMethod]
    public void Fix2b_CopiedWordControl()
    {
        ж<NumberStringHeader> h2 = CopiedHeader(out ж<@string> Ꮡs2);

        Assert.IsTrue(Ꮡs2.Value == "xyz");
        Assert.AreEqual("xyz", ReadThroughData(h2, 3));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ж<NumberStringHeader> CopiedHeaderOver(byte[] bytes, out ж<@string> Ꮡs2)
    {
        ж<NumberStringHeader> h1 = HeaderOver(bytes, out _);
        ref @string s2 = ref heap(default(@string), out Ꮡs2);
        ж<NumberStringHeader> h2 = Ꮡs2.Reinterpret<@string, NumberStringHeader>();

        h2.Value.Data = h1.Value.Data;
        h2.OnHeaderStore();
        h2.Value.Len = 3;
        h2.OnHeaderStore();
        return h2;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ж<NumberStringHeader> CopiedHeader(out ж<@string> Ꮡs2)
    {
        ж<NumberStringHeader> h1 = HeapHeader("xyz", out _);
        ref @string s2 = ref heap(default(@string), out Ꮡs2);
        ж<NumberStringHeader> h2 = Ꮡs2.Reinterpret<@string, NumberStringHeader>();

        h2.Value.Data = h1.Value.Data;
        h2.OnHeaderStore();
        h2.Value.Len = 3;
        h2.OnHeaderStore();
        return h2;
    }

    // P3's glance, item a: a MISALIGNED interior word (`Data += 1` over an []int32 header) lies inside the backing's
    // bytes but on no element, and is no native address either: refused by name, never viewed as native memory.
    [TestMethod]
    public void ItemA_AMisalignedInteriorWordIsRefusedByName()
    {
        ref slice<int> s = ref heap(new slice<int>(new[] { 1, 2, 3, 4 }), out ж<slice<int>> Ꮡs);
        ж<NumberSliceHeader> h = Ꮡs.Reinterpret<slice<int>, NumberSliceHeader>();
        nuint d = (nuint)h.Value.Data;

        h.Value.Data = (uintptr)(d + 1);

        PanicException refused = Assert.ThrowsException<PanicException>(h.OnHeaderStore);
        StringAssert.Contains(refused.Message, "not on an element boundary");
        Assert.AreEqual(4, (int)len(Ꮡs.Value), "the slice keeps its last consistent words");
    }

    // Nit 5: unsafe.Pointer(h.Data) over a NIL header is Go's nil dereference.
    [TestMethod]
    public void Nit5_ANilHeaderPanicsAsGoNilDereference()
    {
        PanicException fromNull = Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.FromHeaderData<NumberStringHeader>(null!));
        StringAssert.Contains(fromNull.Message, "nil pointer dereference");

        PanicException fromNilBox = Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.FromHeaderData(go.ж<NumberStringHeader>.NilBox));
        StringAssert.Contains(fromNilBox.Message, "nil pointer dereference");
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
