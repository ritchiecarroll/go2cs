// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using rand = go.math.rand_package;
using os = os_package;
using reflect = reflect_package;
using debug = go.runtime.debug_package;
using sync = sync_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using go.runtime;
using static go.compress.flate_package;

partial class flate_internal_test_package {

internal partial struct deflateTest {
    internal slice<byte> @in;
    internal nint level;
    internal slice<byte> @out;
}

internal partial struct deflateInflateTest {
    internal slice<byte> @in;
}

internal partial struct reverseBitsTest {
    internal uint16 @in;
    internal uint8 bitCount;
    internal uint16 @out;
}

internal static slice<ж<deflateTest>> deflateTests = new ж<deflateTest>[]{
    Ꮡ(new deflateTest(new byte[]{}.slice(), 0, new byte[]{1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), -1, new byte[]{18, 4, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), DefaultCompression, new byte[]{18, 4, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), 4, new byte[]{18, 4, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), 0, new byte[]{0, 1, 0, 254, 255, 17, 1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x12}.slice(), 0, new byte[]{0, 2, 0, 253, 255, 17, 18, 1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11}.slice(), 0,
        new byte[]{0, 8, 0, 247, 255, 17, 17, 17, 17, 17, 17, 17, 17, 1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{}.slice(), 2, new byte[]{1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), 2, new byte[]{18, 4, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x12}.slice(), 2, new byte[]{18, 20, 2, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11}.slice(), 2, new byte[]{18, 132, 2, 64, 0, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{}.slice(), 9, new byte[]{1, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11}.slice(), 9, new byte[]{18, 4, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x12}.slice(), 9, new byte[]{18, 20, 2, 4, 0, 0, 255, 255}.slice())),
    Ꮡ(new deflateTest(new byte[]{0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11}.slice(), 9, new byte[]{18, 132, 2, 64, 0, 0, 0, 255, 255}.slice()))
}.slice();

internal static slice<ж<deflateInflateTest>> deflateInflateTests = new ж<deflateInflateTest>[]{
    Ꮡ(new deflateInflateTest(new byte[]{}.slice())),
    Ꮡ(new deflateInflateTest(new byte[]{0x11}.slice())),
    Ꮡ(new deflateInflateTest(new byte[]{0x11, 0x12}.slice())),
    Ꮡ(new deflateInflateTest(new byte[]{0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11}.slice())),
    Ꮡ(new deflateInflateTest(new byte[]{0x11, 0x10, 0x13, 0x41, 0x21, 0x21, 0x41, 0x13, 0x87, 0x78, 0x13}.slice())),
    Ꮡ(new deflateInflateTest(largeDataChunk()))
}.slice();

internal static slice<ж<reverseBitsTest>> reverseBitsTests = new ж<reverseBitsTest>[]{
    Ꮡ(new reverseBitsTest(1, 1, 1)),
    Ꮡ(new reverseBitsTest(1, 2, 2)),
    Ꮡ(new reverseBitsTest(1, 3, 4)),
    Ꮡ(new reverseBitsTest(1, 4, 8)),
    Ꮡ(new reverseBitsTest(1, 5, 16)),
    Ꮡ(new reverseBitsTest(17, 5, 17)),
    Ꮡ(new reverseBitsTest(257, 9, 257)),
    Ꮡ(new reverseBitsTest(29, 5, 23))
}.slice();

internal static slice<byte> largeDataChunk() {
    var result = new slice<byte>(100000);
    foreach (var (i, _) in result) {
        result[i] = (byte)((nint)(i * i & 0xFF));
    }
    return result;
}

public static void TestBulkHash4(ж<testing.T> Ꮡt) {
    foreach (var (_, x) in deflateTests) {
        var y = x.Value.@out;
        if (len(y) < minMatchLength) {
            continue;
        }
        y = appendꓸꓸꓸ(y, y);
        for (nint j = 4; j < len(y); j++) {
            var yΔ1 = y.slice(0, j);
            var dst = new slice<uint32>(len(yΔ1) - (nint)minMatchLength + 1);
            foreach (var (i, _) in dst) {
                dst[i] = (uint32)(i + 100);
            }
            bulkHash4(yΔ1, dst);
            foreach (var (i, got) in dst) {
                var want = hash4(yΔ1.slice(i));
                if (got != want && got == (uint32)i + 100){
                    Ꮡt.Errorf("Len:%d Index:%d, want 0x%08x but not modified"u8, len(yΔ1), i, want);
                } else 
                if (got != want) {
                    Ꮡt.Errorf("Len:%d Index:%d, got 0x%08x want:0x%08x"u8, len(yΔ1), i, got, want);
                }
            }
        }
    }
}

public static void TestDeflate(ж<testing.T> Ꮡt) {
    foreach (var (_, h) in deflateTests) {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡbuf), (~h).level);
        if (err != default!) {
            Ꮡt.Errorf("NewWriter: %v"u8, err);
            continue;
        }
        w.Write((~h).@in);
        w.Close();
        if (!bytes.Equal(buf.Bytes(), (~h).@out)) {
            Ꮡt.Errorf("Deflate(%d, %x) = \n%#v, want \n%#v"u8, (~h).level, (~h).@in, buf.Bytes(), (~h).@out);
        }
    }
}

public static void TestWriterClose(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var (zw, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(b), 6);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    {
        var (c, errΔ1) = zw.Write(slice<byte>("Test"u8)); if (errΔ1 != default! || c != 4) {
            Ꮡt.Fatalf("Write to not closed writer: %s, %d"u8, errΔ1, c);
        }
    }
    {
        var errΔ2 = zw.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Close: %v"u8, errΔ2);
        }
    }
    nint afterClose = b.Len();
    {
        var (c, errΔ3) = zw.Write(slice<byte>("Test"u8)); if (errΔ3 == default! || c != 0) {
            Ꮡt.Fatalf("Write to closed writer: %v, %d"u8, errΔ3, c);
        }
    }
    {
        var errΔ4 = zw.Flush(); if (errΔ4 == default!) {
            Ꮡt.Fatalf("Flush to closed writer: %s"u8, errΔ4);
        }
    }
    {
        var errΔ5 = zw.Close(); if (errΔ5 != default!) {
            Ꮡt.Fatalf("Close: %v"u8, errΔ5);
        }
    }
    if (afterClose != b.Len()) {
        Ꮡt.Fatalf("Writer wrote data after close. After close: %d. After writes on closed stream: %d"u8, afterClose, b.Len());
    }
}

// A sparseReader returns a stream consisting of 0s followed by 1<<16 1s.
// This tests missing hash references in a very large input.
internal partial struct sparseReader {
    internal int64 l;
    internal int64 cur;
}

internal static (nint n, error err) Read(this ref sparseReader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (r.cur >= r.l) {
        return (0, io.EOF);
    }
    n = len(b);
    var cur = r.cur + (int64)n;
    if (cur > r.l) {
        n -= (nint)(cur - r.l);
        cur = r.l;
    }
    foreach (var (i, _) in b.slice(0, n)) {
        if (r.cur + (int64)i >= r.l - ((int64)1 << (int)(16))){
            b[i] = 1;
        } else {
            b[i] = 0;
        }
    }
    r.cur = cur;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingSparseChunkˢ = (@string)"skipping sparse chunk during short test"u8;

public static void TestVeryLongSparseChunk(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingSparseChunkˢ);
    }
    var (w, err) = NewWriter(io.Discard, 1);
    if (err != default!) {
        Ꮡt.Errorf("NewWriter: %v"u8, err);
        return;
    }
    {
        (_, err) = io.Copy(new flate_test_package.flate_WriterжWriter(w), new flate_internal_test_package.sparseReaderжReader(Ꮡ(new sparseReader(l: 2300000000)))); if (err != default!) {
            Ꮡt.Errorf("Compress failed: %v"u8, err);
            return;
        }
    }
}

internal partial struct syncBuffer {
    internal bytes.Buffer buf;
    internal sync.RWMutex mu;
    internal bool closed;
    internal channel<bool> ready;
}

internal static ж<syncBuffer> newSyncBuffer() {
    return Ꮡ(new syncBuffer(ready: new channel<bool>(1)));
}

internal static (nint n, error err) Read(this ж<syncBuffer> Ꮡb, slice<byte> p) {
    nint n = default!;
    error err = default!;

    ref var b = ref Ꮡb.DerefOrNull();
    while (ᐧ) {
        Ꮡb.of(syncBuffer.Ꮡmu).RLock();
        (n, err) = b.buf.Read(p);
        Ꮡb.of(syncBuffer.Ꮡmu).RUnlock();
        if (n > 0 || b.closed) {
            return (n, err);
        }
        ᐸꟷ(b.ready);
    }
}

internal static void signal(this ref syncBuffer b) {
    var selᴛ1 = b.ready.ᐸꟷ(true, ꓸꓸꓸ);
    switch (trySelect(selᴛ1)) {
    case 0: {
        break;
    }
    default: {
        break;
    }}
}

internal static (nint n, error err) Write(this ref syncBuffer b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = b.buf.Write(p);
    b.signal();
    return (n, err);
}

internal static void WriteMode(this ж<syncBuffer> Ꮡb) {
    Ꮡb.of(syncBuffer.Ꮡmu).Lock();
}

internal static void ReadMode(this ж<syncBuffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.of(syncBuffer.Ꮡmu).Unlock();
    b.signal();
}

internal static error Close(this ref syncBuffer b) {
    b.closed = true;
    b.signal();
    return default!;
}

internal static void testSync(ж<testing.T> Ꮡt, nint level, slice<byte> input, @string name) {
    if (len(input) == 0) {
        return;
    }
    Ꮡt.Logf("--testSync %d, %d, %s"u8, level, len(input), name);
    var buf = newSyncBuffer();
    var buf1 = @new<bytes.Buffer>();
    buf.WriteMode();
    var (w, err) = NewWriter(io.MultiWriter(new flate_internal_test_package.syncBufferжWriter(buf), new flate_test_package.bytes_BufferжWriter(buf1)), level);
    if (err != default!) {
        Ꮡt.Errorf("NewWriter: %v"u8, err);
        return;
    }
    var r = NewReader(new flate_internal_test_package.syncBufferжReader(buf));
    // Write half the input and read back.
    for (nint i = 0; i < 2; i++) {
        nint lo = default!;
        nint hi = default!;
        if (i == 0){
            (lo, hi) = (0, (len(input) + 1) / 2);
        } else {
            (lo, hi) = ((len(input) + 1) / 2, len(input));
        }
        Ꮡt.Logf("#%d: write %d-%d"u8, i, lo, hi);
        {
            var (_, errΔ1) = w.Write(input.slice(lo, hi)); if (errΔ1 != default!) {
                Ꮡt.Errorf("testSync: write: %v"u8, errΔ1);
                return;
            }
        }
        if (i == 0){
            {
                var errΔ2 = w.Flush(); if (errΔ2 != default!) {
                    Ꮡt.Errorf("testSync: flush: %v"u8, errΔ2);
                    return;
                }
            }
        } else {
            {
                var errΔ3 = w.Close(); if (errΔ3 != default!) {
                    Ꮡt.Errorf("testSync: close: %v"u8, errΔ3);
                }
            }
        }
        buf.ReadMode();
        var outΔ1 = new slice<byte>(hi - lo + 1);
        var (m, errΔ4) = io.ReadAtLeast(r, outΔ1, hi - lo);
        Ꮡt.Logf("#%d: read %d"u8, i, m);
        if (m != hi - lo || errΔ4 != default!) {
            Ꮡt.Errorf("testSync/%d (%d, %d, %s): read %d: %d, %v (%d left)"u8, i, level, len(input), name, hi - lo, m, errΔ4, buf.of(syncBuffer.Ꮡbuf).Len());
            return;
        }
        if (!bytes.Equal(input.slice(lo, hi), outΔ1.slice(0, hi - lo))) {
            Ꮡt.Errorf("testSync/%d: read wrong bytes: %x vs %x"u8, i, input.slice(lo, hi), outΔ1.slice(0, hi - lo));
            return;
        }
        // This test originally checked that after reading
        // the first half of the input, there was nothing left
        // in the read buffer (buf.buf.Len() != 0) but that is
        // not necessarily the case: the write Flush may emit
        // some extra framing bits that are not necessary
        // to process to obtain the first half of the uncompressed
        // data. The test ran correctly most of the time, because
        // the background goroutine had usually read even
        // those extra bits by now, but it's not a useful thing to
        // check.
        buf.WriteMode();
    }
    buf.ReadMode();
    var @out = new slice<byte>(10);
    {
        var (n, errΔ5) = r.Read(@out); if (n > 0 || !AreEqual(errΔ5, io.EOF)) {
            Ꮡt.Errorf("testSync (%d, %d, %s): final Read: %d, %v (hex: %x)"u8, level, len(input), name, n, errΔ5, @out.slice(0, n));
        }
    }
    if (buf.of(syncBuffer.Ꮡbuf).Len() != 0) {
        Ꮡt.Errorf("testSync (%d, %d, %s): extra data at end"u8, level, len(input), name);
    }
    r.Close();
    // stream should work for ordinary reader too
    r = NewReader(new flate_test_package.bytes_BufferжReader(buf1));
    (@out, err) = io.ReadAll(r);
    if (err != default!) {
        Ꮡt.Errorf("testSync: read: %s"u8, err);
        return;
    }
    r.Close();
    if (!bytes.Equal(input, @out)) {
        Ꮡt.Errorf("testSync: decompress(compress(data)) != data: level=%d input=%s"u8, level, name);
    }
}

internal static void testToFromWithLevelAndLimit(ж<testing.T> Ꮡt, nint level, slice<byte> input, @string name, nint limit) {
    ref var buffer = ref heap(new bytes.Buffer(), out var Ꮡbuffer);
    var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡbuffer), level);
    if (err != default!) {
        Ꮡt.Errorf("NewWriter: %v"u8, err);
        return;
    }
    w.Write(input);
    w.Close();
    if (limit > 0 && buffer.Len() > limit) {
        Ꮡt.Errorf("level: %d, len(compress(data)) = %d > limit = %d"u8, level, buffer.Len(), limit);
        return;
    }
    if (limit > 0) {
        Ꮡt.Logf("level: %d, size:%.2f%%, %d b\n"u8, level, (float64)(buffer.Len() * 100) / (float64)limit, buffer.Len());
    }
    var r = NewReader(new flate_test_package.bytes_BufferжReader(Ꮡbuffer));
    (var @out, err) = io.ReadAll(r);
    if (err != default!) {
        Ꮡt.Errorf("read: %s"u8, err);
        return;
    }
    r.Close();
    if (!bytes.Equal(input, @out)) {
        Ꮡt.Errorf("decompress(compress(data)) != data: level=%d input=%s"u8, level, name);
        return;
    }
    testSync(Ꮡt, level, input, name);
}

internal static void testToFromWithLimit(ж<testing.T> Ꮡt, slice<byte> input, @string name, /*[11]*/ array<nint> limit) {
    limit = limit.Clone();

    for (nint i = 0; i < 10; i++) {
        testToFromWithLevelAndLimit(Ꮡt, i, input, name, limit[i]);
    }
    // Test HuffmanCompression
    testToFromWithLevelAndLimit(Ꮡt, -2, input, name, limit[10]);
}

public static void TestDeflateInflate(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (i, h) in deflateInflateTests) {
        if (testing.Short() && len((~h).@in) > 10000) {
            continue;
        }
        testToFromWithLimit(Ꮡt, (~h).@in, fmt.Sprintf("#%d"u8, i), new nint[]{}.array(11));
    }
}

public static void TestReverseBits(ж<testing.T> Ꮡt) {
    foreach (var (_, h) in reverseBitsTests) {
        {
            var v = reverseBits((~h).@in, (~h).bitCount); if (v != (~h).@out) {
                Ꮡt.Errorf("reverseBits(%v,%v) = %v, want %v"u8,
                    (~h).@in, (~h).bitCount, v, (~h).@out);
            }
        }
    }
}

internal partial struct deflateInflateStringTest {
    internal @string filename;
    internal @string label;
    internal array<nint> limit = new(11);
}

internal static slice<deflateInflateStringTest> deflateInflateStringTests = new deflateInflateStringTest[]{
    new(
        "../testdata/e.txt"u8,
        "2.718281828..."u8,
        new nint[]{100018, 50650, 50960, 51150, 50930, 50790, 50790, 50790, 50790, 50790, 43683}.array()
    ),
    new(
        "../../testdata/Isaac.Newton-Opticks.txt"u8,
        "Isaac.Newton-Opticks"u8,
        new nint[]{567248, 218338, 198211, 193152, 181100, 175427, 175427, 173597, 173422, 173422, 325240}.array()
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

public static void TestDeflateInflateString(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    foreach (var (_, vᴛ1) in deflateInflateStringTests) {
        var test = vᴛ1.ΔClone();

        var (gold, err) = os.ReadFile(test.filename);
        if (err != default!) {
            Ꮡt.Error(err);
        }
        testToFromWithLimit(Ꮡt, gold, test.label, test.limit);
        if (testing.Short()) {
            break;
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dictᶜ = "hello world"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ = "hello again world"u8;

public static void TestReaderDict(ж<testing.T> Ꮡt) {
    @string dict = dictᶜ;
    @string text = textᶜ;
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡb), 5);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    w.Write(slice<byte>(dict));
    w.Flush();
    b.Reset();
    w.Write(slice<byte>(text));
    w.Close();
    var r = NewReaderDict(new flate_test_package.bytes_BufferжReader(Ꮡb), slice<byte>(dict));
    (var data, err) = io.ReadAll(r);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (((sstring)data) != "hello again world"u8) {
        Ꮡt.Fatalf("read returned %q want %q"u8, ((@string)data), text);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dictᶜ1 = "hello world"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ1 = "hello again world"u8;

public static void TestWriterDict(ж<testing.T> Ꮡt) {
    @string dict = dictᶜ1;
    @string text = textᶜ1;
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡb), 5);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    w.Write(slice<byte>(dict));
    w.Flush();
    b.Reset();
    w.Write(slice<byte>(text));
    w.Close();
    ref var b1 = ref heap(new bytes.Buffer(), out var Ꮡb1);
    (w, _) = NewWriterDict(new flate_test_package.bytes_BufferжWriter(Ꮡb1), 5, slice<byte>(dict));
    w.Write(slice<byte>(text));
    w.Close();
    if (!bytes.Equal(b1.Bytes(), b.Bytes())) {
        Ꮡt.Fatalf("writer wrote %q want %q"u8, b1.Bytes(), b.Bytes());
    }
}

// See https://golang.org/issue/2508
public static void TestRegression2508(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Logf("test disabled with -short"u8);
        return;
    }
    var (w, err) = NewWriter(io.Discard, 1);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    var buf = new slice<byte>(1024);
    for (nint i = 0; i < 131072; i++) {
        {
            var (_, errΔ1) = w.Write(buf); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writer failed: %v"u8, errΔ1);
            }
        }
    }
    w.Close();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dictˢ = "dict"u8;

public static void TestWriterReset(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    for (nint level = 0; level <= 9; level++) {
        if (testing.Short() && level > 1) {
            break;
        }
        var (w, err) = NewWriter(io.Discard, level);
        if (err != default!) {
            Ꮡt.Fatalf("NewWriter: %v"u8, err);
        }
        var buf = slice<byte>("hello world"u8);
        nint n = 1024;
        if (testing.Short()) {
            n = 10;
        }
        for (nint i = 0; i < n; i++) {
            w.Write(buf);
        }
        w.Reset(io.Discard);
        (var wref, err) = NewWriter(io.Discard, level);
        if (err != default!) {
            Ꮡt.Fatalf("NewWriter: %v"u8, err);
        }
        // DeepEqual doesn't compare functions.
        (w.Value.d.fill, wref.Value.d.fill) = (default!, default!);
        (w.Value.d.step, wref.Value.d.step) = (default!, default!);
        (w.Value.d.bulkHasher, wref.Value.d.bulkHasher) = (default!, default!);
        (w.Value.d.bestSpeed, wref.Value.d.bestSpeed) = (default!, default!);
        // hashMatch is always overwritten when used.
        copy((~w).d.hashMatch[..], (~wref).d.hashMatch[..]);
        if (len((~w).d.tokens) != 0) {
            Ꮡt.Errorf("level %d Writer not reset after Reset. %d tokens were present"u8, level, len((~w).d.tokens));
        }
        // As long as the length is 0, we don't care about the content.
        w.Value.d.tokens = wref.Value.d.tokens;
        // We don't care if there are values in the window, as long as it is at d.index is 0
        w.Value.d.window = wref.Value.d.window;
        if (!reflect.DeepEqual(w.OrTypedNil(), wref.OrTypedNil())) {
            Ꮡt.Errorf("level %d Writer not reset after Reset"u8, level);
        }
    }
    var levels = new nint[]{0, 1, 2, 5, 9}.slice();
    foreach (var (_, level) in levels) {
        Ꮡt.Run(fmt.Sprint(level), (ж<testing.T> tΔ1) => {
            testResetOutput(tΔ1, level, default!);
        });
    }
    var levelsʗ1 = levels;
    Ꮡt.Run(dictˢ, (ж<testing.T> tΔ2) => {
        foreach (var (_, level) in levelsʗ1) {
            tΔ2.Run(fmt.Sprint(level), (ж<testing.T> tΔ3) => {
                testResetOutput(tΔ3, level, default!);
            });
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stoppingˢ = (@string)"Stopping"u8;

internal static void testResetOutput(ж<testing.T> Ꮡt, nint level, slice<byte> dict) {
    void writeData(ж<global::go.compress.flate_package.Writer> wΔ1) {
        var msg = slice<byte>("now is the time for all good gophers"u8);
        wΔ1.Write(msg);
        wΔ1.Flush();
        var hello = slice<byte>("hello world"u8);
        for (nint i = 0; i < 1024; i++) {
            wΔ1.Write(hello);
        }
        var fill = bytes.Repeat(slice<byte>("x"u8), 65000);
        wΔ1.Write(fill);
    }
    var buf = @new<bytes.Buffer>();
    ж<global::go.compress.flate_package.Writer> w = default!;
    error err = default!;
    if (dict == default!){
        (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(buf), level);
    } else {
        (w, err) = NewWriterDict(new flate_test_package.bytes_BufferжWriter(buf), level, dict);
    }
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    writeData(w);
    w.Close();
    var out1 = buf.Bytes();
    var buf2 = @new<bytes.Buffer>();
    w.Reset(new flate_test_package.bytes_BufferжWriter(buf2));
    writeData(w);
    w.Close();
    var out2 = buf2.Bytes();
    if (len(out1) != len(out2)) {
        Ꮡt.Errorf("got %d, expected %d bytes"u8, len(out2), len(out1));
        return;
    }
    if (!bytes.Equal(out1, out2)) {
        nint mm = 0;
        foreach (var (i, b) in out1.slice(0, len(out2))) {
            if (b != out2[i]) {
                Ꮡt.Errorf("mismatch index %d: %#02x, expected %#02x"u8, i, out2[i], b);
            }
            mm++;
            if (mm == 10) {
                Ꮡt.Fatal(stoppingˢ);
            }
        }
    }
    Ꮡt.Logf("got %d bytes"u8, len(out1));
}

// TestBestSpeed tests that round-tripping through deflate and then inflate
// recovers the original input. The Write sizes are near the thresholds in the
// compressor.encSpeed method (0, 16, 128), as well as near maxStoreBlockSize
// (65535).
public static void TestBestSpeed(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    var abc = new slice<byte>(128);
    foreach (var (i, _) in abc) {
        abc[i] = (byte)i;
    }
    var abcabc = bytes.Repeat(abc, 131072 / len(abc));
    slice<byte> want = default!;
    var testCases = new slice<nint>[]{
        new nint[]{65536, 0}.slice(),
        new nint[]{65536, 1}.slice(),
        new nint[]{65536, 1, 256}.slice(),
        new nint[]{65536, 1, 65536}.slice(),
        new nint[]{65536, 14}.slice(),
        new nint[]{65536, 15}.slice(),
        new nint[]{65536, 16}.slice(),
        new nint[]{65536, 16, 256}.slice(),
        new nint[]{65536, 16, 65536}.slice(),
        new nint[]{65536, 127}.slice(),
        new nint[]{65536, 128}.slice(),
        new nint[]{65536, 128, 256}.slice(),
        new nint[]{65536, 128, 65536}.slice(),
        new nint[]{65536, 129}.slice(),
        new nint[]{65536, 65536, 256}.slice(),
        new nint[]{65536, 65536, 65536}.slice()
    }.slice();
    foreach (var (i, tc) in testCases) {
        if (i >= 3 && testing.Short()) {
            break;
        }
        foreach (var (_, firstN) in new nint[]{1, 65534, 65535, 65536, 65537, 131072}.slice()) {
            tc[0] = firstN;
outer:
            foreach (var (_, flush) in new bool[]{false, true}.slice()) {
                var buf = @new<bytes.Buffer>();
                want = want[..0];
                var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(buf), BestSpeed);
                if (err != default!) {
                    Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: NewWriter: %v"u8, i, firstN, flush, err);
                    continue;
                }
                foreach (var (_, n) in tc) {
                    want = appendꓸꓸꓸ(want, abcabc.slice(0, n));
                    {
                        var (_, errΔ1) = w.Write(abcabc.slice(0, n)); if (errΔ1 != default!) {
                            Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: Write: %v"u8, i, firstN, flush, errΔ1);
                            goto continue_outer;
                        }
                    }
                    if (!flush) {
                        continue;
                    }
                    {
                        var errΔ2 = w.Flush(); if (errΔ2 != default!) {
                            Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: Flush: %v"u8, i, firstN, flush, errΔ2);
                            goto continue_outer;
                        }
                    }
                }
                {
                    var errΔ3 = w.Close(); if (errΔ3 != default!) {
                        Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: Close: %v"u8, i, firstN, flush, errΔ3);
                        continue;
                    }
                }
                var r = NewReader(new flate_test_package.bytes_BufferжReader(buf));
                (var got, err) = io.ReadAll(r);
                if (err != default!) {
                    Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: ReadAll: %v"u8, i, firstN, flush, err);
                    continue;
                }
                r.Close();
                if (!bytes.Equal(got, want)) {
                    Ꮡt.Errorf("i=%d, firstN=%d, flush=%t: corruption during deflate-then-inflate"u8, i, firstN, flush);
                    continue;
                }
continue_outer:;
            }
break_outer:;
        }
    }
}

internal static error errIO = errors.New("IO error"u8);

// failWriter fails with errIO exactly at the nth call to Write.
internal partial struct failWriter {
    internal nint n;
}

internal static (nint, error) Write(this ref failWriter w, slice<byte> b) {
    w.n--;
    if (w.n == -1) {
        return (0, errIO);
    }
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataIsaacNewtonˢ = "../../testdata/Isaac.Newton-Opticks.txt"u8;

public static void TestWriterPersistentWriteError(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (d, err) = os.ReadFile(testdataIsaacNewtonˢ);
    if (err != default!) {
        Ꮡt.Fatalf("ReadFile: %v"u8, err);
    }
    d = d[..10000]; // Keep this test short
    (var zw, err) = NewWriter(default!, DefaultCompression);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    // Sweep over the threshold at which an error is returned.
    // The variable i makes it such that the ith call to failWriter.Write will
    // return errIO. Since failWriter errors are not persistent, we must ensure
    // that flate.Writer errors are persistent.
    for (nint iᴛ1 = 0; iᴛ1 < 1000; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var fw = Ꮡ(new failWriter(i));
        zw.Reset(new flate_internal_test_package.failWriterжWriter(fw));
        var (_, werr) = zw.Write(d);
        var cerr = zw.Close();
        var ferr = zw.Flush();
        if (!AreEqual(werr, errIO) && werr != default!) {
            Ꮡt.Errorf("test %d, mismatching Write error: got %v, want %v"u8, i, werr, errIO);
        }
        if (!AreEqual(cerr, errIO) && (~fw).n < 0) {
            Ꮡt.Errorf("test %d, mismatching Close error: got %v, want %v"u8, i, cerr, errIO);
        }
        if (!AreEqual(ferr, errIO) && (~fw).n < 0) {
            Ꮡt.Errorf("test %d, mismatching Flush error: got %v, want %v"u8, i, ferr, errIO);
        }
        if ((~fw).n >= 0) {
            // At this point, the failure threshold was sufficiently high enough
            // that we wrote the whole stream without any errors.
            return;
        }
        iᴛ1 = i;
    }
}

public static void TestWriterPersistentFlushError(ж<testing.T> Ꮡt) {
    var (zw, err) = NewWriter(new flate_internal_test_package.failWriterжWriter(Ꮡ(new failWriter(0))), DefaultCompression);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    var flushErr = zw.Flush();
    var closeErr = zw.Close();
    var (_, writeErr) = zw.Write(slice<byte>("Test"u8));
    checkErrors(new error[]{closeErr, flushErr, writeErr}.slice(), errIO, Ꮡt);
}

public static void TestWriterPersistentCloseError(ж<testing.T> Ꮡt) {
    // If underlying writer return error on closing stream we should persistent this error across all writer calls.
    var (zw, err) = NewWriter(new flate_internal_test_package.failWriterжWriter(Ꮡ(new failWriter(0))), DefaultCompression);
    if (err != default!) {
        Ꮡt.Fatalf("NewWriter: %v"u8, err);
    }
    var closeErr = zw.Close();
    var flushErr = zw.Flush();
    var (_, writeErr) = zw.Write(slice<byte>("Test"u8));
    checkErrors(new error[]{closeErr, flushErr, writeErr}.slice(), errIO, Ꮡt);
    // After closing writer we should persistent "write after close" error across Flush and Write calls, but return nil
    // on next Close calls.
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    zw.Reset(new flate_test_package.bytes_BufferжWriter(Ꮡb));
    err = zw.Close();
    if (err != default!) {
        Ꮡt.Fatalf("First call to close returned error: %s"u8, err);
    }
    err = zw.Close();
    if (err != default!) {
        Ꮡt.Fatalf("Second call to close returned error: %s"u8, err);
    }
    flushErr = zw.Flush();
    (_, writeErr) = zw.Write(slice<byte>("Test"u8));
    checkErrors(new error[]{flushErr, writeErr}.slice(), errWriterClosed, Ꮡt);
}

internal static void checkErrors(slice<error> got, error want, ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Helper();
    foreach (var (_, err) in got) {
        if (!AreEqual(err, want)) {
            Ꮡt.Errorf("Error doesn't match\nWant: %s\nGot: %s"u8, want, got);
        }
    }
}

internal partial struct TestBestSpeedMatch_cases /*dyn*/ {
    internal slice<byte> previous, current;
    internal int32 t, s, want;
}

public static void TestBestSpeedMatch(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    var cases = new TestBestSpeedMatch_cases[]{new(
        previous: new byte[]{0, 0, 0, 1, 2}.slice(),
        current: new byte[]{3, 4, 5, 0, 1, 2, 3, 4, 5}.slice(),
        t: -3,
        s: 3,
        want: 6
    ), new(
        previous: new byte[]{0, 0, 0, 1, 2}.slice(),
        current: new byte[]{2, 4, 5, 0, 1, 2, 3, 4, 5}.slice(),
        t: -3,
        s: 3,
        want: 3
    ), new(
        previous: new byte[]{0, 0, 0, 1, 1}.slice(),
        current: new byte[]{3, 4, 5, 0, 1, 2, 3, 4, 5}.slice(),
        t: -3,
        s: 3,
        want: 2
    ), new(
        previous: new byte[]{0, 0, 0, 1, 2}.slice(),
        current: new byte[]{2, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: -1,
        s: 0,
        want: 4
    ), new(
        previous: new byte[]{0, 0, 0, 1, 2, 3, 4, 5, 2, 2}.slice(),
        current: new byte[]{2, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: -7,
        s: 4,
        want: 5
    ), new(
        previous: new byte[]{9, 9, 9, 9, 9}.slice(),
        current: new byte[]{2, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: -1,
        s: 0,
        want: 0
    ), new(
        previous: new byte[]{9, 9, 9, 9, 9}.slice(),
        current: new byte[]{9, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: 0,
        s: 1,
        want: 0
    ), new(
        previous: new byte[]{}.slice(),
        current: new byte[]{9, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: -5,
        s: 1,
        want: 0
    ), new(
        previous: new byte[]{}.slice(),
        current: new byte[]{9, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: -1,
        s: 1,
        want: 0
    ), new(
        previous: new byte[]{}.slice(),
        current: new byte[]{2, 2, 2, 2, 1, 2, 3, 4, 5}.slice(),
        t: 0,
        s: 1,
        want: 3
    ), new(
        previous: new byte[]{3, 4, 5}.slice(),
        current: new byte[]{3, 4, 5}.slice(),
        t: -3,
        s: 0,
        want: 3
    ), new(
        previous: new slice<byte>(1000),
        current: new slice<byte>(1000),
        t: -1000,
        s: 0,
        want: maxMatchLength - 4
    ), new(
        previous: new slice<byte>(200),
        current: new slice<byte>(500),
        t: -200,
        s: 0,
        want: maxMatchLength - 4
    ), new(
        previous: new slice<byte>(200),
        current: new slice<byte>(500),
        t: 0,
        s: 1,
        want: maxMatchLength - 4
    ), new(
        previous: new slice<byte>(maxMatchLength - 4),
        current: new slice<byte>(500),
        t: -(maxMatchLength - 4),
        s: 0,
        want: maxMatchLength - 4
    ), new(
        previous: new slice<byte>(200),
        current: new slice<byte>(500),
        t: -200,
        s: 400,
        want: 100
    ), new(
        previous: new slice<byte>(10),
        current: new slice<byte>(500),
        t: 200,
        s: 400,
        want: 100
    )
    }.slice();
    foreach (var (i, c) in cases) {
        var e = new deflateFast(prev: c.previous);
        var got = e.matchLen(c.s, c.t, c.current);
        if (got != c.want) {
            Ꮡt.Errorf("Test %d: match length, want %d, got %d"u8, i, c.want, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newWriterˢ = "NewWriter: "u8;
internal static readonly @string writeˢ = "Write: "u8;
internal static readonly @string writerCloseˢ = "Writer.Close: "u8;
internal static readonly @string readAllˢ = "ReadAll: "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string abcᶜ = "abcdefgh"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string xyzᶜ = "stuvwxyz"u8;

public static void TestBestSpeedMaxMatchOffset(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    @string abc = abcᶜ;
    @string xyz = xyzᶜ;
    foreach (var (_, matchBefore) in new bool[]{false, true}.slice()) {
        foreach (var (_, extra) in new nint[]{0, inputMargin - 1, inputMargin, inputMargin + 1, 2 * inputMargin}.slice()) {
            for (nint offsetAdjᴛ1 = -5; offsetAdjᴛ1 <= +5; offsetAdjᴛ1++) {
                var offsetAdj = offsetAdjᴛ1;
                void report(@string desc, error errΔ1) {
                    Ꮡt.Errorf("matchBefore=%t, extra=%d, offsetAdj=%d: %s%v"u8,
                        matchBefore, extra, offsetAdj, desc, errΔ1);
                }
                nint offset = (nint)maxMatchOffset + offsetAdj;
                // Make src to be a []byte of the form
                //	"%s%s%s%s%s" % (abc, zeros0, xyzMaybe, abc, zeros1)
                // where:
                //	zeros0 is approximately maxMatchOffset zeros.
                //	xyzMaybe is either xyz or the empty string.
                //	zeros1 is between 0 and 30 zeros.
                // The difference between the two abc's will be offset, which
                // is maxMatchOffset plus or minus a small adjustment.
                var src = new slice<byte>(offset + len(abc) + extra);
                copy(src, abc);
                if (!matchBefore) {
                    copy(src.slice(offset - len(xyz)), xyz);
                }
                copy(src.slice(offset), abc);
                var buf = @new<bytes.Buffer>();
                var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(buf), BestSpeed);
                if (err != default!) {
                    report(newWriterˢ, err);
                    continue;
                }
                {
                    var (_, errΔ1) = w.Write(src); if (errΔ1 != default!) {
                        report(writeˢ, errΔ1);
                        continue;
                    }
                }
                {
                    var errΔ2 = w.Close(); if (errΔ2 != default!) {
                        report(writerCloseˢ, errΔ2);
                        continue;
                    }
                }
                var r = NewReader(new flate_test_package.bytes_BufferжReader(buf));
                (var dst, err) = io.ReadAll(r);
                r.Close();
                if (err != default!) {
                    report(readAllˢ, err);
                    continue;
                }
                if (!bytes.Equal(dst, src)) {
                    report(""u8, fmt.Errorf("bytes differ after round-tripping"u8));
                    continue;
                }
            }
        }
    }
}

public static void TestBestSpeedShiftOffsets(ж<testing.T> Ꮡt) {
    // Test if shiftoffsets properly preserves matches and resets out-of-range matches
    // seen in https://github.com/golang/go/issues/4142
    var enc = newDeflateFast();
    // testData may not generate internal matches.
    var testData = new slice<byte>(32);
    var rng = rand.New(rand.NewSource(0));
    foreach (var (i, _) in testData) {
        testData[i] = (byte)rng.Uint32();
    }
    // Encode the testdata with clean state.
    // Second part should pick up matches from the first block.
    nint wantFirstTokens = len(enc.encode(default!, testData));
    nint wantSecondTokens = len(enc.encode(default!, testData));
    if (wantFirstTokens <= wantSecondTokens) {
        Ꮡt.Fatalf("test needs matches between inputs to be generated"u8);
    }
    // Forward the current indicator to before wraparound.
    enc.Value.cur = (int32)bufferReset - (int32)len(testData);
    // Part 1 before wrap, should match clean state.
    nint got = len(enc.encode(default!, testData));
    if (wantFirstTokens != got) {
        Ꮡt.Errorf("got %d, want %d tokens"u8, got, wantFirstTokens);
    }
    // Verify we are about to wrap.
    if ((~enc).cur != bufferReset) {
        Ꮡt.Errorf("got %d, want e.cur to be at bufferReset (%d)"u8, (~enc).cur, (nint)(bufferReset));
    }
    // Part 2 should match clean state as well even if wrapped.
    got = len(enc.encode(default!, testData));
    if (wantSecondTokens != got) {
        Ꮡt.Errorf("got %d, want %d token"u8, got, wantSecondTokens);
    }
    // Verify that we wrapped.
    if ((~enc).cur >= bufferReset) {
        Ꮡt.Errorf("want e.cur to be < bufferReset (%d), got %d"u8, (nint)(bufferReset), (~enc).cur);
    }
    // Forward the current buffer, leaving the matches at the bottom.
    enc.Value.cur = bufferReset;
    enc.shiftOffsets();
    // Ensure that no matches were picked up.
    got = len(enc.encode(default!, testData));
    if (wantFirstTokens != got) {
        Ꮡt.Errorf("got %d, want %d tokens"u8, got, wantFirstTokens);
    }
}

public static partial void TestMaxStackSize(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // This test must not run in parallel with other tests as debug.SetMaxStack
        // affects all goroutines.
        nint n = debug.SetMaxStack((1 << (int)(16)));
        defer(debug.SetMaxStack, n, ref ᒐ);
        ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
        defer(Ꮡwg.Wait, ref ᒐ);
        var b = new slice<byte>((1 << (int)(20)));
        for (nint level = HuffmanOnly; level <= BestCompression; level++) {
            // Run in separate goroutine to increase probability of stack regrowth.
            Ꮡwg.Add(1);
            var bʗ1 = b;
            goǃ((nint levelΔ1) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    var (zw, err) = NewWriter(io.Discard, levelΔ1);
                    if (err != default!) {
                        Ꮡt.Errorf("level %d, NewWriter() = %v, want nil"u8, levelΔ1, err);
                    }
                    {
                        var (nΔ1, errΔ1) = zw.Write(bʗ1); if (nΔ1 != len(bʗ1) || errΔ1 != default!) {
                            Ꮡt.Errorf("level %d, Write() = (%d, %v), want (%d, nil)"u8, levelΔ1, nΔ1, errΔ1, len(bʗ1));
                        }
                    }
                    {
                        var errΔ2 = zw.Close(); if (errΔ2 != default!) {
                            Ꮡt.Errorf("level %d, Close() = %v, want nil"u8, levelΔ1, errΔ2);
                        }
                    }
                    zw.Reset(io.Discard);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, level);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end flate_internal_test_package
