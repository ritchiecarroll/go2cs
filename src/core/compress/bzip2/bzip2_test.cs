// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using testing = testing_package;
using encoding;
using static go.compress.bzip2_package;

partial class bzip2_internal_test_package {

internal static slice<byte> mustDecodeHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

internal static slice<byte> mustLoadFile(@string f) {
    var (b, err) = os.ReadFile(f);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

internal static @string trim(slice<byte> b) {
    const nint limit = 1024;
    if (len(b) < limit) {
        return fmt.Sprintf("%q"u8, b);
    }
    return fmt.Sprintf("%q..."u8, b[..(int)(limit)]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataPassRandom1Bz2ˢ = "testdata/pass-random1.bz2"u8;
internal static readonly @string testdataPassRandom1Binˢ = "testdata/pass-random1.bin"u8;
internal static readonly @string testdataPassRandom2Bz2ˢ = "testdata/pass-random2.bz2"u8;
internal static readonly @string testdataPassRandom2Binˢ = "testdata/pass-random2.bin"u8;
internal static readonly @string testdataPassSawtoothBz2ˢ = "testdata/pass-sawtooth.bz2"u8;
internal static readonly @string testdataFailIssue5747Bz2ˢ = "testdata/fail-issue5747.bz2"u8;

internal partial struct TestReader_type /*dyn*/ {
    internal @string desc;
    internal slice<byte> input;
    internal slice<byte> output;
    internal bool fail;
}

public static void TestReader(ж<testing.T> Ꮡt) {
    slice<TestReader_type> vectors = new TestReader_type[]{new(
        desc: "hello world"u8,
        input: mustDecodeHex(""u8 + "425a68393141592653594eece83600000251800010400006449080200031064c"u8 + "4101a7a9a580bb9431f8bb9229c28482776741b0"u8),
        output: slice<byte>("hello world\n"u8)
    ), new(
        desc: "concatenated files"u8,
        input: mustDecodeHex(""u8 + "425a68393141592653594eece83600000251800010400006449080200031064c"u8 + "4101a7a9a580bb9431f8bb9229c28482776741b0425a68393141592653594eec"u8 + "e83600000251800010400006449080200031064c4101a7a9a580bb9431f8bb92"u8 + "29c28482776741b0"u8),
        output: slice<byte>("hello world\nhello world\n"u8)
    ), new(
        desc: "32B zeros"u8,
        input: mustDecodeHex(""u8 + "425a6839314159265359b5aa5098000000600040000004200021008283177245"u8 + "385090b5aa5098"u8),
        output: new slice<byte>(32)
    ), new(
        desc: "1MiB zeros"u8,
        input: mustDecodeHex(""u8 + "425a683931415926535938571ce50008084000c0040008200030cc0529a60806"u8 + "c4201e2ee48a70a12070ae39ca"u8),
        output: new slice<byte>((1 << (int)(20)))
    ), new(
        desc: "random data"u8,
        input: mustLoadFile(testdataPassRandom1Bz2ˢ),
        output: mustLoadFile(testdataPassRandom1Binˢ)
    ), new(
        desc: "random data - full symbol range"u8,
        input: mustLoadFile(testdataPassRandom2Bz2ˢ),
        output: mustLoadFile(testdataPassRandom2Binˢ)
    ), new(
        desc: "random data - uses RLE1 stage"u8,
        input: mustDecodeHex(""u8 + "425a6839314159265359d992d0f60000137dfe84020310091c1e280e100e0428"u8 + "01099210094806c0110002e70806402000546034000034000000f28300000320"u8 + "00d3403264049270eb7a9280d308ca06ad28f6981bee1bf8160727c7364510d7"u8 + "3a1e123083421b63f031f63993a0f40051fbf177245385090d992d0f60"u8),
        output: mustDecodeHex(""u8 + "92d5652616ac444a4a04af1a8a3964aca0450d43d6cf233bd03233f4ba92f871"u8 + "9e6c2a2bd4f5f88db07ecd0da3a33b263483db9b2c158786ad6363be35d17335"u8 + "ba"u8)
    ), new(
        desc: "1MiB sawtooth"u8,
        input: mustLoadFile(testdataPassSawtoothBz2ˢ),
        output: ((Func<slice<byte>>)(() => {
            var b = new slice<byte>((1 << (int)(20)));
            foreach (var (i, _) in b) {
                b[i] = (byte)i;
            }
            return b;
        }))()
    ), new(
        desc: "RLE2 buffer overrun - issue 5747"u8,
        input: mustLoadFile(testdataFailIssue5747Bz2ˢ),
        fail: true
    ), new(
        desc: "out-of-range selector - issue 8363"u8,
        input: mustDecodeHex(""u8 + "425a68393141592653594eece83600000251800010400006449080200031064c"u8 + "4101a7a9a580bb943117724538509000000000"u8),
        fail: true
    ), new(
        desc: "bad block size - issue 13941"u8,
        input: mustDecodeHex(""u8 + "425a683131415926535936dc55330063ffc0006000200020a40830008b0008b8"u8 + "bb9229c28481b6e2a998"u8),
        fail: true
    ), new(
        desc: "bad huffman delta"u8,
        input: mustDecodeHex(""u8 + "425a6836314159265359b1f7404b000000400040002000217d184682ee48a70a"u8 + "12163ee80960"u8),
        fail: true
    )
    }.slice();
    foreach (var (i, v) in vectors) {
        var rd = NewReader(new bzip2_internal_test_package.bytes_ReaderжReader(bytes.NewReader(v.input)));
        var (buf, err) = io.ReadAll(rd);
        {
            var fail = (bool)(err != default!); if (fail != v.fail) {
                if (fail){
                    Ꮡt.Errorf("test %d (%s), unexpected failure: %v"u8, i, v.desc, err);
                } else {
                    Ꮡt.Errorf("test %d (%s), unexpected success"u8, i, v.desc);
                }
            }
        }
        if (!v.fail && !bytes.Equal(buf, v.output)) {
            Ꮡt.Errorf("test %d (%s), output mismatch:\ngot  %s\nwant %s"u8, i, v.desc, trim(buf), trim(v.output));
        }
    }
}

internal partial struct TestBitReader_type /*dyn*/ {
    internal nuint nbits; // Number of bits to read
    internal nint value; // Expected output value (0 for error)
    internal bool fail; // Expected operation failure?
}

public static void TestBitReader(ж<testing.T> Ꮡt) {
    slice<TestBitReader_type> vectors = new TestBitReader_type[]{
        new(nbits: 1, value: 1),
        new(nbits: 1, value: 0),
        new(nbits: 1, value: 1),
        new(nbits: 5, value: 11),
        new(nbits: 32, value: 0x12345678),
        new(nbits: 15, value: 14495),
        new(nbits: 3, value: 6),
        new(nbits: 6, value: 13),
        new(nbits: 1, fail: true)
    }.slice();
    var rd = bytes.NewReader(new byte[]{0xab, 0x12, 0x34, 0x56, 0x78, 0x71, 0x3f, 0x8d}.slice());
    var br = newBitReader(new bzip2_internal_test_package.bytes_ReaderжReader(rd));
    foreach (var (i, v) in vectors) {
        nint val = br.ReadBits(v.nbits);
        {
            var fail = (bool)(br.err != default!); if (fail != v.fail) {
                if (fail){
                    Ꮡt.Errorf("test %d, unexpected failure: ReadBits(%d) = %v"u8, i, v.nbits, br.err);
                } else {
                    Ꮡt.Errorf("test %d, unexpected success: ReadBits(%d) = nil"u8, i, v.nbits);
                }
            }
        }
        if (!v.fail && val != v.value) {
            Ꮡt.Errorf("test %d, mismatching value: ReadBits(%d) = %d, want %d"u8, i, v.nbits, val, v.value);
        }
    }
}

internal partial struct TestMTF_type /*dyn*/ {
    internal nint idx;  // Input index
    internal uint8 sym; // Expected output symbol
}

public static void TestMTF(ж<testing.T> Ꮡt) {
// [1 0 2 3 4]
// [1 0 2 3 4]
// [0 1 2 3 4]
// [4 0 1 2 3]
// [0 4 1 2 3]
    slice<TestMTF_type> vectors = new TestMTF_type[]{
        new(idx: 1, sym: 1),
        new(idx: 0, sym: 1),
        new(idx: 1, sym: 0),
        new(idx: 4, sym: 4),
        new(idx: 1, sym: 0)
    }.slice();
    var mtf = newMTFDecoderWithRange(5);
    foreach (var (i, v) in vectors) {
        var sym = mtf.Decode(v.idx);
        Ꮡt.Log(mtf);
        if (sym != v.sym) {
            Ꮡt.Errorf("test %d, symbol mismatch: Decode(%d) = %d, want %d"u8, i, v.idx, sym, v.sym);
        }
    }
}

public static void TestZeroRead(ж<testing.T> Ꮡt) {
    var b = mustDecodeHex("425a6839314159265359b5aa5098000000600040000004200021008283177245385090b5aa5098"u8);
    var r = NewReader(new bzip2_internal_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
    {
        var (n, err) = r.Read(default!); if (n != 0 || err != default!) {
            Ꮡt.Errorf("Read(nil) = (%d, %v), want (0, nil)"u8, n, err);
        }
    }
}

internal static slice<byte> digits = mustLoadFile("testdata/e.txt.bz2"u8);
internal static slice<byte> newton = mustLoadFile("testdata/Isaac.Newton-Opticks.txt.bz2"u8);
internal static slice<byte> random = mustLoadFile("testdata/random.data.bz2"u8);

internal static void benchmarkDecode(ж<testing.B> Ꮡb, slice<byte> compressed) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Determine the uncompressed size of testfile.
    var (uncompressedSize, err) = io.Copy(io.Discard, NewReader(new bzip2_internal_test_package.bytes_ReaderжReader(bytes.NewReader(compressed))));
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    b.SetBytes(uncompressedSize);
    b.ReportAllocs();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var r = bytes.NewReader(compressed);
        io.Copy(io.Discard, NewReader(new bzip2_internal_test_package.bytes_ReaderжReader(r)));
    }
}

public static void BenchmarkDecodeDigits(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, digits);
}

public static void BenchmarkDecodeNewton(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, newton);
}

public static void BenchmarkDecodeRand(ж<testing.B> Ꮡb) {
    benchmarkDecode(Ꮡb, random);
}

} // end bzip2_internal_test_package
