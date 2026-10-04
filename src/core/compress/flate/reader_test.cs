// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using io = io_package;
using os = os_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using static go.compress.flate_package;

partial class flate_internal_test_package {

public static void TestNlitOutOfRange(ж<testing.T> Ꮡt) {
    // Trying to decode this bogus flate data, which has a Huffman table
    // with nlit=288, should not panic.
    io.Copy(io.Discard, NewReader(new flate_test_package.strings_ReaderжReader(strings.NewReader(
        ((@string)(new byte[]{0xfc, 0xfe, 0x36, 0xe7, 0x5e, 0x1c, 0xef, 0xb3, 0x55, 0x58, 0x77, 0xb6, 0x56, 0xb5, 0x43, 0xf4})) + ((@string)(new byte[]{0x6f, 0xf2, 0xd2, 0xe6, 0x3d, 0x99, 0xa0, 0x85, 0x8c, 0x48, 0xeb, 0xf8, 0xda, 0x83, 0x04, 0x2a})) + ((@string)(new byte[]{0x75, 0xc4, 0xf8, 0x0f, 0x12, 0x11, 0xb9, 0xb4, 0x4b, 0x09, 0xa0, 0xbe, 0x8b, 0x91, 0x4c}))))));
}

// Digits is the digits of the irrational number e. Its decimal representation
// does not repeat, but there are only 10 possible digits, so it should be
// reasonably compressible.
// Newton is Isaac Newtons's educational text on Opticks.

[GoType("dyn")] partial struct suitesᴛ1 {
    internal @string name, @file;
}
internal static slice<suitesᴛ1> suites = new suitesᴛ1[]{
    new("Digits"u8, "../testdata/e.txt"u8),
    new("Newton"u8, "../../testdata/Isaac.Newton-Opticks.txt"u8)
}.slice();

public static void BenchmarkDecode(ж<testing.B> Ꮡb) {
    doBench(Ꮡb, (ж<testing.B> bΔ1, slice<byte> buf0, nint level, nint n) => {
        bΔ1.ReportAllocs();
        bΔ1.StopTimer();
        bΔ1.SetBytes((int64)n);
        var compressed = @new<bytes.Buffer>();
        var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(compressed), level);
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        for (nint i = 0; i < n; i += len(buf0)) {
            if (len(buf0) > n - i) {
                buf0 = buf0.slice(0, n - i);
            }
            io.Copy(new flate_test_package.flate_WriterжWriter(w), new flate_test_package.bytes_Readerжio_Reader(bytes.NewReader(buf0)));
        }
        w.Close();
        var buf1 = compressed.Bytes();
        (buf0, compressed, w) = (default!, default!, default!);
        runtime.GC();
        bΔ1.StartTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            io.Copy(io.Discard, NewReader(new flate_test_package.bytes_Readerжio_Reader(bytes.NewReader(buf1))));
        }
    });
}


[GoType("dyn")] partial struct levelTestsᴛ1 {
    internal @string name;
    internal nint level;
}
internal static slice<levelTestsᴛ1> levelTests = new levelTestsᴛ1[]{
    new("Huffman"u8, HuffmanOnly),
    new("Speed"u8, BestSpeed),
    new("Default"u8, DefaultCompression),
    new("Compression"u8, BestCompression)
}.slice();


[GoType("dyn")] partial struct sizesᴛ1 {
    internal @string name;
    internal nint n;
}
internal static slice<sizesᴛ1> sizes = new sizesᴛ1[]{
    new("1e4"u8, 10000),
    new("1e5"u8, 100000),
    new("1e6"u8, 1000000)
}.slice();

internal static void doBench(ж<testing.B> Ꮡb, Action<ж<testing.B>, slice<byte>, nint, nint> f) {
    foreach (var (_, suite) in suites) {
        var (buf, err) = os.ReadFile(suite.@file);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        if (len(buf) == 0) {
            Ꮡb.Fatalf("test file %q has no data"u8, suite.@file);
        }
        foreach (var (_, vᴛ1) in levelTests) {
            ref var l = ref heap(new levelTestsᴛ1(), out var Ꮡl);
            l = vᴛ1;

            foreach (var (_, vᴛ2) in sizes) {
                ref var s = ref heap(new sizesᴛ1(), out var Ꮡs);
                s = vᴛ2;

                var bufʗ1 = buf;
                var lʗ1 = l;
                var sʗ1 = s;
                Ꮡb.Run(suite.name + "/"u8 + l.name + "/"u8 + s.name, (ж<testing.B> bΔ1) => {
                    f(bΔ1, bufʗ1, lʗ1.level, sʗ1.n);
                });
            }
        }
    }
}

} // end flate_internal_test_package
